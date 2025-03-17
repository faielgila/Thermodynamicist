using Core.VariableTypes;
using Core;

/// <summary>
/// Represents a chemical reaction and its related variables.
/// </summary>
public class Reaction
{
	/// <summary>
	/// Stores all reaction chemicals with their EoS, stoichiometric coefficient, and phase.
	/// </summary>
	public List<RxnSpecies> speciesList;

	public Reaction(List<RxnSpecies> _speciesList)
	{
		speciesList = _speciesList;
	}
	public Reaction(List<(Chemical species, int stoich, string phase, bool IsReactant)> _speciesListNoEOS)
	{
		var _speciesList = new List<RxnSpecies>(); 
		foreach (var (species, stoich, phase, IsReactant) in _speciesListNoEOS)
		{
			var item = new RxnSpecies(species, stoich, phase, IsReactant);
			_speciesList.Add(item);
		}

		speciesList = _speciesList;
	}

	/// <summary>
	/// Estimates the molar enthalpy of reaction at a given temperature and pressure.
	/// </summary>
	/// <param name="T_rxn">temperature, in [K]</param>
	/// <param name="P_rxn">pressure, in [Pa]</param>
	/// <returns>molar enthalpy, in [J/mol]</returns>
	/// </exception>
	public Enthalpy MolarEnthalpyOfReaction(Temperature T_rxn, Pressure P_rxn)
	{
		// Init empty list of enthalpy changes for each species.
		var speciesFormationEnthalpy = new List<Enthalpy>();

		// Calculate enthalpy changes for each species.
		foreach (var speciesItem in speciesList)
		{
			
			// Calculate formation enthalpy for species.
			var speciesEnthalpy = speciesItem.EoS.FormationEnthalpy(T_rxn, P_rxn, speciesItem.phase);

			// Add formation enthalpy to list, multiplied by stoichiometric coefficient.
			// Formation enthalpy is considered negative for reactants, and positive for products.
			if (speciesItem.IsReactant) speciesFormationEnthalpy.Add(speciesEnthalpy * -speciesItem.stoich);
			else speciesFormationEnthalpy.Add(speciesEnthalpy * speciesItem.stoich);
		}

		// Sum all species enthalpy changes.
		var reactionEnthalpy = new Enthalpy(0, ThermoVarRelations.OfReaction);
		foreach (var H_i in speciesFormationEnthalpy)
		{
			reactionEnthalpy += H_i;
		}

		return reactionEnthalpy;
	}

	/// <summary>
	/// Estimates the molar entropy of reaction at a given temperature and pressure.
	/// </summary>
	/// <param name="T">temperature, in [K]</param>
	/// <param name="P">pressure, in [Pa]</param>
	/// <returns>molar entropy, in [J/K/mol]</returns>
	/// </exception>
	public Entropy MolarEntropyOfReaction(Temperature T_rxn, Pressure P_rxn)
	{
		// Init empty list of entropy changes for each species.
		var speciesFormationEntropy = new List<Entropy>();

		// Calculate entropy changes for each species.
		foreach (var speciesItem in speciesList)
		{

			// Calculate formation enthalpy for species.
			var speciesEntropy = speciesItem.EoS.FormationEntropy(T_rxn, P_rxn, speciesItem.phase);

			// Add formation enthalpy to list, multiplied by stoichiometric coefficient.
			// Formation enthalpy is considered negative for reactants, and positive for products.
			if (speciesItem.IsReactant) speciesFormationEntropy.Add(speciesEntropy * -speciesItem.stoich);
			else speciesFormationEntropy.Add(speciesEntropy * speciesItem.stoich);
		}

		// Sum all species enthalpy changes.
		var reactionEntropy = new Entropy(0, ThermoVarRelations.OfReaction);
		foreach (var H_i in speciesFormationEntropy)
		{
			reactionEntropy += H_i;
		}

		return reactionEntropy;
	}

	/// <summary>
	/// Estimates the molar Gibbs energy of reaction at a given temperature and pressure.
	/// Uses the reference molar enthalpy and entropy of each species with the specified EoS in pureSpeciesEoSList.
	/// </summary>
	/// <param name="T">temperature, in [K]</param>
	/// <param name="P">pressure, in [Pa]</param>
	/// <returns>molar Gibbs energy, in [J/mol]</returns>
	public GibbsEnergy MolarGibbsEnergyOfReaction(Temperature T, Pressure P)
	{
		return new GibbsEnergy(MolarEnthalpyOfReaction(T,P) - T*MolarEntropyOfReaction(T,P), ThermoVarRelations.OfReaction);
	}
}
