using System.Runtime.CompilerServices;
using Core.VariableTypes;

namespace Core.EquationsOfState;

/// <summary>
/// Represents the van der Waals eqauation of state and related functions.
/// Extends <see cref="CubicEquationOfState"/>.
/// </summary>
public class VanDerWaalsEOS : CubicEquationOfState
{
	private readonly double a;
	private readonly double b;
	
	public VanDerWaalsEOS(Chemical species) : base(species)
	{
		speciesData = Constants.ChemicalData[Species];
		a = 27 * Math.Pow(R * speciesData.critT, 2) / (64 * speciesData.critP);
		b = R * speciesData.critT / (8 * speciesData.critP);
	}

	#region Parameters
	private double A(Temperature T, Pressure P) { return a * P / R / R / T / T; }
	private double B(Temperature T, Pressure P) { return b * P / R / T; }

	#endregion
	public override Pressure Pressure(Temperature T, Volume VMol)
	{
		return R * T / (VMol - b) - a / (VMol * VMol);
	}

	// from Sandler, eqn 7.4-13
	public override double FugacityCoeff(Temperature T, Pressure P, Volume VMol)
	{
		var z = CompressibilityFactor(T, P, VMol);
		var A = this.A(T, P);
		var B = this.B(T, P);
		return Math.Exp(z - 1 - Math.Log(z - B) - A / z);
	}

	#region Partial derivatives
	public override double PVPartialDerivative(Temperature T, Volume VMol)
	{
		return -R * T / Math.Pow(VMol - b, 2) + 2 * a / Math.Pow(VMol, 3);
	}

	#endregion

	public Volume IncreasingIsothermFinder(Temperature T)
    {
        var VMol = b;
		double checkVal(double v) { return 2 * a / R / T * (v - b) * (v - b) / Math.Pow(VMol, 3); }
		while (checkVal(VMol) <= 1 || Pressure(T, VMol) < 0) { VMol += precisionLimit * Math.Pow(10, 10); }
        return new Volume(VMol);
    }

    #region Cubic form and derivatives

    public override double ZCubicEqn(Temperature T, Pressure P, Volume VMol)
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

	public override double ZCubicDerivative(Temperature T, Pressure P, Volume VMol)
	{
		var prt = P / (R * T);
		var term2 = 3 * Math.Pow(prt * VMol, 2) * prt;
		var term1 = -2 * prt * prt * VMol * (1 + b * prt) ;
		var term0 = a * P * P / Math.Pow(R * T, 3);
		return term2 + term1 + term0;
	}

	public override Volume ZCubicInflectionPoint(Temperature T, Pressure P)
	{
		return new Volume(R * T / (3 * P) + b / 3);
	}
	
	#endregion

	#region Depature functions

	// from Sandler, eqn 6.4-27 solved using van der Waals EoS
	public override Enthalpy DepartureEnthalpy(Temperature T, Pressure P, Volume VMol)
	{
		return new Enthalpy(P * VMol - R * T - a / VMol, ThermoVarRelations.Departure);
	}

    // from Sandler, eqn 6.4-28 solved using van der Waals EoS
    public override Entropy DepartureEntropy(Temperature T, Pressure P, Volume VMol)
	{
		var z = CompressibilityFactor(T, P, VMol);
		return new Entropy(R * T * (z - 1) - a / VMol, ThermoVarRelations.Departure);
	}

	#endregion
}