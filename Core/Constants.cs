using System.Collections.ObjectModel;
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
	
	public static Dictionary<Chemical,
		(double molarMass, Temperature critT, Pressure critP, double acentricFactor, Temperature boilT)
	> ChemicalData = new()
	{
		[Chemical.Ammonia] = (17.031, 405.6, 11.28e6, 0.184, 239.7),
		[Chemical.Benzene] = (78.114, 562.1, 4.894e6, 0.212, 353.3),
		[Chemical.NButane] = (58.124, 425.2, 3.800e6, 0.193, 272.7),
		[Chemical.Isobutane] = (58.124, 408.1, 3.648e6, 0.176, 261.3),
		[Chemical.CarbonDioxide] = (44.010, 304.2, 7.376e6, 0.225, 194.7),
		[Chemical.CarbonMonoxide] = (28.010, 132.9, 3.496e6, 0.049, 81.7),
		[Chemical.Ethane] = (30.070, 305.4, 4.884e6, 0.098, 184.5),
		[Chemical.Hydrogen] = (2.016, 33.2, 1.297e6, -0.22, 20.4),
		[Chemical.HydrogenFluoride] = (20.006, 461.0, 6.488e6, 0.372, 292.7),
		[Chemical.HydrogenSulfide] = (34.080, 373.2, 8.942e6, 0.100, 212.8),
		[Chemical.Methane] = (16.043, 190.6, 4.600e6, 0.008, 111.7),
		[Chemical.Nitrogen] = (28.013, 126.2, 3.394e6, 0.040, 77.4),
		[Chemical.Oxygen] = (31.999, 154.6, 5.046e6, 0.021, 90.2),
		[Chemical.NPentane] = (72.151, 469.6, 3.374e6, 0.251, 309.2),
		[Chemical.Isopentane] = (72.151, 460.4, 3.384e6, 0.227, 301.1),
		[Chemical.Propane] = (44.097, 369.8, 4.246e6, 0.152, 231.1),
		[Chemical.R12] = (120.914, 385.0, 4.124e6, 0.176, 243.4),
		[Chemical.R134a] = (102.03, 374.23, 4.060e6, 0.332, 247.1),
		[Chemical.SulfurDioxide] = (64.063, 430.8, 7.883e6, 0.251, 263),
		[Chemical.Toluene] = (92.941, 591.7, 4.113e6, 0.257, 383.8),
		[Chemical.Water] = (18.015, 647.3, 22.048e6, 0.344, 165.0),
	};

	/// <summary>
	/// Stores thermodynamic parameters for a specified chemical and EoS.
	/// TODO: use a dictionary to store and lookup thermodynamic parameters for multiple chemicals simultaneously.
	/// </summary>
	public static class ThermoParams
	{
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
	}
}