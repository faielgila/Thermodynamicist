using Core.VariableTypes;

namespace Core.EquationsOfState;

public class PengRobinsonEOS : CubicEquationOfState
{
	private double Kappa = Constants.ThermoParams.PengRobinson.Kappa;
	private Temperature CritTemp = Constants.ThermoParams.PengRobinson.GetCriticalPoint().Item1;
	private Pressure CritPres = Constants.ThermoParams.PengRobinson.GetCriticalPoint().Item2;
	private double b = Constants.ThermoParams.PengRobinson.B;
	private double a(Temperature T)
	{
		return Constants.ThermoParams.PengRobinson.A(T);
	}
	
	public PengRobinsonEOS(Chemical species) : base(species) { }

	public override Pressure Pressure(Temperature T, MolarVolume VMol)
	{
		return R * T / (VMol - b) - a(T) / (VMol * VMol + 2 * b * VMol - b * b);
	}

	public override double FugacityCoeff(Temperature T, Pressure P, MolarVolume VMol)
	{
		throw new NotImplementedException();
	}

	public override double ZCubicEqn(Temperature T, Pressure P, MolarVolume VMol)
	{
		var z = P * VMol / R / T;
		var A = a(T) * P / (R * R * T * T);
		var B = b * P / R / T;
		var term3 = z * z * z;
		var term2 = (-1 + B) * z * z;
		var term1 = (A - 3 * B * B - 2 * B) * z;
		var term0 = -A * B + B * B + B * B * B;
		return term3 + term2 + term1 + term0;
	}

	public override double ZCubicDerivative(Temperature T, Pressure P, MolarVolume VMol)
	{
		throw new NotImplementedException();
	}

	public override double ZCubicInflectionPoint(Temperature T, Pressure P)
	{
		throw new NotImplementedException();
	}
}