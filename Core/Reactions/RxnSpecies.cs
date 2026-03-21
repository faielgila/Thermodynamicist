using ThermodynamicistCore.EquationsOfState;
using ThermodynamicistCore;

namespace ThermodynamicistCore.Reactions;

/// <summary>
/// Stores all data that <see cref="Reaction"/> needs for each species in a reaction.
/// </summary>
public class RxnSpecies
{
	public Chemical chemical;
	public IEquationOfStateFactory EoSFactory;
	public int stoich;
	public string phase;
	public bool IsReactant;

	public RxnSpecies(Chemical _chemical, IEquationOfStateFactory _EoSFactory, int _stoich, string _phase, bool _IsReactant)
	{
		chemical = _chemical;
		EoSFactory = _EoSFactory;
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
		EoSFactory = EquationOfState.GetDefaultEoSFromPhase(_phase);
	}

	/// <summary>
	/// Creates an instance of the EoS for this species using the stored EoSFactory.
	/// </summary>
	public EquationOfState EoS()
	{
		return EoSFactory.Create(chemical);
	}
}
