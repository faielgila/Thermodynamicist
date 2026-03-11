namespace Core.VariableTypes;

public class MassFraction : ThermoVariable
{
    public MassFraction(double value, ThermoVarRelations relation = ThermoVarRelations.Undefined)
        : base(value, relation, "") { }

    public MassFraction() { }

    public static implicit operator double(MassFraction T) => T.Value;
    public static implicit operator MassFraction(double T) => new(T);
}