namespace Core.VariableTypes;

public struct MolarGibbsEnergy
{
	private readonly double _value;
	private readonly ThermoVarRelations _relation;
	
	public double Value => _value;

	public static implicit operator double(MolarGibbsEnergy G) => G._value;
	public static implicit operator MolarGibbsEnergy(double G) => new (G);

	public MolarGibbsEnergy(double value, ThermoVarRelations relation = ThermoVarRelations.RealMolar)
	{
		_value = value;
		_relation = relation;
	}
}