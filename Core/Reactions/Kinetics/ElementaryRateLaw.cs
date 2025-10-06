using Core.VariableTypes;

namespace Core.Reactions.Kinetics;

public class ElementaryRateLaw(List<RxnSpecies> _speciesList, double _frequencyFactor, GibbsEnergy _activationEnergy)
	: RateLaw(_speciesList, _frequencyFactor, _activationEnergy)
{
	public override RateOfReaction ReactionRate(Temperature T, Pressure P, MolarityVector concentrations)
	{
		var k = RateConstant(T);
		double product = 1;
		foreach (var rxnSpecies in speciesList)
		{
			var chemical = rxnSpecies.chemical;
			if (rxnSpecies.IsReactant) product *= Math.Pow(concentrations[chemical], stoich[chemical]);
		}
		return product * k;
	}
}
