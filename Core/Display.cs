using Core.EquationsOfState;
using Core.VariableTypes;

namespace Core;

public class Display
{
	public static string DoubleToEngrNotation(double number)
	{
		var exponent = Math.Floor(Math.Log10(number) / 3) * 3;
		var mantissa = number / Math.Pow(10,exponent);
		return mantissa + "×10^" + exponent;
	}

	public static string FormatAllStateVariables(EquationOfState EoS, Temperature T, Pressure P, MolarVolume VMol)
	{
		var StateVars = EoS.GetAllStateVariables(T, P, VMol);

		var formatted =
			"Z = " + (double) StateVars.Z + "\n" +
			"U = " + (double) StateVars.U + "J/mol \n";

		return null;
		//TODO
	}
}