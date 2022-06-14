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
	public const double PrecisionLimit = 1e-15;

	/// <summary>
	/// Stores basic chemical data such as molar mass, critical point, acentric factor, and boiling point as a Tuple.
	/// </summary>
	// from Sandler, table 6.6-1
	public static readonly Dictionary<Chemical,
		(double molarMass, Temperature critT, Pressure critP, double acentricFactor, Temperature boilT)
	> ChemicalData = new()
	{
		[Chemical.Ammonia]          = (17.031, 405.6, 11.28e6, 0.184, 239.7),
		[Chemical.Benzene]          = (78.114, 562.1, 4.894e6, 0.212, 353.3),
		[Chemical.NButane]          = (58.124, 425.2, 3.800e6, 0.193, 272.7),
		[Chemical.Isobutane]        = (58.124, 408.1, 3.648e6, 0.176, 261.3),
		[Chemical.CarbonDioxide]    = (44.010, 304.2, 7.376e6, 0.225, 194.7),
		[Chemical.CarbonMonoxide]   = (28.010, 132.9, 3.496e6, 0.049, 81.7),
		[Chemical.Ethane]           = (30.070, 305.4, 4.884e6, 0.098, 184.5),
		[Chemical.Hydrogen]         = (2.016, 33.2, 1.297e6, -0.22, 20.4),
		[Chemical.HydrogenFluoride] = (20.006, 461.0, 6.488e6, 0.372, 292.7),
		[Chemical.HydrogenSulfide]  = (34.080, 373.2, 8.942e6, 0.100, 212.8),
		[Chemical.Methane]          = (16.043, 190.6, 4.600e6, 0.008, 111.7),
		[Chemical.Nitrogen]         = (28.013, 126.2, 3.394e6, 0.040, 77.4),
		[Chemical.Oxygen]           = (31.999, 154.6, 5.046e6, 0.021, 90.2),
		[Chemical.NPentane]         = (72.151, 469.6, 3.374e6, 0.251, 309.2),
		[Chemical.Isopentane]       = (72.151, 460.4, 3.384e6, 0.227, 301.1),
		[Chemical.Propane]          = (44.097, 369.8, 4.246e6, 0.152, 231.1),
		[Chemical.R12]              = (120.914, 385.0, 4.124e6, 0.176, 243.4),
		[Chemical.R134a]            = (102.03, 374.23, 4.060e6, 0.332, 247.1),
		[Chemical.SulfurDioxide]    = (64.063, 430.8, 7.883e6, 0.251, 263),
		[Chemical.Toluene]          = (92.941, 591.7, 4.113e6, 0.257, 383.8),
		[Chemical.Water]            = (18.015, 647.3, 22.048e6, 0.344, 165.0)
	};
	
	/// <summary>
	/// Stores ideal constant-pressure molar heat capacity constants and the accuracy limits.
	/// First array stores the coefficients for the equation Cp* = a + b*T + c*T² + d*T³, for Cp* in J/(mol*K),
	/// second array stores temperature range for which the equation is accurate over.
	/// For some chemicals, alternative coefficients that are usable for extreme temperatures are available in
	/// the <see cref="HighTempIdealGasCpConstants"/> dictionary.
	/// </summary>
	// from Sandler, appendix A.II
	public static readonly Dictionary<Chemical, (double[] vals, double[] lims)> IdealGasCpConstants = new()
	{
		[Chemical.Ammonia]          = (new [] {27.551, 2.563e-2, 0.990e-5, -6.687e-9}, new double[] {273, 1500}),
		[Chemical.Benzene]          = (new [] {-36.193, 48.444e-2, -31.548e-5, 77.573e-9}, new double[] {273, 1500}),
		[Chemical.NButane]          = (new [] {3.954, 37.126e-2, -18.326e-5, 34.979e-9}, new double[] {273, 1500}),
		[Chemical.Isobutane]        = (new [] {-7.908, 41.573e-2, -22.992e-5, 49.875e-9}, new double[] {273, 1500}),
		[Chemical.CarbonDioxide]    = (new [] {22.243, 5.977e-2, -3.499e-5, 7.464e-9}, new double[] {273, 1800}),
		[Chemical.CarbonMonoxide]   = (new [] {28.142, 0.167e-2, 0.537e-5, -2.221e-9}, new double[] {273, 1800}),
		[Chemical.Ethane]           = (new [] {6.895, 17.255e-2, -6.402e-5, 7.280e-9}, new double[] {273, 1500}),
		[Chemical.Hydrogen]         = (new [] {29.088, -0.192e-2, 0.400e-5, -0.870e-9}, new double[] {273, 1800}),
		[Chemical.HydrogenFluoride] = (new [] {30.130, -0.493e-2, 0.659e-5, -1.573e-9}, new double[] {273, 2000}),
		[Chemical.HydrogenSulfide]  = (new [] {29.582, 1.309e-2, 0.571e-5, -3.292e-9}, new double[] {273, 1800}),
		[Chemical.Methane]          = (new [] {19.875, 5.021e-2, 1.2683-5, -11.004e-9}, new double[] {273, 1500}),
		[Chemical.Nitrogen]         = (new [] {28.883, -0.157e-2, 0.808e-5, -2.871e-9}, new double[] {273, 1800}),
		[Chemical.Oxygen]           = (new [] {25.460, 1.519e-2, -0.715e-5, 1.311e-9}, new double[] {273, 1800}),
		[Chemical.NPentane]         = (new [] {6.770, 45.398e-2, -22.448e-5, 42.459e-9}, new double[] {273, 1500}),
		[Chemical.Propane]          = (new [] {-4.042, 30.456e-2, -15.711e-5, 31.716e-9}, new double[] {273, 1500}),
		[Chemical.R12]              = (new double[] {0, 0, 0, 0}, new double[] {0, 0}),
		[Chemical.R134a]            = (new double[] {0, 0, 0, 0}, new double[] {0, 0}),
		[Chemical.SulfurDioxide]    = (new [] {25.762, 5.791e-2, -3.809e-5, 8.607e-9}, new double[] {273, 1800}),
		[Chemical.Toluene]          = (new double[] {0, 0, 0, 0}, new double[] {0, 0}),
		[Chemical.Water]            = (new [] {25.460, 1.519e-2, -0.715e-5, 1.311e-9}, new double[] {273, 1800})
	};

	/// <summary>
	/// Stores alternative coefficients that are usable for extreme temperatures (T > 1800 K).
	/// It is not recommended to use this data unless absolutely necessary, as it is less accurate for lower
	/// and more reasonable temperatures.
	/// </summary>
	// from Sandler, appendix A.II
	public static readonly Dictionary<Chemical, (double[] vals, double[] lims)> HighTempIdealGasCpConstants = new()
	{
		[Chemical.CarbonMonoxide] = (new [] {27.113, 0.655e-2, -0.100e-5, 0}, new double[] {273, 3800}),
		[Chemical.Hydrogen] = (new [] {26.879, 0.435e-2, -0.033e-5, 0}, new double[] {273, 3800}),
		[Chemical.Nitrogen] = (new [] {27.318, 0.623e-2, -0.095e-5, 0}, new double[] {273, 3800}),
		[Chemical.Oxygen]   = (new [] {28.167, 0.630e-2, -0.075e-5, 0}, new double[] {273, 3800}),
		[Chemical.Water]    = (new [] {29.163, 1.449e-2, -0.202e-5, 0}, new double[] {273, 3800}),
	};
}