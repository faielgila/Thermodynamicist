namespace Core.VariableTypes;

public struct MolarInternalEnergy
{
	private readonly double _value;
	private readonly ThermoVarRelations _relation;
	
	public double Value => _value;

	public static implicit operator double(MolarInternalEnergy U) => U._value;
	public static implicit operator MolarInternalEnergy(double U) => new (U);

	public MolarInternalEnergy(double value, ThermoVarRelations relation = ThermoVarRelations.RealMolar)
	{
		_value = value;
		_relation = relation;
	}
}