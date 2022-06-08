namespace Core.VariableTypes;

public struct MoleFraction
{
	private readonly double _value;

	public static implicit operator double(MoleFraction x) => x._value;
	public static implicit operator MoleFraction(double x) => new MoleFraction(x);

	private MoleFraction(double value) { _value = value; }

	public enum Relation
	{
		FractionInVapor,
		FractionInLiquid1, FractionInLiquid2,
		FractionInSolid1, FractionInSolid2,
		FractionInSystem
	}
}