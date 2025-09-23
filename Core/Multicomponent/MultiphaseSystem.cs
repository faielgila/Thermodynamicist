using Core.Data;
using Core.VariableTypes;
using System;
using System.Collections.Concurrent;

namespace Core.Multicomponent;

/// <summary>
/// Represents a closed system with multiple phases present.
/// Each phase is assumed to be a homogeneous mixture (as implemented in <see cref="HomogeneousMixture"/>).
/// </summary>
// Nonbinary systems require multiple species mole fractions to specify the state of the system.
// TODO: Refactor use of MoleFraction to Dictionary<Chemical, MoleFraction>.
public class MultiphaseSystem
{
	public Dictionary<Chemical, MoleFraction> speciesList;

	public List<HomogeneousMixture> mixtureList;

	/// <summary>
	/// Species to use as a basis; for example, a specification for mole fraction
	/// for this species is not required and will be back-calculated from all
	/// other species. Unless overriden, species basis will be set to whichever
	/// is first in speciesList.
	/// </summary>
	public Chemical SpeciesBasis { get; set; }

	/// <summary>
	/// True if there are only two species in the system.
	/// </summary>
	bool IsBinarySystem { get; set; }

	/// <summary>
	/// Lists all phases present in the system.
	/// </summary>
	List<string> SystemPhases { get; }

	/// <summary>
	/// Stores all calculated chemical potential curves for each species at various temperatures, pressures, and phases.
	/// </summary>
	public Dictionary<MultiphaseStatePoint, ChemicalPotentialCurve> chemicalPotentialCurves = [];

	/// <summary>
	/// Stores all calculated Gibbs energy curves for each phase at various temperatures and pressures.
	/// </summary>
	public Dictionary<(string phase, Temperature T, Pressure P), PhaseTotalGibbsEnergyCurve> totalGibbsEnergyCurves = [];

	public List<(MoleFraction, double)> Error;

	public MultiphaseSystem(Dictionary<Chemical, MoleFraction> _speciesList, List<HomogeneousMixture> _mixtureList)
	{
		if (_speciesList.Count < 2 || _mixtureList.Count < 2)
			throw new NotSupportedException("Systems must contain more than one species and phase.");
		speciesList = _speciesList;
		mixtureList = _mixtureList;
		SpeciesBasis = speciesList.Keys.FirstOrDefault();
		IsBinarySystem = (speciesList.Count == 2);

		SystemPhases = [];
		foreach (var mixture in mixtureList) SystemPhases.Add(mixture.totalPhase);
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
		if (SystemPhases.Count > 2) throw new NotImplementedException("Multicomponent equilibrium currently only supports two phases.");

		var searchDensity = 0.01;
		var compositions = new LinearEnumerable(0.01, 1, searchDensity).ToList();
		foreach (var phase in SystemPhases)
		{
			CalculatePotentialAndEnergyCurves(T, P, phase, compositions);
		}


		// Run a roughing search across all compositions.

		// All compositions are mol% species0.
		// TODO: Add detection for which species the user wants to use as the basis.
		// TODO: Use for loop here for nonbinary mixtures
		List<MoleFraction> roughing = [];
		//List<MultiphaseEquilibriumResult> results = [];
		Error = [];
		for (MoleFraction xV = 0.01; xV <= 1; xV += searchDensity)
		{
			//Console.WriteLine($"Testing equilibrium for x0V={xV}");

			var species0 = speciesList.Keys.ToList()[0];
			var species1 = speciesList.Keys.ToList()[1];
			var curves_V = GetChemicalPotentialCurves(T, P, "vapor");
			var curves_L = GetChemicalPotentialCurves(T, P, "liquid");
			var curvePotential_V0 = curves_V[species0];
			var curvePotential_V1 = curves_V[species1];
			var curveComposition_L0 = curves_L[species0].Invert();
			var curveComposition_L1 = curves_L[species1].Invert();

			var mu_V0 = curvePotential_V0.GetValue(xV);
			var mu_V1 = curvePotential_V1.GetValue(xV);
			var xL_from0 = curveComposition_L0.GetValue(mu_V0);
			var xL_from1 = curveComposition_L1.GetValue(mu_V1);
			if (mu_V0 is null || xL_from0 is null)
			{
				//Console.WriteLine($"Common tangent not possible for x0V={xV}");
				continue;
			}
			if (mu_V1 is null || xL_from1 is null)
			{
				//Console.WriteLine($"Common tangent not possible for x0V={xV}");
				continue;
			}

			var error = Math.Abs(xL_from0 - xL_from1);
			Error.Add((xV, error));
			Console.WriteLine($"State found at x0V={xV} with error {error}");
			if (error <= 0.01)
			{
				var xL = (xL_from0 + xL_from1) / 2;
				var equilibrium = new MultiphaseEquilibriumResult();
				equilibrium.Value.Add(("vapor", species0), xV);
				equilibrium.Value.Add(("vapor", species1), 1-xV);
				equilibrium.Value.Add(("liquid", species0), xL);
				equilibrium.Value.Add(("liquid", species1), 1-xL);
				roughing.Add(xV);
				//results.Add(equilibrium);
				Console.WriteLine($"Roughing candidate found at x0V={xV} x0L={xL}");
			}
		}


		// Run a final search near the roughing solutions.
		List<MultiphaseEquilibriumResult> results = [];
		List<MoleFraction> resultFractions = [];
		var searchRadius = 0.01;
		searchDensity /= 10;
		foreach (var xVr in roughing)
		{
			compositions = new LinearEnumerable(xVr - searchRadius, xVr + searchRadius, searchDensity).ToList();
			foreach (var phase in SystemPhases)
			{
				CalculatePotentialAndEnergyCurves(T, P, phase, compositions);
			}

			for (MoleFraction xV = xVr - searchRadius; xV <= xVr + searchRadius; xV += searchDensity)
			{
				//Console.WriteLine($"Testing equilibrium for x0V={xV}");

				var species0 = speciesList.Keys.ToList()[0];
				var species1 = speciesList.Keys.ToList()[1];
				var curves_V = GetChemicalPotentialCurves(T, P, "vapor");
				var curves_L = GetChemicalPotentialCurves(T, P, "liquid");
				var curvePotential_V0 = curves_V[species0];
				var curvePotential_V1 = curves_V[species1];
				var curveComposition_L0 = curves_L[species0].Invert();
				var curveComposition_L1 = curves_L[species1].Invert();

				var mu_V0 = curvePotential_V0.GetValue(xV);
				var mu_V1 = curvePotential_V1.GetValue(xV);
				var xL_from0 = curveComposition_L0.GetValue(mu_V0);
				var xL_from1 = curveComposition_L1.GetValue(mu_V1);
				if (mu_V0 is null || xL_from0 is null)
				{
					//Console.WriteLine($"Common tangent not possible for x0V={xV}");
					continue;
				}
				if (mu_V1 is null || xL_from1 is null)
				{
					//Console.WriteLine($"Common tangent not possible for x0V={xV}");
					continue;
				}

				// TODO: Replace the 0.01 redundancy check with an expression
				// that picks the solution with the least error.
				var error = Math.Abs(xL_from0 - xL_from1);
				Error.Add((xV, error));
				Console.WriteLine($"Final candidate found at x0V={xV} with error {error}");
				if (error <= 0.001)
				{
					var xL = (xL_from0 + xL_from1) / 2;
					var xL_prev = resultFractions.Find(x => Math.Abs(xL - x) <= 0.01);
					if (xL_prev is not null)
					{
						xL = (xL_prev + xL) / 2;
						resultFractions.Remove(xL_prev);
						resultFractions.Add(xL);
						Console.WriteLine($"Updated equilibrium found at x0V={xV} x0L={xL}");
						continue;
					}
					resultFractions.Add(xL);
					var equilibrium = new MultiphaseEquilibriumResult();
					equilibrium.Value.Add(("vapor", species0), xV);
					equilibrium.Value.Add(("vapor", species1), 1 - xV);
					equilibrium.Value.Add(("liquid", species0), xL);
					equilibrium.Value.Add(("liquid", species1), 1 - xL);
					results.Add(equilibrium);
					Console.WriteLine($"New equilibrium found at x0V={xV} x0L={xL}");
				}
			}
		}
		return results;
	}

	/// <summary>
	/// Calculates the chemical potential curves for all species in the given phase
	/// at the specified temperature and pressure.
	/// </summary>
	public void CalculatePotentialAndEnergyCurves(Temperature T, Pressure P, string phase, List<double> compositions)
	{
		// Check for already calculated curves.
		//foreach (var entry in speciesList)
		//{
		//	var state = new MultiphaseStatePoint(entry.Key, phase, T, P);
		//	if (chemicalPotentialCurves.ContainsKey(state)) { return; }
		//}

		//MoleFraction startComposition = 0.001;
		//MoleFraction stopComposition = 1;
		//MoleFraction stepComposition = 0.05;
		//var compositions = new LinearEnumerable(startComposition, stopComposition, stepComposition).ToList();

		var state0 = new MultiphaseStatePoint(speciesList.First().Key, phase, T, P);
		var state1 = new MultiphaseStatePoint(speciesList.Last().Key, phase, T, P);
		var phaseCurve0 = new ConcurrentDictionary<MoleFraction, ChemicalPotential>();
		var phaseCurve1 = new ConcurrentDictionary<MoleFraction, ChemicalPotential>();
		var energyCurve = new ConcurrentDictionary<MoleFraction, GibbsEnergy>();

		//foreach (var x in compositions)
		//{
		//	var homomix_local = mixtureList[GetMixutureIdxFromPhase(phase)];

		//	// Assume that the composition x is the mole fraction of the first species in speciesList.
		//	homomix_local.speciesList[0].speciesMoleFraction = x;
		//	homomix_local.speciesList[1].speciesMoleFraction = 1 - x;

		//	var mu0 = homomix_local.SpeciesChemicalPotential(T, P, state0.species);
		//	var mu1 = homomix_local.SpeciesChemicalPotential(T, P, state1.species);
		//	var G = homomix_local.TotalMolarGibbsEnergy(T, P);
		//	// Note the use of composition 'x' for both species.
		//	// TODO: This should be replaced by a full vector of the composition when non-binary systems are implemented.
		//	phaseCurve0.TryAdd(x, mu0);
		//	phaseCurve1.TryAdd(x, mu1);
		//	energyCurve.TryAdd(x, G);
		//}

		Console.WriteLine($"Calculating chemical potential curves for phase \"{phase}\"");
		Parallel.ForEach(compositions, x => {
			// Assume that the composition x is the mole fraction of the first species in speciesList.
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

		var phaseTable0 = new ChemicalPotentialCurve(state0, phaseCurve0.ToDictionary(x => x.Key, x => x.Value));
		var phaseTable1 = new ChemicalPotentialCurve(state1, phaseCurve1.ToDictionary(x => x.Key, x => x.Value));
		var energyTable = new PhaseTotalGibbsEnergyCurve((phase, T, P), energyCurve.ToDictionary(x => x.Key, x => x.Value));
		phaseTable0.Resort();
		phaseTable1.Resort();
		energyTable.Resort();

		if (ChemicalPotentialCurvesContainsKey(state0))
		{
			chemicalPotentialCurves[state0].Append(phaseTable0);
			chemicalPotentialCurves[state0].Resort();
		} else
		{
			chemicalPotentialCurves.Add(state0, phaseTable0);
		}

		if (ChemicalPotentialCurvesContainsKey(state1))
		{
			chemicalPotentialCurves[state1].Append(phaseTable1);
			chemicalPotentialCurves[state1].Resort();
		} else
		{
			chemicalPotentialCurves.Add(state1, phaseTable1);
		}

		if (totalGibbsEnergyCurves.ContainsKey((phase, T, P)))
		{
			totalGibbsEnergyCurves[(phase, T, P)].Append(energyTable);
			totalGibbsEnergyCurves[(phase, T, P)].Resort();
		} else
		{
			totalGibbsEnergyCurves.Add((phase, T, P), energyTable);
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

	public Dictionary<(Temperature T, Pressure P, Chemical species), ChemicalPotentialCurve> GetChemicalPotentialCurves(string phase)
	{
		Dictionary<(Temperature T, Pressure P, Chemical species), ChemicalPotentialCurve> results = [];
		foreach (var entry in chemicalPotentialCurves)
		{
			if (entry.Key.phase == phase) results.Add((entry.Key.T, entry.Key.P, entry.Key.species), entry.Value);
		}
		return results;
	}

	public Dictionary<Chemical, ChemicalPotentialCurve> GetChemicalPotentialCurves(Temperature T, Pressure P, string phase)
	{
		Dictionary<Chemical, ChemicalPotentialCurve> results = [];
		foreach (var entry in chemicalPotentialCurves)
		{
			if (entry.Key.phase != phase) continue;
			if (entry.Key.T != T) continue;
			if (entry.Key.P != P) continue;
			
			if (!results.ContainsKey(entry.Key.species)) results.Add(entry.Key.species, entry.Value);
		}
		return results;
	}

	/// <summary>
	/// Determines if the chemical potential curves list already contains an
	/// entry for the given state.
	/// </summary>
	public bool ChemicalPotentialCurvesContainsKey(MultiphaseStatePoint state)
	{
		foreach (var curve in chemicalPotentialCurves)
		{
			if (curve.Key.Equals(state)) return true;
		}
		return false;
	}

	/// <summary>
	/// Converts all chemical potential curves in <see cref="ChemicalPotentialCurves"/>
	/// to a CSV-formatted string.
	/// </summary>
	public Dictionary<MultiphaseStatePoint, string> ConvertChemicalPotentialCurvesToCSV()
	{
		Dictionary<MultiphaseStatePoint, string> dict = [];
		foreach (var entry in chemicalPotentialCurves)
		{
			dict.Add(entry.Key, entry.Value.ToCSVString());
		}
		return dict;
	}

	/// <summary>
	/// Converts all total Gibbs energy curves in <see cref="totalGibbsEnergyCurves"/>
	/// to a CSV-formatted string.
	/// </summary>
	public Dictionary<(string phase, Temperature T, Pressure P), string> ConvertTotalGibbsEnergyCurvesToCSV()
	{
		Dictionary<(string phase, Temperature T, Pressure P), string> dict = [];
		foreach (var entry in totalGibbsEnergyCurves)
		{
			dict.Add(entry.Key, entry.Value.ToCSVString());
		}
		return dict;
	}
}


/// <summary>
/// Stores a state point for the system according to phase, species, temperature, and pressure.
/// </summary>
public struct MultiphaseStatePoint(Chemical _species, string _phase, Temperature _T, Pressure _P)
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
public struct MultiphaseEquilibriumResult()
{
	public Temperature T;
	public Pressure P;

	public Dictionary<(string phase, Chemical species), MoleFraction> Value = [];
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
}