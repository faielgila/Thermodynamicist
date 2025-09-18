using Core.Data;
using Core.VariableTypes;
using System;
using System.Text;

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

		MoleFraction startX = 0.01;
		MoleFraction stepX = 0.01;

		foreach (var phase in SystemPhases)
		{
			CalculatePotentialAndEnergyCurves(T, P, phase);
			Console.WriteLine($"Calculated potential curves for phase \"{phase}\".");
		}

		// All compositions are mol% species0.
		// TODO: Add detection for which species the user wants to use as the basis.
		// TODO: Use for loop here for nonbinary mixtures.
		List<MultiphaseEquilibriumResult> results = [];
		for (MoleFraction xV = startX; xV <= 1; xV += stepX)
		{
			Console.WriteLine($"Testing equilibrium for x0V={xV}...");
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
			if (mu_V0 is null || xL_from0 is null) continue;
			if (mu_V1 is null || xL_from1 is null) continue;

			var error = Math.Abs(xL_from0 - xL_from1);
			Console.WriteLine($"Phase compositions found. Error={error}");
			if (error <= 0.008)
			{
				var xL = (xL_from0 + xL_from1) / 2;
				var equilibrium = new MultiphaseEquilibriumResult();
				equilibrium.Value.Add(("vapor", species0), xV);
				equilibrium.Value.Add(("vapor", species1), 1-xV);
				equilibrium.Value.Add(("liquid", species0), xL);
				equilibrium.Value.Add(("liquid", species1), 1-xL);
				results.Add(equilibrium);
				Console.WriteLine($"Equilibrium point found: x0V={xV}, x0L={xL}");
			}
		}
		Console.WriteLine($"Equilibrium search complete. Equilibria found: {results.Count}");

		return results;
	}

	/// <summary>
	/// Calculates the chemical potential curves for all species in the given phase
	/// at the specified temperature and pressure.
	/// </summary>
	public void CalculatePotentialAndEnergyCurves(Temperature T, Pressure P, string phase)
	{
		// Check for already calculated curves.
		//foreach (var entry in speciesList)
		//{
		//	var state = new MultiphaseStatePoint(entry.Key, phase, T, P);
		//	if (chemicalPotentialCurves.ContainsKey(state)) { return; }
		//}

		MoleFraction startComposition = 0.001;
		MoleFraction stopComposition = 1;
		MoleFraction stepComposition = 0.05;
		var compositions = new LinearEnumerable(startComposition, stopComposition, stepComposition);

		var homomix_local = mixtureList[GetMixutureIdxFromPhase(phase)];

		var state0 = new MultiphaseStatePoint(homomix_local.speciesList[0].chemical, phase, T, P);
		var state1 = new MultiphaseStatePoint(homomix_local.speciesList[1].chemical, phase, T, P);
		var phaseCurve0 = new ChemicalPotentialCurve() { state = state0 };
		var phaseCurve1 = new ChemicalPotentialCurve() { state = state1 };
		var energyCurve = new PhaseTotalGibbsEnergyCurve() { state = (phase, T, P) };

		foreach (var x in compositions)
		{
			// Assume that the composition x is the mole fraction of the first species in speciesList.
			homomix_local.speciesList[0].speciesMoleFraction = x;
			homomix_local.speciesList[1].speciesMoleFraction = 1-x;

			var mu0 = homomix_local.SpeciesChemicalPotential(T, P, state0.species);
			var mu1 = homomix_local.SpeciesChemicalPotential(T, P, state1.species);
			var G = homomix_local.TotalMolarGibbsEnergy(T, P);
			// Note the use of composition 'x' for both species.
			// TODO: This should be replaced by a full vector of the composition when non-binary systems are implemented.
			phaseCurve0.Add(x, mu0);
			phaseCurve1.Add(x, mu1);
			energyCurve.Add(x, G);
		}
		chemicalPotentialCurves.Add(phaseCurve0.state, phaseCurve0);
		chemicalPotentialCurves.Add(phaseCurve1.state , phaseCurve1);
		totalGibbsEnergyCurves.Add(energyCurve.state, energyCurve);
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
public class MultiphaseStatePoint(Chemical _species, string _phase, Temperature _T, Pressure _P)
{
	public Chemical species = _species;
	public string phase = _phase;
	public Temperature T = _T;
	public Pressure P = _P;
}

public class MultiphaseEquilibriumResult()
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