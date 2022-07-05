namespace Core.VariableTypes;

public struct MolarVolume
{
    private readonly double _value;
    private readonly ThermoVarRelations _relation;

    public double Value => _value;
    
    public static implicit operator double(MolarVolume VMol) => VMol._value;
    public static implicit operator MolarVolume(double VMol) => new (VMol);

    public MolarVolume(double value, ThermoVarRelations relation = ThermoVarRelations.RealMolar)
    {
        _value = value;
        _relation = relation;
    }
}