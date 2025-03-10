using Core.EquationsOfState;
using Core;
public class RxnSpeciesListItem
{
	public Chemical chemical;
	public EquationOfState EoS;
	public int stoich;
	public string phase;
	public bool IsReactant;

	public RxnSpeciesListItem(Chemical _chemical, EquationOfState _EoS, int _stoich, string _phase, bool _IsReactant)
	{
		chemical = _chemical;
		EoS = _EoS;
		stoich = _stoich;
		phase = _phase;
		IsReactant = _IsReactant;
	}

	public RxnSpeciesListItem(Chemical _chemical, int _stoich, string _phase, bool _IsReactant)
	{
		chemical = _chemical;
		stoich = _stoich;
		phase = _phase;
		IsReactant = _IsReactant;

		// Since no EoS are given, assign default values based on phase.
		EoS = _phase switch
		{
			"vapor" or "liquid" => new PengRobinsonEOS(chemical),
			"solid" => new ModSolidLiquidVaporEOS(chemical),
			_ => new PengRobinsonEOS(chemical),
		};
	}

	public RxnSpeciesListItem() { }
}
