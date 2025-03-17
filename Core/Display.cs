using System.Globalization;
using Core.EquationsOfState;
using Core.VariableTypes;

namespace Core;

public static class Display
{
	/// <summary>
	/// Generates a text-form of a number in engineering notation.
	/// </summary>
	public static string ToEngrNotation(this double number)
	{
		if (number == 0) return number.ToString(CultureInfo.CurrentCulture);
		if (Double.IsNaN(number)) return 0.ToString(CultureInfo.CurrentCulture);

		int exponent = (int) Math.Floor(Math.Log10(Math.Abs(number)) / 3) * 3;
		var mantissa = number / Math.Pow(10, exponent);
		if (exponent == 0) return number.ToString(CultureInfo.CurrentCulture);
		
		return mantissa + "×10" + exponent.IntToSuperscript();
	}

	/// <summary>
	/// Generates a text-form of a number rounded to the specified significant figures in engineering notation.
	/// </summary>
	public static string ToEngrNotation(this double number, int sigfigs)
	{
		return ToEngrNotation(number.RoundToSigfigs(sigfigs));
	}

	/// <summary>
	/// Rounds a number to a given number of significant figures.
	/// </summary>
	public static double RoundToSigfigs(this double d, int sigfigs)
	{
		if (d == 0)
			return 0;

		double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
		return scale * Math.Round(d / scale, sigfigs);
	}

	/// <summary>
	/// Generates a list of the values of every state variable for the system.
	/// </summary>
	/// <param name="EoS">Equation of state, contains equations for calculating state variables</param>
	/// <param name="T">Temperature of the system, in [K]</param>
	/// <param name="P">Pressure of the system, in [P]</param>
	/// <param name="VMol">Molar volume of the system, in [m³/mol]</param>
	public static string GetAllStateVariablesFormatted(EquationOfState EoS, Temperature T, Pressure P, Volume VMol)
	{
		var (Z, U, H, S, G, A, f) = EoS.GetAllStateVariables(T, P, VMol);

		var formatted =
			"Z = " + Z.ToEngrNotation() + " \n" +
			"V = " + VMol.ToEngrNotation() + " \n" +
			"U = " + U.ToEngrNotation() + " \n" +
			"H = " + H.ToEngrNotation() + " \n" +
			"S = " + S.ToEngrNotation() + " \n" +
			"G = " + G.ToEngrNotation() + " \n" +
			"A = " + A.ToEngrNotation() + " \n" +
			"φ = " + f.ToEngrNotation();

		return formatted;
	}

	/// <summary>
	/// Generates a list of the values of every state variable for the system rounded to the significant figures provided.
	/// </summary>
	/// <param name="EoS">Equation of state, contains equations for calculating state variables</param>
	/// <param name="T">Temperature of the system, in [K]</param>
	/// <param name="P">Pressure of the system, in [P]</param>
	/// <param name="VMol">Molar volume of the system, in [m³/mol]</param>
	public static string GetAllStateVariablesFormatted(EquationOfState EoS, Temperature T, Pressure P, Volume VMol, int sigfigs)
	{
		var (Z, U, H, S, G, A, f) = EoS.GetAllStateVariables(T, P, VMol);

		var formatted =
			"Z = " + Z.ToEngrNotation() + " \n" +
			"V = " + VMol.ToEngrNotation() + " \n" +
			"U = " + U.ToEngrNotation() + " \n" +
			"H = " + H.ToEngrNotation() + " \n" +
			"S = " + S.ToEngrNotation() + " \n" +
			"G = " + G.ToEngrNotation() + " \n" +
			"A = " + A.ToEngrNotation() + " \n" +
			"φ = " + f.ToEngrNotation();

		return formatted;
	}

	/// <summary>
	/// Returns the superscript Unicode character for the integer provided.
	/// </summary>
	private static string IntToSuperscript(this int exp)
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

	public static class Colors
	{
		/// <summary>
		/// Assigns a hue to each temperature relative to the critical temperature.
		/// </summary>
		/// <param name="T">Temperature, in [K]</param>
		/// <param name="critT">Critical temperature, in [K]</param>
		/// <returns>Blue if 50 K below critical or more, red if at critical,
		/// purple if 25 K above critical or more, interpolated colors inbetween.</returns>
		public static double HueTemperatureMap(Temperature T, Temperature critT)
		{
			double t = T - critT;
			if (t <= -50) return 0.5;
			if (t >= 25) return 0.75;
			if (t > -50 && t < 0) return -t / 100;
			if (t > 0 && t < 25) return 1 - t / 100;
			else return 0;
		}
	}
}