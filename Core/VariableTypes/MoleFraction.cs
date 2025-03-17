namespace Core.VariableTypes;

public class MoleFraction : ThermoVariable
{
    public MoleFraction(double value, ThermoVarRelations relation = ThermoVarRelations.Undefined)
        : base(value, relation, "") { }

    public static implicit operator double(MoleFraction T) => T.Value;
    public static implicit operator MoleFraction(double T) => new(T);
}