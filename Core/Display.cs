﻿using System.Globalization;
using Core.EquationsOfState;
using Core.VariableTypes;

namespace Core;

public static class Display
{
	public static string ToEngrNotation(this double number)
	{
		var exponent = Math.Floor(Math.Log10(Math.Abs(number)) / 3) * 3;
		var mantissa = number / Math.Pow(10,exponent);
		if (exponent == 0) return number.ToString(CultureInfo.CurrentCulture);
		return mantissa + "×10^" + exponent;
	}

	public static string GetAllStateVariablesFormatted(EquationOfState EoS, Temperature T, Pressure P, MolarVolume VMol)
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
			"f = " + f.ToEngrNotation() + " Pa";

		return formatted;
	}
}