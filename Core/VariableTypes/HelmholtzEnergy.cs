namespace Core.VariableTypes;

public struct MolarHelmholtzEnergy
{
	private readonly double _value;
	private readonly ThermoVarRelations _relation;
	
	public double Value => _value;

	public static implicit operator double(MolarHelmholtzEnergy A) => A._value;
	public static implicit operator MolarHelmholtzEnergy(double A) => new (A);

	public MolarHelmholtzEnergy(double value, ThermoVarRelations relation = ThermoVarRelations.RealMolar)
	{
		_value = value;
		_relation = relation;
	}
}