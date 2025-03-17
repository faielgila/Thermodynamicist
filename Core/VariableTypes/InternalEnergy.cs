namespace Core.VariableTypes;

public class InternalEnergy : ThermoVariable
{
    public InternalEnergy(double value, ThermoVarRelations relation = ThermoVarRelations.RealMolar)
        : base(value, relation, "J/mol") { }

    public static implicit operator double(InternalEnergy T) => T.Value;
    public static implicit operator InternalEnergy(double T) => new(T);
}