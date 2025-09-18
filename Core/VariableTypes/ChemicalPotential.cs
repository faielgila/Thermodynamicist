namespace Core.VariableTypes;

public class ChemicalPotential : ThermoVariable
{
    public ChemicalPotential(double value, ThermoVarRelations relation = ThermoVarRelations.RealMolar)
        : base(value, relation, "J/mol") { }

    public ChemicalPotential() { }

    public static implicit operator double(ChemicalPotential T) => T.Value;
    public static implicit operator ChemicalPotential(double T) => new(T);
}