using Core.VariableTypes;

namespace Core.Reactions.Kinetics;

/// <summary>
/// Abstract class which represents a chemical reaction's rate law.
/// </summary>
public abstract class RateLaw
{
	public Dictionary<Chemical, int> stoich { get; private set; }

	public List<RxnSpecies> speciesList { get; private set; }

	/// <summary>
	/// Pre-exponential factor for use in the Arrhenius equation.
	/// </summary>
	private double frequencyFactor;

	/// <summary>
	/// Activation energy for the reaction.
	/// Consider that the activation energy is highly dependent on the phases
	/// present, whether a catalyst is used, the exact reaction mechanism
	/// being predicted, etc.
	/// </summary>
	private GibbsEnergy activationEnergy;

	public RateLaw(List<RxnSpecies> _speciesList, double _frequencyFactor, GibbsEnergy _activationEnergy)
	{
		speciesList = _speciesList;
		stoich = [];
		foreach (var rxnSpecies in _speciesList)
		{
			stoich.Add(rxnSpecies.chemical, rxnSpecies.stoich);
		}

		frequencyFactor = _frequencyFactor;
		activationEnergy = _activationEnergy;
	}

	/// <summary>
	/// Calculates the rate of reaction at the given temperature and pressure
	/// relative to the species.
	/// If the species is a reactant, the reaction rate will be negative (i.e., consumption rate).
	/// If the species is a product, the reaction rate will be positive (i.e., generation rate).
	/// </summary>
	public abstract RateOfReaction ReactionRate(Temperature T, Pressure P, MolarityVector concentrations);

	/// <summary>
	/// Calculates the individual rates of generation/consumption for all species in the reaction
	/// based on stoichiometry.
	/// </summary>
	public GenericSpeciesVector<RateOfReaction> SpeciesReactionRates(Temperature T, Pressure P, MolarityVector concentrations)
	{
		var r = ReactionRate(T, P, concentrations);
		GenericSpeciesVector<RateOfReaction> results = [];
		foreach (var item in concentrations)
		{
			try
			{
				GetRxnSpeciesIdx(item.Key);
			} catch
			{
				results.Add(item.Key, 0);
				continue;
			}
			var rxnSpecies = speciesList[GetRxnSpeciesIdx(item.Key)];
			var change = rxnSpecies.IsReactant ? rxnSpecies.stoich * -r : rxnSpecies.stoich * r;
			results.Add(item.Key, change);
		}
		return results;
	}

	/// <summary>
	/// Uses the Arrhenius equation to calculate the reaction rate constant k.
	/// </summary>
	public double RateConstant(Temperature T)
	{
		return frequencyFactor * Math.Exp(-activationEnergy / (T * Constants.R) );
	}

	/// <summary>
	/// Gets the index in speciesList which represents the given chemical.
	/// </summary>
	int GetRxnSpeciesIdx(Chemical species)
	{
		for (int i = 0; i < speciesList.Count; i++)
		{
			if (speciesList[i].chemical == species) return i;
		}

		throw new KeyNotFoundException($"{Constants.ChemicalNames[species]} not found in speciesList.");
	}
}