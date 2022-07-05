using System.Runtime.CompilerServices;
using Core.VariableTypes;

namespace Core.EquationsOfState;

public class VanDerWaalsEOS : CubicEquationOfState
{
	private double a;
	private double b;
	
	public VanDerWaalsEOS(Chemical species) : base(species)
	{
		speciesData = Constants.ChemicalData[Species];
		a = 27 * Math.Pow(R * speciesData.critT, 2) / (64 * speciesData.critP);
		b = R * speciesData.critT / (8 * speciesData.critP);
	}

	private double A(Temperature T, Pressure P) { return a * P / R / R / T / T; }
	private double B(Temperature T, Pressure P) { return b * P / R / T; }
	public override Pressure Pressure(Temperature T, MolarVolume VMol)
	{
		return R * T / (VMol - b) - a / (VMol * VMol);
	}

	// from Sandler, eqn 7.4-13
	public override double FugacityCoeff(Temperature T, Pressure P, MolarVolume VMol)
	{
		var z = CompressibilityFactor(T, P, VMol);
		var A = this.A(T, P);
		var B = this.B(T, P);
		return Math.Exp(z - 1 - Math.Log(z - B) - A / z);
	}

	#region Cubic and related equations

	public override double ZCubicEqn(Temperature T, Pressure P, MolarVolume VMol)
	{
		var z = CompressibilityFactor(T, P, VMol);
		var A = this.A(T, P);
		var B = this.B(T, P);
		var term3 = Math.Pow(z, 3);
		var term2 = (-1 - B) * z * z;
		var term1 = A * z;
		var term0 = -A * B;
		return term0 + term1 + term2 + term3;
	}

	public override double ZCubicDerivative(Temperature T, Pressure P, MolarVolume VMol)
	{
		var prt = P / (R * T);
		var term2 = 3 * Math.Pow(prt * VMol, 2) * prt;
		var term1 = -2 * prt * prt * VMol * (1 + b * prt) ;
		var term0 = a * P * P / Math.Pow(R * T, 3);
		return term2 + term1 + term0;
	}

	public override double ZCubicInflectionPoint(Temperature T, Pressure P)
	{
		return R * T / (3 * P) + b / 3;
	}
	
	#endregion

	#region Depature functions

	public override MolarEnthalpy DepartureEnthalpy(Temperature T, Pressure P, MolarVolume VMol)
	{
		throw new NotImplementedException();
	}

	public override MolarEntropy DepartureEntropy(Temperature T, Pressure P, MolarVolume VMol)
	{
		throw new NotImplementedException();
	}

	#endregion
}