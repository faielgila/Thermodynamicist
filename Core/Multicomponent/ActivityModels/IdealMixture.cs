using Core.VariableTypes;
using System;

namespace Core.Multicomponent.ActivityModels;

public class IdealMixture(string _phase, List<MixtureSpecies> _speciesList) : ActivityModel(_speciesList)
{
	/// <summary>
	/// Overall mixture phase.
	/// </summary>
	public string totalPhase = _phase;

	/// <summary>
	/// Returns an exact copy of the mixture which is not linked to the original (i.e., a deep copy).
	/// totalPhase will remain a shallow copy of the original.
	/// </summary>
	public override ActivityModel Copy()
	{
		var speciesListCopy = new List<MixtureSpecies>();
		foreach (var item in speciesList) speciesListCopy.Add(item.Copy());
		return new IdealMixture(totalPhase, speciesListCopy);
	}

	/// <summary>
	/// Gets the index in speciesList which represents the given chemical.
	/// </summary>
	int GetMixtureSpeciesIdx(Chemical species)
	{
		for (int i = 0; i < speciesList.Count; i++)
		{
			if (speciesList[i].chemical == species) return i;
		}

		throw new KeyNotFoundException($"{Constants.ChemicalNames[species]} not found in speciesList.");
	}

	public override double SpeciesActivityCoefficient(Chemical species, Temperature T, Pressure P)
	{
		// By definition, activity coefficients for an ideal mixture is 1.
		// While seemingly unnecessary, in order to use IdealMixture as an
		// activity model, this method must be implemented.
		return 1;
	}

	/// <summary>
	/// Calculates the total molar volume for an ideal solution.
	/// </summary>
	public double TotalMolarVolume(Temperature T, Pressure P)
	{
		Volume V = 0;

		foreach (var mixSpecies in speciesList)
		{
			var Vi = mixSpecies.EoS.PhaseFinder(T, P, true)[totalPhase];
			var xi = mixSpecies.speciesMoleFraction;

			// See Perry's handbook, eqn 4-118
			// or Sandler, eqn 9.3-5b
			V += xi * Vi;
		}

		return V;
	}

	/// <summary>
	/// Calculates the overall molar heat capacity for an ideal solution.
	/// </summary>
	public double MolarHeatCapacity(Temperature T, Pressure P)
	{
		double Cp = 0;
		foreach (var mixSpecies in speciesList)
		{
			var Vi = mixSpecies.EoS.PhaseFinder(T, P, true)[totalPhase];

			var xi = mixSpecies.speciesMoleFraction;
			var Cpi = mixSpecies.EoS.MolarHeatCapacity(T, P, Vi);
			Cp += xi * Cpi;
		}
		return Cp;
	}

	/// <summary>
	/// Calculates the total molar enthalpy for an ideal solution.
	/// </summary>
	public Enthalpy TotalMolarEnthalpy(Temperature T, Pressure P)
	{
		Enthalpy H = 0;

		foreach (var mixSpecies in speciesList)
		{
			var VMol = mixSpecies.EoS.PhaseFinder(T, P, true)[totalPhase];

			var xi = mixSpecies.speciesMoleFraction;
			var Hi = mixSpecies.EoS.ReferenceMolarEnthalpy(T, P, VMol);

			// See Perry's handbook, eqn 4-120
			// or Sandler, eqn 9.3-5c
			H += xi * Hi;
		}

		return H;
	}

	/// <summary>
	/// Calculates the total molar entropy for an ideal solution.
	/// </summary>
	public Entropy TotalMolarEntropy(Temperature T, Pressure P)
	{
		Entropy S = 0;

		foreach (var mixSpecies in speciesList)
		{
			var VMol = mixSpecies.EoS.PhaseFinder(T, P, true)[totalPhase];

			var xi = mixSpecies.speciesMoleFraction;
			var Si = mixSpecies.EoS.ReferenceMolarEntropy(T, P, VMol);

			// See Perry's handbook, eqn 4-119
			// or Sandler, eqn 9.3-5d
			S += xi * Si - Constants.R * xi * Math.Log(xi);
		}

		return S;
	}

	/// <summary>
	/// Calculates the total molar Gibbs energy for an ideal solution.
	/// </summary>
	public GibbsEnergy TotalMolarGibbsEnergy(Temperature T, Pressure P)
	{
		GibbsEnergy G = 0;

		foreach (var mixSpecies in speciesList)
		{
			var VMol = mixSpecies.EoS.PhaseFinder(T, P, true)[totalPhase];

			var xi = mixSpecies.speciesMoleFraction;
			var Gi = mixSpecies.EoS.ReferenceMolarGibbsEnergy(T, P, VMol);

			// See Perry's handbook, eqn 4-117
			// or Sandler, eqn 9.3-5e
			G += xi * Gi + Constants.R * T * xi * Math.Log(xi);
		}

		return G;
	}

	/// <summary>
	/// Calculates the chemical potential (partial molar Gibbs energy) for
	/// a component in an ideal solution.
	/// </summary>
	public ChemicalPotential ComponentChemicalPotential(Temperature T, Pressure P, Chemical species)
	{
		var EoS = speciesList[GetMixtureSpeciesIdx(species)].EoS;
		var VMol = EoS.PhaseFinder(T, P, true)[totalPhase];

		var Gi = EoS.ReferenceMolarGibbsEnergy(T, P, VMol);
		var xi = speciesList[GetMixtureSpeciesIdx(species)].speciesMoleFraction;

		// See Perry's handbook, eqn 4-113
		return Gi + Constants.R * T * Math.Log(xi);
	}

	/// <summary>
	/// Calculates the fugacity of a component in an ideal solution.
	/// Uses the Lewis-Randall rule.
	/// </summary>
	public double ComponentFugacity(Temperature T, Pressure P, Chemical species)
	{
		var EoS = speciesList[GetMixtureSpeciesIdx(species)].EoS;
		var VMol = EoS.PhaseFinder(T, P, true)[totalPhase];

		var fi = EoS.Fugacity(T, P, VMol);
		var xi = speciesList[GetMixtureSpeciesIdx(species)].speciesMoleFraction;

		// See Perry's handbook, eqn 4-122
		// or Sandler, eqn 9.3-3
		return xi * fi;
	}
}
