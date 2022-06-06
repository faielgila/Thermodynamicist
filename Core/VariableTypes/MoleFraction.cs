namespace Core.VariableTypes;

public struct MoleFraction
{
	public double Value;

	public enum Relation
	{
		FractionInVapor,
		FractionInLiquid1, FractionInLiquid2,
		FractionInSolid1, FractionInSolid2,
		FractionInSystem
	}
}