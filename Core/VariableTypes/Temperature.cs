namespace Core.VariableTypes;

public struct Temperature
{
	private readonly double _value;

	public static implicit operator double(Temperature T) => T._value;
	public static implicit operator Temperature(double T) => new Temperature(T);

	private Temperature(double value) { _value = value; }

	public enum Relation
	{
		Temperature
	}
}