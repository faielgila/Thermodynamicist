using ThermodynamicistCore.EquationsOfState;
using ThermodynamicistCore.Multicomponent;
using ThermodynamicistCore.VariableTypes;

namespace ThermodynamicistCore.Reactions;

/// <summary>
/// Represents a single chemical species in a reactive mixture.
/// </summary>
public class ReactiveMixtureSpecies
{
	public Chemical chemical;

	public IEquationOfStateFactory EoSFactory;

	public string phase;

	public Molarity concentration;

	public ReactiveMixtureSpecies(Chemical _chemical, Molarity _concentration, string _phase)
	{
		chemical = _chemical;
		concentration = _concentration;
		phase = _phase;

		// Since no EoS is given, assign default values based on phase.
		EoSFactory = EquationOfState.GetDefaultEoSFromPhase(_phase);
	}

	public ReactiveMixtureSpecies(Chemical _chemical, IEquationOfStateFactory _EoSFactory, Molarity _concentration, string _phase)
	{
		chemical = _chemical;
		concentration = _concentration;
		phase = _phase;
		EoSFactory = _EoSFactory;
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
