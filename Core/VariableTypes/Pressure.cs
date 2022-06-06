namespace Core.VariableTypes;

public struct Pressure
{
	public double Value;

	public enum Relation
	{
		Pressure,
		VaporPressure,
		PartialPressure
	}
}