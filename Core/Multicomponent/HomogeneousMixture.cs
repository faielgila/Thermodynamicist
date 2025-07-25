using Core.EquationsOfState;
using Core.Multicomponent.ActivityModels;
using Core.VariableTypes;
using System.Runtime.CompilerServices;

namespace Core.Multicomponent;

/// <summary>
/// Represents a homogeneous mixture (a multicomponent, single-phase system).
/// </summary>
class HomogeneousMixture(List<MixtureSpecies> speciesList, string phase, ActivityModel activityModel)
{
	/// <summary>
	/// Stores all mixture species with their EoS, molar fraction in the mixture, and modeledPhase.
	/// </summary>
	public List<MixtureSpecies> speciesList = speciesList;

	/// <summary>
	/// Overall mixture phase.
	/// </summary>
	public string totalPhase = phase;

	/// <summary>
	/// Mole fraction of this mixture relative to the system.
	/// Can be null if the system contains only this mixture,
	/// but this is not recommended.
	/// </summary>
	public MoleFraction? mixtureMoleFraction;

	/// <summary>
	/// Activity coefficient model to use in thermodynamic calculations.
	/// </summary>
	public ActivityModel activityModel = activityModel;

	/// <summary>
	/// Gets the index in speciesList which represents the given chemical.
	/// </summary>
	int GetMixtureSpeciesIdx(Chemical species)
	{
		for (int i = 0; i < speciesList.Count; i++)
		{
			if (speciesList[i].chemical == species) return i;
		}

		throw new KeyNotFoundException("Species not found in speciesList.");
	}

	#region State functions - Gibbs energy

	public GibbsEnergy TotalMolarGibbsEnergy(Temperature T, Pressure P)
	{
		return 0;
		// see Phase Diagrams and Thermodynamics of Solutions, eq 4.3
	}

	public GibbsEnergy GibbsEnergyOfMixing(Temperature T, Pressure P)
	{
		return 0;
	}

	#endregion

	#region Ideal Gas Mixture (IGM) relations

	/// <summary>
	/// Calculates the total molar volume for an ideal gas mixture.
	/// </summary>
	public double IGMTotalMolarVolume(Temperature T, Pressure P)
	{
		Volume V = 0;

		foreach (var mixSpecies in speciesList)
		{
			var xi = mixSpecies.speciesMoleFraction;
			var Vi = IdealGasLaw.Volume(T, P);

			// See Perry's handbook, eqn 4-104
			// or Sandler, eqn 9.1-13
			V += xi * Vi;
		}

		return V;
	}

	/// <summary>
	/// Calculates the total molar enthalpy for an ideal gas mixture.
	/// </summary>
	public Enthalpy IGMTotalMolarEnthalpy(Temperature T) {
		Enthalpy H = 0;

		foreach (var mixSpecies in speciesList)
		{
			// We'll only be using the ideal change equations in the EoS,
			// but since EquationOfState cannot be directly instatiated
			// some other instance will need to be used. vdW is the simplest EoS.
			var IGEoS = new VanDerWaalsEOS(mixSpecies.chemical);

			var xi = mixSpecies.speciesMoleFraction;
			var Hi = IGEoS.IdealMolarEnthalpyChange(Constants.StandardConditions.T, T);

			// See Perry's handbook, eqn 4-104
			// or Sandler, eqn 9.1-13
			H += xi * Hi;
		}

		return H;
	}

	/// <summary>
	/// Calculates the total molar Gibbs energy for an ideal gas mixture.
	/// </summary>
	public GibbsEnergy IGMTotalMolarGibbsEnergy(Temperature T)
	{
		GibbsEnergy G = 0;

		foreach(var mixSpecies in speciesList)
		{
			// We'll only be using the ideal change equations in the EoS,
			// but since EquationOfState cannot be directly instatiated
			// some other instance will need to be used. vdW is the simplest EoS.
			var IGEoS = new VanDerWaalsEOS(mixSpecies.chemical);

			var xi = mixSpecies.speciesMoleFraction;
			var Gi = IGEoS.IdealMolarGibbsEnergyChange(Constants.StandardConditions.T, T);

			// See Perry's handbook, eqn 4-106
			// or Sandler, eqn 9.1-15
			G += xi * Gi + Constants.R * T * xi * Math.Log(xi);
		}

		return G;
	}

	/// <summary>
	/// Calculates the chemical potential (partial molar Gibbs energy) for a species in an ideal gas mixture.
	/// </summary>
	public ChemicalPotential IGMComponentChemicalPotential(Temperature T, Chemical species)
	{
		// We'll only be using the ideal change equations in the EoS,
		// but since EquationOfState cannot be directly instatiated
		// some other instance will need to be used. vdW is the simplest EoS.
		var IGEoS = new VanDerWaalsEOS(species);
		var Gi = IGEoS.IdealMolarGibbsEnergyChange(Constants.StandardConditions.T, T);
		var xi = speciesList[GetMixtureSpeciesIdx(species)].speciesMoleFraction;

		// See Perry's handbook, eqn 4-107
		// or Sandler, eqn 9.1-9
		return Gi + Constants.R * T * Math.Log(xi);
	}

	/// <summary>
	/// Calculates the entropy of mixing for an ideal gas mixture.
	/// </summary>
	public Entropy IGMEntropyOfMixing()
	{
		Entropy S = 0;
		foreach (var mixSpecies in speciesList)
		{
			var xi = mixSpecies.speciesMoleFraction;

			// See Sandler, eqn 9.1-8
			S += xi * Math.Log(xi);
		}
		return Constants.R * -S;
	}

	/// <summary>
	/// Calculates the Gibbs energy of mixing for an ideal gas mixture.
	/// </summary>
	public GibbsEnergy IGMGibbsEnergyOfMixing(Temperature T)
	{
		// See Sandler, eqn 9.1-10
		return -T * IGMEntropyOfMixing();
	}

	#endregion

	#region Ideal Solution/Mixture (IS) relations

	/// <summary>
	/// Calculates the total molar volume for an ideal solution.
	/// </summary>
	public double ISTotalMolarVolume(Temperature T, Pressure P)
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
	public double ISMolarHeatCapacity(Temperature T, Pressure P)
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
	public Enthalpy ISTotalMolarEnthalpy(Temperature T, Pressure P)
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
	public Entropy ISTotalMolarEntropy(Temperature T, Pressure P)
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
	public GibbsEnergy ISTotalMolarGibbsEnergy(Temperature T, Pressure P)
	{
		GibbsEnergy G = 0;

		foreach (var mixSpecies in speciesList)
		{
			var VMol = mixSpecies.EoS.PhaseFinder(T, P, true)[totalPhase];

			var xi = mixSpecies.speciesMoleFraction;
			var Gi = mixSpecies.EoS.ReferenceMolarGibbsEnergy(T,P,VMol);

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
	public ChemicalPotential ISComponentChemicalPotential(Temperature T, Pressure P, Chemical species)
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
	public double ISComponentFugacity(Temperature T, Pressure P, Chemical species)
	{
		var EoS = speciesList[GetMixtureSpeciesIdx(species)].EoS;
		var VMol = EoS.PhaseFinder(T, P, true)[totalPhase];

		var fi = EoS.Fugacity(T, P, VMol);
		var xi = speciesList[GetMixtureSpeciesIdx(species)].speciesMoleFraction;

		// See Perry's handbook, eqn 4-122
		// or Sandler, eqn 9.3-3
		return xi * fi;
	}

	#endregion
}
