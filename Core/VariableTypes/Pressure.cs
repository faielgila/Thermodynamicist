namespace Core.VariableTypes;

public struct Pressure
{
	private readonly double _value;
	
	public static implicit operator double(Pressure P) => P._value;
	public static implicit operator Pressure(double P) => new Pressure(P);

	private Pressure(double value) { _value = value; }

	public enum Relation
	{
		Pressure,
		VaporPressure,
		PartialPressure
	}
}