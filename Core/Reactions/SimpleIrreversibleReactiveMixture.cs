using Core.Multicomponent;
using Core.Multicomponent.ActivityModels;
using Core.VariableTypes;

namespace Core.Reactions;

/// <summary>
/// Represents a homogeneous mixture with multiple reactions throughout the bulk mixture.
/// This model does not take into account any phase-change that occurs as a result of reactions;
/// all species are assumed to be in the same phase as the mixture.
/// </summary>
public class SimpleIrreversibleReactiveMixture : HomogeneousMixture
{
	/// <summary>
	/// Smallest increment in time, dt.
	/// </summary>
	public Time dtPrecision = 0.01;

	/// <summary>
	/// List of reactions in the mixture.
	/// </summary>
	public List<Reaction> reactionSet;

	/// <summary>
	/// List of reactive species, homologous to <see cref="HomogeneousMixture.speciesList"/>.
	/// </summary>
	public List<ReactiveMixtureSpecies> reactiveSpeciesList;

	/// <summary>
	/// Creates a homogeneous mixture with multiple reactions throughout the bulk mixture.
	/// </summary>
	/// <param name="_phase">mixture phase</param>
	/// <param name="_activityModel">activity model</param>
	public SimpleIrreversibleReactiveMixture(List<Reaction> _reactionSet,
		List<ReactiveMixtureSpecies> _speciesList, string _phase, ActivityModel _activityModel)
		: base(ReactiveMixtureSpecies.ConvertToMixtureSpeciesList(_speciesList), _phase, _activityModel, null)
	{
		reactiveSpeciesList = _speciesList;
		reactionSet = _reactionSet;
	}

	/// <summary>
	/// Calculates the overall rate of reaction for each reaction in the mixture.
	/// </summary>
	/// <param name="T">temperature, in [K]</param>
	/// <param name="P">pressure, in [Pa]</param>
	/// <returns>reaction rate, in [mol/L/s]</returns>
	public List<RateOfReaction> ReactionRates(Temperature T, Pressure P)
	{
		var initialConcs = GetConcentrations();
		var rates = new List<RateOfReaction>();
		foreach (var rxn in reactionSet)
		{
			rates.Add(rxn.rateLaw.ReactionRate(T, P, initialConcs));
		}
		return rates;
	}

	/// <summary>
	/// Calculates the rate of reaction for each species using the given temperature and pressure
	/// using the rate law and supplied temperature and pressure of the reaction.
	/// </summary>
	/// <param name="T">mixture temperature, in [K]</param>
	/// <param name="P">mixture pressure, in [Pa]</param>
	/// <returns>rate of reaction per species, in [mol/L/s]</returns>
	public GenericSpeciesVector<RateOfReaction> SpeciesReactionRates(Temperature T, Pressure P)
	{
		var initialConcs = GetConcentrations();
		var ratesVec = new GenericSpeciesVector<RateOfReaction>();
		foreach (var rxn in reactionSet)
		{
			var rxnRates = rxn.rateLaw.SpeciesReactionRates(T, P, initialConcs);
			ratesVec.CombineVectors(rxnRates);
		}
		return ratesVec;
	}

	/// <summary>
	/// Advances the composition of the mixture according to the rate of reaction
	/// for the duration/timestep 'dt'.
	/// </summary>
	/// <param name="T">temperature, in [K]</param>
	/// <param name="P">pressure, in [Pa]</param>
	/// <param name="dt">timestep, in [s]; defaults to <see cref="dtPrecision"/> if null.</param>
	public void ForwardTick(Temperature T, Pressure P, Time? dt)
	{
		dt ??= dtPrecision;

		var concVec_1 = new MolarityVector();
		var concVec_0 = GetConcentrations();
		var ratesVec = SpeciesReactionRates(T, P);
		foreach (var item in reactiveSpeciesList)
		{
			var change = ratesVec[item.chemical] * dt;
			concVec_1.Add(item.chemical, change + concVec_0[item.chemical]);
		}

		SetConcentrations(concVec_1);
	}

	/// <summary>
	/// Gets the concentrations of all species in the mixture.
	/// </summary>
	/// <returns>molarity for each species, in [mol/L]</returns>
	public MolarityVector GetConcentrations()
	{
		MolarityVector concVec = [];
		foreach (var item in reactiveSpeciesList)
		{
			concVec.Add(item.chemical, item.concentration);
		}
		return concVec;
	}

	/// <summary>
	/// Sets the concentrations for each species in the mixture.
	/// Note that this will also update the HomogeneousMixture's speciesList.
	/// </summary>
	/// <param name="concVec">molarity for each species, in [mol/L]</param>
	public void SetConcentrations(MolarityVector concVec)
	{
		// Update reactiveSpeciesList.
		foreach (var item in reactiveSpeciesList)
		{
			item.concentration = concVec[item.chemical];
		}
		
		// Update mixtureSpeciesList.
		SetComposition(ConvertMolarityToComposition(concVec));
	}

	/// <summary>
	/// Converts a molarity vector (mol/L for each species) to
	/// a composition vector (mol% for each species in the mixture).
	/// </summary>
	/// <param name="concVec">molarity for each species, in [mol/L]</param>
	/// <returns>composition of the mixture, in [mol%]</returns>
	public CompositionVector ConvertMolarityToComposition(MolarityVector concVec)
	{
		// Get total molar concentration.
		var totalConc = concVec.Total();

		CompositionVector compVec = [];
		foreach (var item in concVec)
		{
			var val = item.Value / totalConc;
			compVec.Add(item.Key, val);
		}
		return compVec;
	}
}
