namespace Core.VariableTypes;

public struct MolarEnthalpy
{
	private readonly double _value;
	private readonly ThermoVarRelations _relation;
	
	public double Value => _value;

	public static implicit operator double(MolarEnthalpy H) => H._value;
	public static implicit operator MolarEnthalpy(double H) => new (H);

	public MolarEnthalpy(double value, ThermoVarRelations relation = ThermoVarRelations.RealMolar)
	{
		_value = value;
		_relation = relation;
	}
}