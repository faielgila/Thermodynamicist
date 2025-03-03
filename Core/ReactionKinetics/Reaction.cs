using System;
using Core.EquationsOfState;
using Core.VariableTypes;
using Core.Data;
using Core;

public class Reaction
{
	/// <summary>
	/// Stores reactant chemicals with their stoichiometric coefficient and phase.
	/// </summary>
	public Dictionary<Chemical, (int stoich, string phase)> reactantList;

	/// <summary>
	/// Stores product chemicals with their stoichiometric coefficient and phase.
	/// </summary>
	public Dictionary<Chemical, (int stoich, string phase)> productList;

	/// <summary>
	/// Stores equation of state to use for each reactant and product species as a pure component.
	/// </summary>
	public Dictionary<Chemical, EquationOfState> pureSpeciesEoSList;

	private static Temperature standardT = Constants.StandardConditions.T;
	private static Pressure standardP = Constants.StandardConditions.P;

	/// <summary>
	/// Acceptable temperature differnce between standardT and T for no temperature correction to be performed.
	/// Measured in [K]
	/// </summary>
	private Temperature dTPrecision = 0.5;
	/// <summary>
	/// Acceptable pressure difference between standardP and P for no pressure correction to be performed.
	/// Measured in [Pa]
	/// </summary>
	private Pressure dPPrecision = 100;

	public Reaction(
		Dictionary<Chemical, (int stoich, string phase)> _reactantList,
		Dictionary<Chemical, (int stoich, string phase)> _productList)
	{
		reactantList = _reactantList;
		productList = _productList;

		pureSpeciesEoSList = new Dictionary<Chemical, EquationOfState>();
		AssignPureSpeciesEoS(reactantList);
		AssignPureSpeciesEoS(productList);
	}
	public Reaction(
		Dictionary<Chemical, (int stoich, string phase)> _reactantList,
		Dictionary<Chemical, (int stoich, string phase)> _productList,
		Dictionary<Chemical, EquationOfState> _pureSpeciesEoSList)
	{
		reactantList = _reactantList;
		productList = _productList;
		pureSpeciesEoSList = _pureSpeciesEoSList;
	}

	/// <summary>
	/// Assigns default EoS for every species in speciesList based on their phase.
	/// </summary>
	/// <param name="speciesList">list of chemical species to assign.
	/// key = chemical species, value = (stoichiometric coef., phase key)</param>
	private void AssignPureSpeciesEoS(Dictionary<Chemical, (int, string)> speciesList)
	{
		foreach (var species in speciesList)
		{
			switch (species.Value.Item2)
			{
				case "vapor":
				case "liquid":
					pureSpeciesEoSList.Add(species.Key, new PengRobinsonEOS(species.Key));
					break;
				case "solid":
					pureSpeciesEoSList.Add(species.Key, new ModSolidLiquidVaporEOS(species.Key));
					break;
				default:
					pureSpeciesEoSList.Add(species.Key, new PengRobinsonEOS(species.Key));
					break;
			}
		}
	}

	/// <summary>
	/// Estimates the molar enthalpy of reaction at a given temperature and pressure.
	/// Uses the reference molar enthalpy of each species with the specified EoS in pureSpeciesEoSList.
	/// </summary>
	/// <param name="T_rxn">temperature, in [K]</param>
	/// <param name="P_rxn">pressure, in [Pa]</param>
	/// <returns>molar enthalpy, in [J/mol]</returns>
	/// <exception cref="KeyNotFoundException"> Thrown when a species is in reactantList or productList, but not in pureSpeciesEoSList.
	/// </exception>
	public Enthalpy MolarEnthalpyOfReaction(Temperature T_rxn, Pressure P_rxn)
	{
		// Init empty list of enthalpy changes for each species.
		var reactantsFormationEnthalpy = new List<Enthalpy>();
		var productsFormationEnthalpy = new List<Enthalpy>();

		// Calculate enthalpy changes for each reactant species.
		foreach (var species in reactantList)
		{
			// Get equation of state for species, as defined in pureSpeciesEoSList.
			EquationOfState EoS;
			try { EoS = pureSpeciesEoSList[species.Key]; }
			catch { throw new KeyNotFoundException("Reactant species not found in pure species EoS list."); }

			// Get reaction phase for species.
			string phase_rxn = species.Value.phase;
			
			// Calculate formation enthalpy for species.
			var speciesEnthalpy = EoS.FormationEnthalpy(T_rxn, P_rxn, phase_rxn);

			// Add formation enthalpy to list, multiplied by stoichiometric coefficient.
			reactantsFormationEnthalpy.Add(speciesEnthalpy * species.Value.stoich);
		}

		// Calculate enthalpy changes for each product species.
		foreach (var species in productList)
		{
			// Get equation of state for species, as defined in pureSpeciesEoSList.
			EquationOfState EoS;
			try { EoS = pureSpeciesEoSList[species.Key]; }
			catch { throw new KeyNotFoundException("Product species not found in pure species EoS list."); }

			// Get reaction phase for species.
			string phase_rxn = species.Value.phase;

			// Calculate formation enthalpy for species.
			var speciesEnthalpy = EoS.FormationEnthalpy(T_rxn, P_rxn, phase_rxn);

			// Add formation enthalpy to list, multiplied by stoichiometric coefficient.
			productsFormationEnthalpy.Add(speciesEnthalpy * species.Value.stoich);
		}

		// Sum all species enthalpy changes.
		var reactionEnthalpy = new Enthalpy(0, ThermoVarRelations.OfReaction);
		foreach (var H_i in reactantsFormationEnthalpy)
		{
			reactionEnthalpy += H_i;
		}
		foreach (var H_i in productsFormationEnthalpy)
		{
			reactionEnthalpy -= H_i;
		}

		return reactionEnthalpy;
	}

	/// <summary>
	/// Estimates the molar entropy of reaction at a given temperature and pressure.
	/// Uses the reference molar entropy of each species with the specified EoS in pureSpeciesEoSList.
	/// </summary>
	/// <param name="T">temperature, in [K]</param>
	/// <param name="P">pressure, in [Pa]</param>
	/// <returns>molar entropy, in [J/K/mol]</returns>
	/// <exception cref="KeyNotFoundException"> Thrown when a species is in reactantList or productList, but not in pureSpeciesEoSList.
	/// </exception>
	public Entropy MolarEntropyOfReaction(Temperature T, Pressure P)
	{
		// Use formation entropies to estimate entropy of reaction.

		Entropy reactantFormationEntropy = 0;
		foreach (var reactant in reactantList)
		{
			EquationOfState EoS;
			try { EoS = pureSpeciesEoSList[reactant.Key]; }
			catch { throw new KeyNotFoundException("Reactant species not found in pure species EoS list."); }

			var VMol = EoS.EquilibriumPhases(T, P)[reactant.Value.Item2];
			reactantFormationEntropy += reactant.Value.Item1 * EoS.ReferenceMolarEntropy(T, P, VMol);
		}

		Entropy productFormationEntropy = 0;
		foreach (var product in productList)
		{
			EquationOfState EoS;
			try { EoS = pureSpeciesEoSList[product.Key]; }
			catch { throw new KeyNotFoundException("Product species not found in pure species EoS list."); }

			var VMol = EoS.EquilibriumPhases(T, P)[product.Value.Item2];
			productFormationEntropy += product.Value.Item1 * EoS.ReferenceMolarEntropy(T, P, VMol);
		}

		return new Entropy(productFormationEntropy - reactantFormationEntropy, ThermoVarRelations.OfReaction);
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
