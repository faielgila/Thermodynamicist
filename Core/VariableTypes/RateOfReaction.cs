namespace Core.VariableTypes;

public class RateOfReaction : ThermoVariable
{
    public RateOfReaction(double value, ThermoVarRelations relation = ThermoVarRelations.Undefined)
        : base(value, relation, "mol/L/s") { }

    public RateOfReaction() { }

    public static implicit operator double(RateOfReaction T) => T.Value;
    public static implicit operator RateOfReaction(double T) => new(T);
}