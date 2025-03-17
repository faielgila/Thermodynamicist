namespace Core.VariableTypes;

public class HelmholtzEnergy : ThermoVariable
{
    public HelmholtzEnergy(double value, ThermoVarRelations relation = ThermoVarRelations.RealMolar)
        : base(value, relation, "J/mol") { }

    public static implicit operator double(HelmholtzEnergy T) => T.Value;
    public static implicit operator HelmholtzEnergy(double T) => new(T);
}