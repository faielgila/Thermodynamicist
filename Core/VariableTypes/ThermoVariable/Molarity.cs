namespace Core.VariableTypes;

public class Molarity : ThermoVariable
{
	public Molarity(double value, ThermoVarRelations relation = ThermoVarRelations.Undefined)
		: base(value, relation, "mol/L") { }

	public Molarity() { }

	public static implicit operator double(Molarity T) => T.Value;
	public static implicit operator Molarity(double T) => new(T);
}