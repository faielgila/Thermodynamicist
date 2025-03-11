using Core.VariableTypes;

namespace Core.EquationsOfState;

/// <summary>
/// Represents the Peng-Robinson equation of state and related functions.
/// Extends <see cref="CubicEquationOfState"/>.
/// </summary>
public class PengRobinsonEOS : CubicEquationOfState
{
	private readonly double b;
	private readonly double Kappa;

	public PengRobinsonEOS(Chemical species) : base(species)
	{
		var acentricFactor = speciesData.acentricFactor;
		Kappa = 0.37464 + (1.54226 - 0.26992 * acentricFactor) * acentricFactor;
		b = 0.07780 * R * speciesData.critT / speciesData.critP; 
	}

	#region Parameters

	/// <summary>
	/// Calculates the parameter "a" for the PR EoS.
	/// </summary>
	/// <param name="T">temperature, in [K]</param>
	/// <returns>unitless empirical constant</returns>
	private double a(Temperature T)
	{
		var critT = speciesData.critT;
		var critP = speciesData.critP;
		var aleph = 0.45724 * Math.Pow(R * critT, 2) / critP;
		return aleph * Alpha(T);
	}
	
	// The following four methods evaluate expressions that are common in equations derived from the Peng-Robinson EoS.
	private double A(Temperature T, Pressure P) { return a(T) * P / R / R / T / T; }
	private double B(Temperature T, Pressure P) { return b * P / R / T; }
	private double Alpha(Temperature T)
	{
		return Math.Pow(1 + Kappa * (1 - Math.Sqrt(T / speciesData.critT)), 2);
	}
	private double Da(Temperature T)
	{
		var critT = speciesData.critT;
		var critP = speciesData.critP;
		return -0.45724 * Math.Pow(R * critT, 2) / critP * Kappa * Math.Sqrt(Alpha(T) / critT / T);
	}

	#endregion

	public override Pressure Pressure(Temperature T, Volume VMol)
	{
		return R * T / (VMol - b) - a(T) / (VMol * VMol + 2 * b * VMol - b * b);
	}

	// from Sandler, eqn 7.4-14
	public override double FugacityCoeff(Temperature T, Pressure P, Volume VMol)
	{
		var sqrt2 = Math.Sqrt(2);
		var z = CompressibilityFactor(T, P, VMol);
		var A = this.A(T, P);
		var B = this.B(T, P);
		var termLog = (z + B + sqrt2 * B) / (z + B - sqrt2 * B);
		var LogFugacityCoeff = z - 1 - Math.Log(z - B) - A / (2 * sqrt2 * B) * Math.Log(termLog);
		return Math.Exp(LogFugacityCoeff);
	}

	#region Partial derivatives
	public override double PVPartialDerivative(Temperature T, Volume VMol)
	{
		return -R * T / Math.Pow(VMol - b, 2) + 2 * a(T) * (VMol + b) / Math.Pow(VMol * VMol + 2 * b * VMol - b * b, 2);
	}

	#endregion

	#region Cubic form and derivatives
	public override double ZCubicEqn(Temperature T, Pressure P, Volume VMol)
	{
		var z = CompressibilityFactor(T, P, VMol);
		var A = this.A(T, P);
		var B = this.B(T, P);
		var term3 = z * z * z;
		var term2 = (-1 + B) * z * z;
		var term1 = (A - 3 * B * B - 2 * B) * z;
		var term0 = -A * B + B * B + B * B * B;
		return term3 + term2 + term1 + term0;
	}

	public override double ZCubicDerivative(Temperature T, Pressure P, Volume VMol)
	{
		var z = CompressibilityFactor(T, P, VMol);
		var A = this.A(T, P);
		var B = this.B(T, P);
		return 3*z*z + (-1 + B) * 2*z + (A - 3*B*B - 2*B);
	}

	public override Volume ZCubicInflectionPoint(Temperature T, Pressure P)
	{
		return new Volume( R*T/3/P - b/3);
	}

	#endregion

	#region Departure functions

	// from Sandler, eqn 6.4-29
	public override Enthalpy DepartureEnthalpy(Temperature T, Pressure P, Volume VMol)
	{
		var sqrt2 = Math.Sqrt(2);
		var da = this.Da(T);
		var a = this.a(T);
		var z = CompressibilityFactor(T, P, VMol);
		var B = this.B(T, P);
		var logPiece = (z + B + sqrt2 * B) / (z + B - sqrt2 * B);
		var value = R * T * (z - 1) + (T * da - a) / (2 * sqrt2 * b) * Math.Log(logPiece);
		return new Enthalpy(value, ThermoVarRelations.Departure);
	}

	// from Sandler, eqn 6.4-30
	public override Entropy DepartureEntropy(Temperature T, Pressure P, Volume VMol)
	{
		var sqrt2 = Math.Sqrt(2);
		var da = this.Da(T);
		var z = CompressibilityFactor(T, P, VMol);
		var B = this.B(T, P);
		var logPiece = (z + B + sqrt2 * B) / (z + B - sqrt2 * B);
		var value = R * Math.Log(z - B) + da / (2 * sqrt2 * b) * Math.Log(logPiece);
		return new Entropy(value, ThermoVarRelations.Departure);
	}
	
	#endregion

}