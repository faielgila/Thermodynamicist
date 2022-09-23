namespace Core.VariableTypes;

public class GibbsEnergy : ThermoVariable
{
    public GibbsEnergy(double value, ThermoVarRelations relation = ThermoVarRelations.RealMolar)
        : base(value, relation) { }

    public static implicit operator double(GibbsEnergy T) => T.Value;
    public static implicit operator GibbsEnergy(double T) => new(T);
}