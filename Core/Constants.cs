using Core.VariableTypes;
using Math = System.Math;

namespace Core;

public class Constants
{
	/// <summary>
	/// Gas constant, measured in J/(mol*K)
	/// </summary>
	public const double R = 8.314;

	/// <summary>
	/// Precision limit, to be used in algorithms.
	/// </summary>
	public const double precisionLimit = 1e-12;

	/// <summary>
	/// Stores thermodynamic parameters for a specified chemical and EoS.
	/// TODO: use a dictionary to store and lookup thermodynamic parameters for multiple chemicals simultaneously.
	/// </summary>
	public static class ThermoParams
	{
		// Parameters for H₂O
		static Temperature CritTemp = 647.3;
		static Pressure CritPres = 22.048e6;
		static double AcentricFactor = 0.344;

		/// <summary>
		/// Constant-pressure ideal gas molar heat capacity constants.
		/// Uses the equation Cp* = a + b*T + c*T² + d*T³, for Cp* in J/(mol*K)
		/// Accurate within the temperature range specified in <see cref="CpIGLimits"/>
		/// </summary>
		static double[] CpIGConstants =
			{ 25.460, 1.519e-2, -0.715e-5, 1.311e-9 };

		/// <summary>
		/// Valid temperature range for <see cref="CpIGConstants"/> in Kelvin.
		/// Calculations of Cp* outside this range are likely inaccurate.
		/// TODO: Add a warning notification if given temperature is outside this range.
		/// </summary>
		static double[] CpIGLimits =
			{ 273, 1800 };

		/// <summary>
		/// Stores parameters for the van Der Waals equation of state.
		/// </summary>
		public static class VanDerWaals
		{
			public static double A = 27 * Math.Pow(R*CritTemp,2) / (64 * CritPres);
			public static double B = R * CritTemp / (8 * CritPres);
		}

		/// <summary>
		/// Stores parameters for the Peng-Robinson equation of state.
		/// </summary>
		public static class PengRobinson
		{
			public static double Kappa = 0.3764 + 1.54226 * AcentricFactor - 0.26992 * AcentricFactor * AcentricFactor;
			public static double B = 0.07780 * R * CritTemp / CritPres;

			public static double alpha(Temperature T)
			{
				return Math.Pow(1 + Kappa * (1 - Math.Sqrt(T / CritTemp)), 2);
			}

			public static double A(Temperature T)
			{
				return 0.45724 * Math.Pow(R * CritTemp, 2) / CritPres * alpha(T);
			}

			public static double DA(Temperature T)
			{
				return -0.45724 * Math.Pow(R * CritTemp, 2) / CritPres * Kappa * Math.Sqrt(alpha(T) / CritTemp / T);
			}

			/// <summary>
			/// Returns the critical point of the fluid.
			/// </summary>
			/// <returns>tuple of the form (T, P)</returns>
			public static Tuple<Temperature, Pressure> GetCriticalPoint()
			{
				return new Tuple<Temperature, Pressure>(CritTemp, CritPres);
			}
		}
	}
}