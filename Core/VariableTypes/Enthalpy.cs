namespace Core.VariableTypes;

public class Enthalpy : ThermoVariable
{
    public Enthalpy(double value, ThermoVarRelations relation = ThermoVarRelations.RealMolar)
        : base(value, relation) { }

    public static implicit operator double(Enthalpy T) => T.Value;
    public static implicit operator Enthalpy(double T) => new(T);
}