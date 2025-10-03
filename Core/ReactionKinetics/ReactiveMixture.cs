using Core.Multicomponent;
using Core.Multicomponent.ActivityModels;
using Core.VariableTypes;

namespace Core.ReactionKinetics;

public class ReactiveMixture : HomogeneousMixture
{
	public List<Reaction> reactionSet;

	public ReactiveMixture(List<Reaction> _reactionSet,
		List<MixtureSpecies> _speciesList, CompositionVector _compositions, string _phase, ActivityModel _activityModel)
		: base(_speciesList, _phase, _activityModel, null)
	{
		reactionSet = _reactionSet;
	}

	
}
