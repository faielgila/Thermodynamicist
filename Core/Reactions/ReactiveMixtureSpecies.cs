using Core.EquationsOfState;
using Core.Multicomponent;
using Core.VariableTypes;

namespace Core.Reactions;

/// <summary>
/// Represents a single chemical species in a reactive mixture.
/// </summary>
public class ReactiveMixtureSpecies
{
	public Chemical chemical;

	public EquationOfState EoS;

	public string phase;

	public Molarity concentration;

	public ReactiveMixtureSpecies(Chemical _chemical, Molarity _concentration, string _phase)
	{
		chemical = _chemical;
		concentration = _concentration;
		phase = _phase;

		// Since no EoS is given, assign default values based on phase.
		EoS = EquationOfState.GetDefaultEoSFromPhase(_phase).Create(_chemical);
	}

	public ReactiveMixtureSpecies(Chemical _chemical, EquationOfState _EoS, Molarity _concentration, string _phase)
	{
		chemical = _chemical;
		concentration = _concentration;
		phase = _phase;
		EoS = _EoS;
	}

	/// <summary>
	/// Converts the given ReactiveMixtureSpecies list into a MixtureSpecies list.
	/// Used to pass on the species list to the Homogeneous Mixture object.
	/// </summary>
	public static List<MixtureSpecies> ConvertToMixtureSpeciesList(List<ReactiveMixtureSpecies> rxnSpeciesList)
	{
		// Get total concentration of mixture to convert concentrations to mole fractions.
		Molarity totalConc = 0;
		foreach (var item in rxnSpeciesList)
		{
			totalConc += item.concentration;
		}

		var mixSpeciesList = new List<MixtureSpecies>();
		foreach (var item in rxnSpeciesList)
		{
			var spec = new MixtureSpecies(item.chemical, item.concentration / totalConc, item.phase);
			mixSpeciesList.Add(spec);
		}
		return mixSpeciesList;
	}
}
