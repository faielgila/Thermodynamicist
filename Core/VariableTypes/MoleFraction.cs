namespace Core.VariableTypes;

public struct MoleFraction
{
	private readonly double _value;
	private readonly ThermoVarRelations _relation;

	public static implicit operator double(MoleFraction x) => x._value;
	public static implicit operator MoleFraction(double x) => new (x, ThermoVarRelations.Undefined);

	public MoleFraction(double value, ThermoVarRelations relation)
	{
		_value = value;
		_relation = relation;
	}
}