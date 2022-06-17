using Core.VariableTypes;

namespace Core.EquationsOfState;

public static class IdealGasLaw
{
	public const double R = Constants.R;
	
	public static Pressure Pressure(Temperature T, MolarVolume VMol) { return R * T / VMol; }

	public static MolarVolume MolarVolume(Temperature T, Pressure P) { return R * T / P; }
}