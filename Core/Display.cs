using System.Globalization;
using Core.EquationsOfState;
using Core.VariableTypes;

namespace Core;

public static class Display
{
	public static string ToEngrNotation(this double number)
	{
		if (number == 0) return number.ToString(CultureInfo.CurrentCulture);
		if (Double.IsNaN(number)) return 0.ToString(CultureInfo.CurrentCulture);

		int exponent = (int) Math.Floor(Math.Log10(Math.Abs(number)) / 3) * 3;
		var mantissa = number / Math.Pow(10, exponent);
		if (exponent == 0) return number.ToString(CultureInfo.CurrentCulture);
		
		return mantissa + "×10" + exponent.IntToSuperscript();
	}

	public static string ToEngrNotation(this double number, int sigfigs)
    {
		return ToEngrNotation(number.RoundToSigfigs(sigfigs));
    }

	public static string GetAllStateVariablesFormatted(EquationOfState EoS, Temperature T, Pressure P, Volume VMol)
	{
		var stateVars = EoS.GetAllStateVariables(T, P, VMol);
		var f = EoS.FugacityCoeff(T, P, VMol);

		var formatted =
			"Z = " + stateVars.Z.ToEngrNotation() + "\n" +
			"V = " + VMol.Value.ToEngrNotation() + " m³/mol \n" +
			"U = " + stateVars.U.Value.ToEngrNotation() + " J/mol \n" +
			"H = " + stateVars.H.Value.ToEngrNotation() + " J/mol \n" +
			"S = " + stateVars.S.Value.ToEngrNotation() + " J/mol/K \n" +
			"G = " + stateVars.G.Value.ToEngrNotation() + " J/mol/K \n" +
			"A = " + stateVars.A.Value.ToEngrNotation() + " J/mol/K \n" +
			"φ = " + f.ToEngrNotation();

		return formatted;
	}

	public static string GetAllStateVariablesFormatted(EquationOfState EoS, Temperature T, Pressure P, Volume VMol, int sigfigs)
	{
		var stateVars = EoS.GetAllStateVariables(T, P, VMol);
		var f = EoS.FugacityCoeff(T, P, VMol);

		var formatted =
			"Z = " + stateVars.Z.ToEngrNotation(sigfigs) + "\n" +
			"V = " + VMol.Value.ToEngrNotation(sigfigs) + " m³/mol \n" +
			"U = " + stateVars.U.Value.ToEngrNotation(sigfigs) + " J/mol \n" +
			"H = " + stateVars.H.Value.ToEngrNotation(sigfigs) + " J/mol \n" +
			"S = " + stateVars.S.Value.ToEngrNotation(sigfigs) + " J/mol/K \n" +
			"G = " + stateVars.G.Value.ToEngrNotation(sigfigs) + " J/mol/K \n" +
			"A = " + stateVars.A.Value.ToEngrNotation(sigfigs) + " J/mol/K \n" +
			"φ = " + f.ToEngrNotation(sigfigs);

		return formatted;
	}

	static double RoundToSigfigs(this double d, int digits)
	{
		if (d == 0)
			return 0;

		double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
		return scale * Math.Round(d / scale, digits);
	}

	static string IntToSuperscript(this int exp)
    {
		char[] chars = exp.ToString().ToArray();
		string expString = "";
        foreach (var item in chars)
        {
			expString += ExponentDict[item];
        }
		return expString;
    }

	private static readonly Dictionary<char, string> ExponentDict = new()
	{
		{ '0', "⁰" },
		{ '1', "¹" },
		{ '2', "²" },
		{ '3', "³" },
		{ '4', "⁴" },
		{ '5', "⁵" },
		{ '6', "⁶" },
		{ '7', "⁷" },
		{ '8', "⁸" },
		{ '9', "⁹" },
		{ '-', "⁻" }
	};

}