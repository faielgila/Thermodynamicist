using Core.EquationsOfState;
using Core.Multicomponent.ActivityModels;
using Core.VariableTypes;

namespace Core.Multicomponent;

/// <summary>
/// Represents a homogeneous mixture (a multicomponent, single-phase system).
/// </summary>
public class HomogeneousMixture(List<MixtureSpecies> _speciesList, string _phase, ActivityModel _activityModel, MoleFraction? _moleFraction)
{
	private static readonly double R = Constants.R;
	public Temperature dTPrecision { get; set; } = 0.5;

	/// <summary>
	/// Spin up a <see cref="CalculationCache"/> to store calculated values and avoid repeat math.
	/// </summary>
	public readonly MetadataCalculationCache calcCache = new();

	/// <summary>
	/// Stores all mixture species with their EoS, molar fraction in the mixture, and modeledPhase.
	/// </summary>
	public List<MixtureSpecies> speciesList = _speciesList;

	/// <summary>
	/// Overall mixture phase.
	/// </summary>
	public string totalPhase = _phase;

	/// <summary>
	/// Mole fraction of this mixture relative to the system.
	/// Can be null if the system contains only this mixture,
	/// but this is not recommended.
	/// </summary>
	public MoleFraction? mixtureMoleFraction = _moleFraction;

	/// <summary>
	/// Activity coefficient model to use in thermodynamic calculations.
	/// </summary>
	public ActivityModel activityModel = _activityModel;

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

	/// <summary>
	/// Returns an exact copy of the mixture which is not linked to the original (i.e., a deep copy).
	/// </summary>
	public HomogeneousMixture Copy()
	{
		var activityModelCopy = activityModel.Copy();
		return new HomogeneousMixture(speciesList, totalPhase, activityModelCopy, mixtureMoleFraction);
	}


	#region Total properties

	/// <summary>
	/// Calculates the total molar Gibbs energy of the mixture.
	/// </summary>
	/// <exception cref="KeyNotFoundException"/>
	public GibbsEnergy TotalMolarGibbsEnergy(Temperature T, Pressure P)
	{
		GibbsEnergy total = 0;
		foreach (var species in speciesList)
		{
			total += SpeciesPartialMolarGibbsEnergy(T, P, species.chemical) * species.speciesMoleFraction;
		}
		return total;
		// see Phase Diagrams and Thermodynamics of Solutions, eq 4.3
	}

	/// <summary>
	/// Calculates the total molar enthalpy of the mixture.
	/// </summary>
	/// <exception cref="KeyNotFoundException"/>
	public Enthalpy TotalMolarEnthalpy(Temperature T, Pressure P)
	{
		var sumH = new Enthalpy(0, ThermoVarRelations.RealMolar);
		foreach (var item in speciesList)
		{
			var mixtureSpecies = speciesList[GetMixtureSpeciesIdx(item.chemical)];
			var x = mixtureSpecies.speciesMoleFraction;
			var modeledPhase = mixtureSpecies.modeledPhase;
			var EoS = mixtureSpecies.EoS;

			var phases = EoS.PhaseFinder(T, P, true);
			double VMol = 0;
			try
			{
				VMol = phases[modeledPhase];
			}
			catch
			{
				throw new KeyNotFoundException($"Phase \"{modeledPhase}\" for {Constants.ChemicalNames[item.chemical]} not found using {EoS.GetType().Name} phase finder.");
			}

			var pureH = EoS.ReferenceMolarEnthalpy(T, P, VMol);
			sumH += item.speciesMoleFraction * pureH;
		}
		var mixH = MolarEnthalpyOfMixing(T, P);
		return new Enthalpy(mixH + sumH, ThermoVarRelations.RealMolar);
	}

	#endregion


	#region Mixing properties

	public GibbsEnergy MolarGibbsEnergyOfMixing(Temperature T, Pressure P)
	{
		return 0;
	}

	/// <summary>
	/// Calculates the molar enthalpy of mixing for the mixture.
	/// </summary>
	public Enthalpy MolarEnthalpyOfMixing(Temperature T, Pressure P)
	{
		var Hex = MolarExcessEnthalpy(T, P);
		return new Enthalpy(Hex, ThermoVarRelations.Mixing);
	}

	#endregion


	#region Partial Excess properties

	/// <summary>
	/// Calculates the partial molar excess Gibbs energy for a given species in the mixture.
	/// See Sandler, eqn 9.3-12
	/// </summary>
	public GibbsEnergy SpeciesPartialMolarExcessGibbsEnergy(Temperature T, Chemical species)
	{
		var gamma = activityModel.SpeciesActivityCoefficient(species, T);
		return new GibbsEnergy(R * T * Math.Log(gamma), ThermoVarRelations.PartialMolarExcess);
	}

	/// <summary>
	/// Calculates the partial molar excess enthalpy for a given species in the mixture.
	/// </summary>
	public Enthalpy SpeciesPartialMolarExcessEnthalpy(Temperature T, Chemical species)
	{
		/* Use a first-order approximation of the partial derivative relationship between
		 * activity coefficients and the partial molar excess enthalpy. A (δT)² term is
		 * ignored for the purposes of this calculation.
		 * See Sandler eqn 9.3-21
		*/
		var gamma0 = activityModel.SpeciesActivityCoefficient(species, T);
		var gamma1 = activityModel.SpeciesActivityCoefficient(species, T + dTPrecision);
		var val = -R * T / dTPrecision * (T + 2 * dTPrecision) * Math.Log(gamma1 / gamma0);
		return new Enthalpy(val, ThermoVarRelations.PartialMolarExcess);
	}

	#endregion


	#region Partial properties

	/// <summary>
	/// Calculates the partial molar Gibbs energy for a given species in the mixture.
	/// </summary>
	/// <exception cref="KeyNotFoundException"/>
	public GibbsEnergy SpeciesPartialMolarGibbsEnergy(Temperature T, Pressure P, Chemical species)
	{
		var mixtureSpecies = speciesList[GetMixtureSpeciesIdx(species)];
		var x = mixtureSpecies.speciesMoleFraction;
		var modeledPhase = mixtureSpecies.modeledPhase;
		var EoS = mixtureSpecies.EoS;

		var phases = EoS.PhaseFinder(T, P, true);
		double VMol = 0;
		try
		{
			VMol = phases[modeledPhase];
		}
		catch
		{
			throw new KeyNotFoundException($"Phase \"{modeledPhase}\" for {Constants.ChemicalNames[species]} not found using {EoS.GetType().Name} phase finder.");
		}

		var pureG = EoS.ReferenceMolarGibbsEnergy(T, P, VMol);
		var partialGex = SpeciesPartialMolarExcessGibbsEnergy(T, species);
		var entropicCorrectionTerm = R * T * Math.Log(x);

		return new GibbsEnergy(partialGex + pureG + entropicCorrectionTerm, ThermoVarRelations.PartialMolar);
	}

	/// <summary>
	/// Calculates the partial molar enthalpy for a given species in the mixture.
	/// </summary>
	/// <exception cref="KeyNotFoundException"/>
	public Enthalpy SpeciesPartialMolarEnthalpy(Temperature T, Pressure P, Chemical species)
	{
		var mixtureSpecies = speciesList[GetMixtureSpeciesIdx(species)];
		var x = mixtureSpecies.speciesMoleFraction;
		var modeledPhase = mixtureSpecies.modeledPhase;
		var EoS = mixtureSpecies.EoS;

		var phases = EoS.PhaseFinder(T, P, true);
		double VMol = 0;
		try
		{
			VMol = phases[modeledPhase];
		}
		catch
		{
			throw new KeyNotFoundException($"Phase \"{modeledPhase}\" for {Constants.ChemicalNames[species]} not found using {EoS.GetType().Name} phase finder.");
		}

		var pureH = EoS.ReferenceMolarEnthalpy(T, P, VMol);
		var partialHex = SpeciesPartialMolarExcessEnthalpy(T, species);
		return new Enthalpy(pureH + partialHex, ThermoVarRelations.PartialMolar);
	}

	#endregion


	#region Excess properies

	/// <summary>
	/// Calculates the molar excess Enthalpy for the mixture.
	/// </summary>
	public Enthalpy MolarExcessEnthalpy(Temperature T, Pressure P)
	{
		double Hex = 0;
		foreach (var item in speciesList)
		{
			Hex += item.speciesMoleFraction * SpeciesPartialMolarExcessEnthalpy(T, item.chemical);
		}
		return new Enthalpy(Hex, ThermoVarRelations.MolarExcess);
	}

	#endregion


	#region Fugacity and Chemical potential

	/// <summary>
	/// Calculates the fugacity coefficient of a species in the mixture.
	/// </summary>
	public double SpeciesFugacity(Chemical species, Temperature T, Pressure P)
	{
		var mixtureSpecies = speciesList[GetMixtureSpeciesIdx(species)];
		var modeledPhase = mixtureSpecies.modeledPhase;
		var EoS = mixtureSpecies.EoS;

		var phases = EoS.PhaseFinder(T, P, true);
		double VMol = 0;
		try
		{
			VMol = phases[modeledPhase];
		} catch
		{
			throw new KeyNotFoundException($"Phase \"{modeledPhase}\" for {Constants.ChemicalNames[species]} not found using {EoS.GetType().Name} phase finder.");
		}
		var pureComponentFugacity = EoS.Fugacity(T, P, VMol);
		var activityCoef = activityModel.SpeciesActivityCoefficient(species, T);

		return activityCoef * pureComponentFugacity * mixtureSpecies.speciesMoleFraction;
	}

	/// <summary>
	/// Calculates the chemical potential for a species in the mixture.
	/// Equivalent to <see cref="SpeciesPartialMolarGibbsEnergy(Temperature, Pressure, Chemical)"/>.
	/// </summary>
	public ChemicalPotential SpeciesChemicalPotential(Temperature T, Pressure P, Chemical species)
	{
		return new ChemicalPotential(SpeciesPartialMolarGibbsEnergy(T, P, species), ThermoVarRelations.RealMolar);
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
	/// Calculates the molar entropy of mixing for an ideal gas mixture.
	/// </summary>
	public Entropy IGMMolarEntropyOfMixing()
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
	/// Calculates the molar Gibbs energy of mixing for an ideal gas mixture.
	/// </summary>
	public GibbsEnergy IGMMolarGibbsEnergyOfMixing(Temperature T)
	{
		// See Sandler, eqn 9.1-10
		return -T * IGMMolarEntropyOfMixing();
	}

	#endregion
}
