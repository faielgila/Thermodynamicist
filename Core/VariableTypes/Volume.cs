namespace Core.VariableTypes;

public struct MolarVolume
{
    public readonly double _value;

    public static implicit operator double(MolarVolume VMol) => VMol._value;
    public static implicit operator MolarVolume(double VMol) => new MolarVolume(VMol);

    private MolarVolume(double value) { _value = value; }

    public enum Relation
    {
        MolarVolume
    }
}