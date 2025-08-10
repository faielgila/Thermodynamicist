using Core.EquationsOfState;
using Core.VariableTypes;

namespace Core.Multicomponent;

public class MixtureSpecies
{
	public Chemical chemical;

	public EquationOfState EoS;

	public string modeledPhase;

	public MoleFraction speciesMoleFraction;

	/// <summary>
	/// Stores important information for <see cref="HomogeneousMixture"/> needs for each species
	/// in the mixture.
	/// </summary>
	/// <param name="_phase">modeled phase for the EoS</param>
	/// <param name="_moleFraction">mole fraction of this species in the mixture relative to the overall mixture</param>
	public MixtureSpecies(Chemical _chemical, EquationOfState _EoS, MoleFraction _moleFraction, string _phase)
	{
		chemical = _chemical;
		EoS = _EoS;
		modeledPhase = _phase;

		speciesMoleFraction = _moleFraction;
	}

	/// <summary>
	/// Stores important information for <see cref="HomogeneousMixture"/> needs for each species
	/// in the mixture. Automatically assigns an equation of state based on the given phase.
	/// </summary>
	/// <param name="_phase">modeled phase for the EoS</param>
	/// <param name="_moleFraction">mole fraction of this species in the mixture relative to the overall mixture</param>
	public MixtureSpecies(Chemical _chemical, MoleFraction _moleFraction, string _phase)
	{
		chemical = _chemical;
		modeledPhase = _phase;

		speciesMoleFraction = _moleFraction;

		// Since no EoS is given, assign default values based on phase.
		EoS = EquationOfState.GetDefaultEoSFromPhase(_phase).Create(_chemical);
	}
}
