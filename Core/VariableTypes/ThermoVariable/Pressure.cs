namespace Core.VariableTypes;

public class Pressure : ThermoVariable
{
    public Pressure(double value, ThermoVarRelations relation = ThermoVarRelations.Pressure)
        : base(value, relation, "Pa") { }

    public Pressure() { }

    public static implicit operator double(Pressure T) => T.Value;
    public static implicit operator Pressure(double T) => new(T);
}