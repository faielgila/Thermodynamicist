namespace Core.VariableTypes;

public struct Pressure
{
	private readonly double _value;
	private readonly ThermoVarRelations _relation;
	
	public static implicit operator double(Pressure P) => P._value;
	public static implicit operator Pressure(double P) => new (P);

	public Pressure(double value, ThermoVarRelations relation = ThermoVarRelations.Pressure)
	{
		_value = value;
		_relation = relation;
	}
}