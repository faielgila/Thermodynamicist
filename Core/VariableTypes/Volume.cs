namespace Core.VariableTypes;

public class Volume : ThermoVariable
{
    public Volume(double value, ThermoVarRelations relation = ThermoVarRelations.RealMolar)
        : base(value, relation) { }

    public static implicit operator double(Volume T) => T.Value;
    public static implicit operator Volume(double T) => new(T);
}