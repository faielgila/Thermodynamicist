using Core.EquationsOfState;
using Core;

namespace Core.Reactions;

/// <summary>
/// Stores all data that <see cref="Reaction"/> needs for each species in a reaction.
/// </summary>
public class RxnSpecies
{
	public Chemical chemical;
	public EquationOfState EoS;
	public int stoich;
	public string phase;
	public bool IsReactant;

	public RxnSpecies(Chemical _chemical, EquationOfState _EoS, int _stoich, string _phase, bool _IsReactant)
	{
		chemical = _chemical;
		EoS = _EoS;
		stoich = _stoich;
		phase = _phase;
		IsReactant = _IsReactant;
	}

	public RxnSpecies(Chemical _chemical, int _stoich, string _phase, bool _IsReactant)
	{
		chemical = _chemical;
		stoich = _stoich;
		phase = _phase;
		IsReactant = _IsReactant;

		// Since no EoS is given, assign default values based on phase.
		EoS = EquationOfState.GetDefaultEoSFromPhase(_phase).Create(_chemical);
	}
}
