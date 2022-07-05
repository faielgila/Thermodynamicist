namespace Core.VariableTypes;

public struct MolarEntropy
{
	private readonly double _value;
	private readonly ThermoVarRelations _relation;
	
	public double Value => _value;

	public static implicit operator double(MolarEntropy S) => S._value;
	public static implicit operator MolarEntropy(double S) => new (S);

	public MolarEntropy(double value, ThermoVarRelations relation = ThermoVarRelations.RealMolar)
	{
		_value = value;
		_relation = relation;
	}
}