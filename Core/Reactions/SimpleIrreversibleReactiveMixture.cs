using Core.Multicomponent;
using Core.Multicomponent.ActivityModels;
using Core.VariableTypes;

namespace Core.Reactions;

public class SimpleIrreversibleReactiveMixture : HomogeneousMixture
{
	public List<Reaction> reactionSet;

	public SimpleIrreversibleReactiveMixture(List<Reaction> _reactionSet,
		List<MixtureSpecies> _speciesList, CompositionVector _compositions, string _phase, ActivityModel _activityModel)
		: base(_speciesList, _phase, _activityModel, null)
	{
		reactionSet = _reactionSet;
	}

	
}
