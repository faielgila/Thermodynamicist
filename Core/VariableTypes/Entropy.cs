namespace Core.VariableTypes;

public class Entropy : ThermoVariable
{
    public Entropy(double value, ThermoVarRelations relation = ThermoVarRelations.RealMolar)
        : base(value, relation, "J/K/mol") { }

    public static implicit operator double(Entropy T) => T.Value;
    public static implicit operator Entropy(double T) => new(T);
}