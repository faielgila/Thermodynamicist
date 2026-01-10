namespace Core.VariableTypes;

public class Time : ThermoVariable
{
    public Time(double value, ThermoVarRelations relation = ThermoVarRelations.Undefined)
        : base(value, relation, "s") { }

    public Time() { }

    public static implicit operator double(Time T) => T.Value;
    public static implicit operator Time(double T) => new(T);
}