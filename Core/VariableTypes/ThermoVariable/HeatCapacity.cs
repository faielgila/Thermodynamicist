namespace Core.VariableTypes;

public class HeatCapacity : ThermoVariable
{
    public HeatCapacity(double value, ThermoVarRelations relation = ThermoVarRelations.Undefined)
        : base(value, relation, "J/K/mol") { }

    public HeatCapacity() { }

    public static implicit operator double(HeatCapacity T) => T.Value;
    public static implicit operator HeatCapacity(double T) => new(T);
}