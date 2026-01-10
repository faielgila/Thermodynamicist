namespace Core.VariableTypes;

public class Temperature : ThermoVariable
{
	public Temperature(double value, ThermoVarRelations relation = ThermoVarRelations.Temperature)
		: base(value, relation, "K") { }

	public Temperature() { }

	public static implicit operator double(Temperature T) => T.Value;
	public static implicit operator Temperature(double T) => new(T);
}