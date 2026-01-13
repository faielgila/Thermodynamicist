using Core.Data;
using Core.VariableTypes;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Core.Multicomponent;

/// <summary>
/// Represents a closed system with multiple phases present.
/// Each phase is assumed to be a homogeneous mixture (as implemented in <see cref="HomogeneousMixture"/>).
/// </summary>
// TODO: Nonbinary systems require multiple species mole fractions to specify the state of the system.
public class MultiphaseSystem
{
	/// <summary>
	/// Represents total system mole fractions for all species in the system.
	/// </summary>
	public CompositionVector compositionVector;

	/// <summary>
	/// List of all species in the system.
	/// </summary>
	public List<Chemical> SpeciesList { get; }

	/// <summary>
	/// List of homogeneous mixtures which comprise the system.
	/// Each mixture represents a single phase in the system.
	/// </summary>
	public List<HomogeneousMixture> mixtureList;

	/// <summary>
	/// Species to use as a basis. A specification for mole fraction
	/// for this species is not required and will be back-calculated from all
	/// other species. Unless overriden, species basis will be set to whichever
	/// is first in speciesList.
	/// For example, a mixture of water and ethanol could use water
	/// as the species basis, where the ethanol mole fraction is treated
	/// as an independent variable and water is treated as dependent.
	/// </summary>
	public Chemical SpeciesBasis { get; set; }

	/// <summary>
	/// List of non-basis chemicals in the system.
	/// </summary>
	private List<Chemical> SpeciesListNonBasis;

	/// <summary>
	/// True if there are only two species in the system.
	/// </summary>
	bool IsBinarySystem { get; set; }

	/// <summary>
	/// Lists all phases present in the system.
	/// </summary>
	public List<string> PhasesList { get; }

	/// <summary>
	/// Stores all calculated chemical potential curves for each species at various temperatures, pressures, and phases.
	/// </summary>
	public ChemicalPotentialCurvesLibrary curvesLibChemicalPotential = [];

	/// <summary>
	/// Stores all calculated Gibbs energy curves for each phase at various temperatures and pressures.
	/// </summary>
	public PhaseTotalGibbsEnergyCurvesLibrary curvesLibPhaseTotalGibbsEnergy = [];

	public Dictionary<MoleFraction, double> LastPhaseEquilibriaErrors = [];

	/// <summary>
	/// Creates a multiphase system.
	/// </summary>
	/// <param name="_compositionList">Composition vector of all species mole fractions.</param>
	/// <param name="_mixtureList">List of homogeneous mixtures, one for each phase in the system.</param>
	/// <param name="_basis">Species to treat as the derived composition.</param>
	public MultiphaseSystem(CompositionVector _compositionList, List<HomogeneousMixture> _mixtureList, Chemical _basis)
	{
		SpeciesList = [.. _compositionList.dict.Keys];

		if (SpeciesList.Count < 2 || _mixtureList.Count < 2)
			throw new NotSupportedException("Systems must contain more than one species and phase.");

		compositionVector = _compositionList;
		mixtureList = _mixtureList;
		SpeciesBasis = _basis;
		SpeciesListNonBasis = [.. from spec in SpeciesList
								  where spec != SpeciesBasis
								  select spec];
		IsBinarySystem = (SpeciesList.Count == 2);
		PhasesList = [.. mixtureList.Select(mix => mix.totalPhase)];

		// Validate compositions in mixture species.
		// If any of the mixture mole fractions are not set, skip this part.
		if (mixtureList.Select(mix => mix.mixtureMoleFraction).Any(x => x is null)) return;
		Dictionary<Chemical, MoleFraction> aggregateSpecies = [];
		foreach (var homoMix in mixtureList)
		{
			foreach (var mixtureSpecies in homoMix.speciesList)
			{
				var val = mixtureSpecies.speciesMoleFraction * homoMix.mixtureMoleFraction;
				if (aggregateSpecies.ContainsKey(mixtureSpecies.chemical))
				{
					aggregateSpecies[mixtureSpecies.chemical] += val;
				} else
				{
					aggregateSpecies.Add(mixtureSpecies.chemical, val);
				}
			}
		}
		if (aggregateSpecies.Any(entry => compositionVector.dict[entry.Key] != entry.Value))
		{
			throw new ArgumentException("Mixture species compositions do not add to system composition.");
		}
	}

	/// <summary>
	/// Finds and lists all phase equilibria at a given temperature and pressure
	/// given the overall system composition in mixtureList.
	/// WARNING: This is a very intensive process. Use only when necessary.
	/// </summary>
	// TODO: Optimize search algorithm. Potential spaces for improvent are with searching
	// for common tangents and narrowing down obviously impossible equilibria. Or make a separate
	// and smarter method which uses CALPHAD.
	// TODO: Use SpeciesBasis instead of assuming all fractions are for species0.
	public List<MultiphaseEquilibriumResult> FindPhaseEquilibria(Temperature T, Pressure P)
	{
		if (!IsBinarySystem) throw new NotImplementedException("Multicomponent equilibrium currently only supports binary mixtures.");
		if (PhasesList.Count > 2) throw new NotImplementedException("Multicomponent equilibrium currently only supports two phases.");

		var errorTolerance = 0.02;

		var searchDensity = 0.01;
		var compositions = new LinearEnumerable(0.0001, 1, searchDensity).ToList();
		compositions.Add(0.9999);
		foreach (var phase in PhasesList)
		{
			PopulateCurveLibraries(T, P, phase, compositions);
		}
		LastPhaseEquilibriaErrors = [];


		// Run a roughing search across all compositions.
		Console.WriteLine("== Performing rough search across all compositions ==");
		// All compositions (e.g. xV) are mole fractions of the non-basis species.
		// For a binary mixture, this is really easy.
		List<MoleFraction> roughing = [];
		List<MoleFraction> possibleStates = [];
		for (MoleFraction xV = 0.01; xV <= 1; xV += searchDensity)
		{
			(MoleFraction xL, double error) = SearchForCommonTangets(xV);
			if (double.IsNaN(error))
			{
				Console.WriteLine($"No state found at x0V={xV.RoundToSigfigs(3)}");
				continue;
			}
			possibleStates.Add(xV);
			LastPhaseEquilibriaErrors.Add(xV, error);
			Console.WriteLine($"State found at x0V={xV.RoundToSigfigs(3)} with error {error}");
			if (error <= errorTolerance*2)
			{
				roughing.Add(xV);
				Console.WriteLine($"Roughing candidate found at x0V={xV.RoundToSigfigs(3)} x0L={xL.RoundToSigfigs(3)}");
			}
		}

		// If ther are no possible states, return empty list.
		if (roughing.Count == 0) return [];

		// Include roughing near edges of the 'no state found' region.
		roughing.Add(possibleStates.Min());
		roughing.Add(possibleStates.Max());

		// Run a final search near the roughing solutions.
		Dictionary<(MoleFraction xV, MoleFraction xL), double> resultsPrelim = [];
		var searchRadius = 0.02;
		searchDensity /= 10;
		var finalSearchSpace = new MoleFractionSearchRanges(roughing, searchRadius);
		foreach (var xVr in finalSearchSpace.Ranges)
		{
			Console.WriteLine($"== Performing final search for xV = [{xVr.Value.min.RoundToSigfigs(3)}, {xVr.Value.max.RoundToSigfigs(3)}] ==");

			// Beware of NaN states!
			// If the range being tested extends beyond the 'no state found' region,
			// you'll need to find exactly where the edge of that region is.
			var compositionsV = new LinearEnumerable(xVr.Value.min, xVr.Value.max, searchDensity).ToList();
			PopulateCurveLibraries(T, P, "vapor", compositionsV);
			MoleFraction xL_fromMin = double.NaN;
			foreach (var xVtest in compositionsV)
			{
				(var xL, _) = SearchForCommonTangets(xVtest);
				if (double.IsNaN(xL))
				{
					continue;
				} else {
					xL_fromMin = xL;
					break;
				}
			}
			MoleFraction xL_fromMax = double.NaN;
			compositionsV.Reverse();
			foreach (var xVtest in compositionsV)
			{
				(var xL, _) = SearchForCommonTangets(xVtest);
				if (double.IsNaN(xL))
				{
					continue;
				}
				else
				{
					xL_fromMax = xL;
					break;
				}
			}
			var compositionsL = new LinearEnumerable(Math.Min(xL_fromMin, xL_fromMax), Math.Max(xL_fromMin, xL_fromMax), searchDensity).ToList();
			PopulateCurveLibraries(T, P, "liquid", compositionsL);
			Console.WriteLine($"Determined search range for xL to be between {xL_fromMin} and {xL_fromMax}");

			(MoleFraction xV, MoleFraction xL, double error) previousResult = (double.NaN, double.NaN, double.NaN);
			for (MoleFraction xV = xVr.Value.min; xV <= xVr.Value.max; xV += searchDensity)
			{
				(MoleFraction xL, double error) = SearchForCommonTangets(xV);
				if (double.IsNaN(xL)) continue;
				Console.WriteLine($"State found at x0V={xV.RoundToSigfigs(3)} with error {error}");
				LastPhaseEquilibriaErrors.Add(xV, error);

				// Check to ensure error is below accepted tolerance.
				// Necesary to avoid always finding solutions at the no-state boundaries.
				if (error > errorTolerance)
				{
					Console.WriteLine($"State ignored at x0V={xV.RoundToSigfigs(3)} with error {error}");
					continue;
				}
				
				// If the error has decreased, replace the previous preliminary result with the current result.
				if (error < previousResult.error)
				{
					resultsPrelim.Remove((previousResult.xV, previousResult.xL));
					resultsPrelim.Add((xV, xL), error);
					Console.WriteLine($"Better final equilibrium found at x0V={xV.RoundToSigfigs(3)} x0L={xL.RoundToSigfigs(3)} with error {error}");
				}
				else if (error == previousResult.error)
				{
					resultsPrelim.Remove((previousResult.xV, previousResult.xL));
					xV = (xV + previousResult.xV) / 2;
					(xL, error) = SearchForCommonTangets(xV);
					resultsPrelim.Add((xV, xL), error);
					LastPhaseEquilibriaErrors.Add(xV, error);
					Console.WriteLine($"Equivalent final equilibrium found at x0V={xV.RoundToSigfigs(3)} x0L={xL.RoundToSigfigs(3)} with error {error}");
				}

				// Store this current state for future use.
				previousResult = (xV, xL, error);
			}
		}

		// Search complete. Any remaining preliminary results are now final results.
		List<MultiphaseEquilibriumResult> results = [];
		foreach (var entry in resultsPrelim)
		{
			var xV = entry.Key.xV;
			var xL = entry.Key.xL;
			var equilibrium = new MultiphaseEquilibriumResult(T, P);
			equilibrium.Value.Add((PhasesList[0], SpeciesListNonBasis[0]), xV);
			equilibrium.Value.Add((PhasesList[0], SpeciesBasis), 1 - xV);
			equilibrium.Value.Add((PhasesList[1], SpeciesListNonBasis[0]), xL);
			equilibrium.Value.Add((PhasesList[1], SpeciesBasis), 1 - xL);
			results.Add(equilibrium);
		}
		LastPhaseEquilibriaErrors.ToList();
		return results;

		(MoleFraction xL, double error) SearchForCommonTangets(MoleFraction xV)
		{
			var curves_V = curvesLibChemicalPotential.GetCurves(T, P, PhasesList[0]);
			var curves_L = curvesLibChemicalPotential.GetCurves(T, P, PhasesList[1]);
			var curvePotential_V0 = curves_V[SpeciesListNonBasis[0]];
			var curvePotential_V1 = curves_V[SpeciesBasis];
			var curveComposition_L0 = curves_L[SpeciesListNonBasis[0]].Invert();
			var curveComposition_L1 = curves_L[SpeciesBasis].Invert();

			var mu_V0 = curvePotential_V0.GetValue(xV);
			var mu_V1 = curvePotential_V1.GetValue(xV);
			var xL_from0 = curveComposition_L0.GetValue(mu_V0);
			var xL_from1 = curveComposition_L1.GetValue(mu_V1);
			if (mu_V0 is null || xL_from0 is null)
			{
				Console.WriteLine($"Common tangent not possible for x0V={xV}");
				return (double.NaN, double.NaN);
			}
			if (mu_V1 is null || xL_from1 is null)
			{
				Console.WriteLine($"Common tangent not possible for x0V={xV}");
				return (double.NaN, double.NaN);
			}
			var error = Math.Abs(xL_from0 - xL_from1);
			var xL = (xL_from0 + xL_from1) / 2;
			return (xL, error);
		}
	}

	/// <summary>
	/// Calculates the chemical potential curves for all species in the given phase
	/// at the specified temperature and pressure.
	/// </summary>
	public void PopulateCurveLibraries(Temperature T, Pressure P, string phase, List<double> compositions)
	{
		// Check for already calculated curves.
		//foreach (var entry in speciesList)
		//{
		//	var state = new MultiphaseStatePoint(entry.Key, phase, T, P);
		//	if (curvesLibChemicalPotential.ContainsKey(state)) { return; }
		//}

		var state0 = new MultiphaseStatePoint(SpeciesListNonBasis[0], phase, T, P);
		var state1 = new MultiphaseStatePoint(SpeciesBasis, phase, T, P);
		var phaseCurve0 = new ConcurrentDictionary<MoleFraction, ChemicalPotential>();
		var phaseCurve1 = new ConcurrentDictionary<MoleFraction, ChemicalPotential>();
		var energyCurve = new ConcurrentDictionary<MoleFraction, GibbsEnergy>();

		Console.WriteLine($"Calculating chemical potential curves for phase \"{phase}\"");
		Parallel.ForEach(compositions, x => {
			var speciesList_local = new List<MixtureSpecies>
			{
				new(state0.species, x, state0.phase),
				new(state1.species, 1-x, state1.phase)
			};
			var activityModel_local = mixtureList[GetMixutureIdxFromPhase(phase)].activityModel.Copy();
			activityModel_local.speciesList = speciesList_local;
			var homomix_local = new HomogeneousMixture(speciesList_local, phase, activityModel_local, null);

			var mu0 = homomix_local.SpeciesChemicalPotential(T, P, state0.species);
			var mu1 = homomix_local.SpeciesChemicalPotential(T, P, state1.species);
			var G = homomix_local.TotalMolarGibbsEnergy(T, P);
			// Note the use of composition 'x' for both species.
			// TODO: This should be replaced by a full vector of the composition when non-binary systems are implemented.
			phaseCurve0.TryAdd(x, mu0);
			phaseCurve1.TryAdd(x, mu1);
			energyCurve.TryAdd(x, G);

			Console.WriteLine($"Calculated chemical potentials at x0={x}");
		});

		var phaseTable0 = new ChemicalPotentialCurve(state0, phaseCurve0);
		var phaseTable1 = new ChemicalPotentialCurve(state1, phaseCurve1);
		var energyTable = new PhaseTotalGibbsEnergyCurve((phase, T, P), energyCurve);
		phaseTable0.Resort();
		phaseTable1.Resort();
		energyTable.Resort();

		if (curvesLibChemicalPotential.ContainsKey(state0))
		{
			curvesLibChemicalPotential.GetCurve(state0).Append(phaseTable0);
			curvesLibChemicalPotential.GetCurve(state0).Resort();
		}
		else
		{
			curvesLibChemicalPotential.Add(state0, phaseTable0);
		}

		if (curvesLibChemicalPotential.ContainsKey(state1))
		{
			curvesLibChemicalPotential.GetCurve(state1).Append(phaseTable1);
			curvesLibChemicalPotential.GetCurve(state1).Resort();
		}
		else
		{
			curvesLibChemicalPotential.Add(state1, phaseTable1);
		}

		if (curvesLibPhaseTotalGibbsEnergy.ContainsKey((T, P, phase)))
		{
			curvesLibPhaseTotalGibbsEnergy[(T, P, phase)].Append(energyTable);
			curvesLibPhaseTotalGibbsEnergy[(T, P, phase)].Resort();
		}
		else
		{
			curvesLibPhaseTotalGibbsEnergy.Add((T, P, phase), energyTable);
		}
		
	}

	/// <summary>
	/// Gets the homogeneous mixture from the list with the matching phase.
	/// </summary>
	private int GetMixutureIdxFromPhase(string phase)
	{
		foreach (var mix in mixtureList)
		{
			if (mix.totalPhase == phase) return mixtureList.IndexOf(mix);
		}
		throw new KeyNotFoundException($"Mixture with phase \'{phase}\' not found in mixtureList.");
	}
}


/// <summary>
/// Stores a state point for the system according to phase, species, temperature, and pressure.
/// </summary>
public class MultiphaseStatePoint(Chemical _species, string _phase, Temperature _T, Pressure _P)
{
	public Chemical species = _species;
	public string phase = _phase;
	public Temperature T = _T;
	public Pressure P = _P;

	public bool Equals(MultiphaseStatePoint compare)
	{
		if (compare.species == species &&
			compare.phase == phase &&
			compare.T.Value == T &&
			compare.P.Value == P)
			return true;
		else return false;
	}
}

/// <summary>
/// Represents a result from equilibrium searches.
/// Stores temperature, pressure, and a Dictionary containing mole fractions for each species in each phase.
/// </summary>
public struct MultiphaseEquilibriumResult
{
	public Temperature T;
	public Pressure P;
	public Dictionary<(string phase, Chemical species), MoleFraction> Value;

	public MultiphaseEquilibriumResult(Temperature _T, Pressure _P, Dictionary<(string phase, Chemical species), MoleFraction> _value)
	{
		T = _T;
		P = _P;
		Value = _value;
	}

	public MultiphaseEquilibriumResult(Temperature _T, Pressure _P)
	{
		T = _T;
		P = _P;
		Value = [];
	}

	public static implicit operator Dictionary<(string phase, Chemical species), MoleFraction>(MultiphaseEquilibriumResult result) => result.Value;
}

/// <summary>
/// Represents a chemical potential-composition curve as <see cref="InterpolableTable{MoleFraction, ChemicalPotential}"/>.
/// </summary>
public class ChemicalPotentialCurve : InterpolableTable<MoleFraction, ChemicalPotential>
{
	public MultiphaseStatePoint state;

	public ChemicalPotentialCurve() { headers = ("mole fraction", "chemical potential"); }

	public ChemicalPotentialCurve(MultiphaseStatePoint _state, Dictionary<MoleFraction, ChemicalPotential> data) : base(data)
	{
		state = _state;
		headers = ("mole fraction", "chemical potential");
	}

	public ChemicalPotentialCurve(MultiphaseStatePoint _state, ConcurrentDictionary<MoleFraction, ChemicalPotential> data) : base(data.ToDictionary(x => x.Key, x => x.Value))
	{
		state = _state;
		headers = ("mole fraction", "chemical potential");
	}
}

/// <summary>
/// Represents a Gibbs energy-composition curve for a specific phase as <see cref="InterpolableTable{MoleFraction, GibbsEnergy}"/>.
/// </summary>
public class PhaseTotalGibbsEnergyCurve : InterpolableTable<MoleFraction, GibbsEnergy>
{
	public (string phase, Temperature T, Pressure P) state;

	public PhaseTotalGibbsEnergyCurve() { headers = ("mole fraction", "total molar Gibbs energy"); }

	public PhaseTotalGibbsEnergyCurve((string phase, Temperature T, Pressure P) _state, Dictionary<MoleFraction, GibbsEnergy> data) : base(data)
	{
		state = _state;
		headers = ("mole fraction", "total molar Gibbs energy");
	}

	public PhaseTotalGibbsEnergyCurve((string phase, Temperature T, Pressure P) _state, ConcurrentDictionary<MoleFraction, GibbsEnergy> data) : base(data.ToDictionary(x => x.Key, x => x.Value))
	{
		state = _state;
		headers = ("mole fraction", "total molar Gibbs energy");
	}
}

/// <summary>
/// Stores all chemical potential curves for a given multiphase system in a Dictionary.
/// Extends Dictionary<> with special functions relevant for selecting curves.
/// </summary>
public class ChemicalPotentialCurvesLibrary : Dictionary<MultiphaseStatePoint, ChemicalPotentialCurve>
{
	public new bool ContainsKey(MultiphaseStatePoint state)
	{
		foreach (var kvp in this)
		{
			if (kvp.Key.phase != state.phase) continue;
			if (kvp.Key.species != state.species) continue;
			if (kvp.Key.T != state.T) continue;
			if (kvp.Key.P != state.P) continue;

			return true;
		}
		return false;
	}

	public ChemicalPotentialCurve GetCurve(MultiphaseStatePoint state)
	{
		foreach (var kvp in this)
		{
			if (kvp.Key.phase != state.phase) continue;
			if (kvp.Key.species != state.species) continue;
			if (kvp.Key.T != state.T) continue;
			if (kvp.Key.P != state.P) continue;

			return kvp.Value;
		}
		throw new KeyNotFoundException("Chemial potential curve for the given state not present.");
	}

	/// <summary>
	/// Retrieves curves for a phase at the given temperature and pressure.
	/// </summary>
	public Dictionary<Chemical, ChemicalPotentialCurve> GetCurves(Temperature T, Pressure P, string phase)
	{
		Dictionary<Chemical, ChemicalPotentialCurve> results = [];
		foreach (var kvp in this)
		{
			if (kvp.Key.phase != phase) continue;
			if (kvp.Key.T != T) continue;
			if (kvp.Key.P != P) continue;

			if (!results.ContainsKey(kvp.Key.species)) results.Add(kvp.Key.species, kvp.Value);
		}
		return results;
	}

	/// <summary>
	/// Retrieves curves for a chemical at the given temperature and pressure.
	/// </summary>
	public Dictionary<string, ChemicalPotentialCurve> GetCurves(Temperature T, Pressure P, Chemical chemical)
	{
		Dictionary<string, ChemicalPotentialCurve> results = [];
		foreach (var kvp in this)
		{
			if (kvp.Key.species != chemical) continue;
			if (kvp.Key.T != T) continue;
			if (kvp.Key.P != P) continue;

			if (!results.ContainsKey(kvp.Key.phase)) results.Add(kvp.Key.phase, kvp.Value);
		}
		return results;
	}

	/// <summary>
	/// Converts every curve in the library to a CSV-formatted string.
	/// </summary>
	public Dictionary<MultiphaseStatePoint, string> ConvertToCSV()
	{
		Dictionary<MultiphaseStatePoint, string> dict = [];
		foreach (var kvp in this)
		{
			dict.Add(kvp.Key, kvp.Value.ToCSVString());
		}
		return dict;
	}
}

/// <summary>
/// Stores all total Gibbs energy curves for a given multiphase system in a Dictionary.
/// Extends Dictionary<> with special functions relevant for selecting curves.
/// </summary>
public class PhaseTotalGibbsEnergyCurvesLibrary : Dictionary<(Temperature T, Pressure P, string phase), PhaseTotalGibbsEnergyCurve>
{
	/// <summary>
	/// Converts every curve in the library to a CSV-formatted string.
	/// </summary>
	public Dictionary<(Temperature T, Pressure P, string phase), string> ConvertToCSV()
	{
		Dictionary<(Temperature T, Pressure P, string phase), string> dict = [];
		foreach (var kvp in this)
		{
			dict.Add(kvp.Key, kvp.Value.ToCSVString());
		}
		return dict;
	}
}

struct MoleFractionSearchRanges
{
	public Dictionary<Int32, (MoleFraction min, MoleFraction max)> Ranges { private set; get; }

	public MoleFractionSearchRanges(List<(MoleFraction min, MoleFraction max)> minmaxRanges)
	{
		UnifyRanges(minmaxRanges);
	}

	public MoleFractionSearchRanges(List<MoleFraction> _centerRanges, MoleFraction radius)
	{
		List<(MoleFraction min, MoleFraction max)> minmaxRanges = [];
		foreach (var x in _centerRanges)
		{
			minmaxRanges.Add((Math.Max(x-radius, 0), Math.Min(x+radius, 1)));
		}
		UnifyRanges(minmaxRanges);
	}

	/// <summary>
	/// Converts a potentially overlapping set of ranges in minmaxRanges
	/// to be the union of those ranges.
	/// </summary>
	private void UnifyRanges(List<(MoleFraction min, MoleFraction max)> minmaxRanges)
	{
		Ranges = [];
		foreach (var x in minmaxRanges)
		{
			if (!Ranges.Any())
			{
				Ranges.Add(GetHashcode(x), x);
				continue;
			}

			// Get all s such that x is entirely contained in s.
			var range_contained = (from s in Ranges
								   where s.Value.min <= x.min && s.Value.max >= x.max
								   select s).ToArray();
			if (range_contained.Length != 0)
			{
				continue;
			}

			// Get all s such that s is entirely contained in x.
			var range_contains = (from s in Ranges
								  where s.Value.min >= x.min && s.Value.max <= x.max
								  select s).ToArray();
			if (range_contains.Length != 0)
			{
				var s = range_contains[0];
				Ranges.Remove(s.Key);
				Ranges.Add(GetHashcode(x), x);
				continue;
			}

			// Get all s such that only the 'top' portion of s is contained in x.
			var range_left = (from s in Ranges
							  where s.Value.min <= x.min && x.min <= s.Value.max && s.Value.max <= x.max
							  select s).ToArray();
			if (range_left.Length != 0)
			{
				var s = range_left[0];
				var r = (s.Value.min, x.max);
				Ranges.Remove(s.Key);
				Ranges.Add(GetHashcode(r), r);
				continue;
			}

			// Get all s such that only the 'bottom' portion of s is contained in x.
			var range_right = (from s in Ranges
							   where s.Value.max >= x.max && x.min <= s.Value.min && s.Value.min <= x.max
							   select s).ToArray();
			if (range_right.Length != 0)
			{
				var s = range_right[0];
				var r = (x.min, s.Value.max);
				Ranges.Remove(s.Key);
				Ranges.Add(GetHashcode(r), r);
				continue;
			}

			// All other cases are such that s and x have no intersection.
			Ranges.Add(GetHashcode(x), x);
		}
	}

	/// <summary>
	/// Adds a min-max defined range and unifies it with the existing set of ranges.
	/// </summary>
	public void AddRange(MoleFraction min, MoleFraction max)
	{
		var x = (min, max);
		Ranges.Add(GetHashcode(x), x);
		UnifyRanges(Ranges.Values.ToList());
	}

	private Int32 GetHashcode((MoleFraction min, MoleFraction max) x)
	{
		return (x.min.GetHashCode() << 32) + x.max.GetHashCode();
	}
}