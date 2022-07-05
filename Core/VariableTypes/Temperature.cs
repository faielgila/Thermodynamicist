namespace Core.VariableTypes;

public struct Temperature
{
	private readonly double _value;
	private readonly ThermoVarRelations _relation;
	
	public double Value => _value;

	public static implicit operator double(Temperature T) => T._value;
	public static implicit operator Temperature(double T) => new (T);

	public Temperature(double value, ThermoVarRelations relation = ThermoVarRelations.Temperature)
	{
		_value = value;
		_relation = relation;
	}
}