namespace Core.Data
{
	class HeatCapacityParameters
	{
		/// <summary>
		/// Stores ideal constant-pressure molar heat capacity constants and the accuracy limits.
		/// First array stores the coefficients for the equation Cp* = a + b*T + c*T² + d*T³, etc., for Cp* in J/(mol*K),
		/// Second array stores temperature range for which the equation is accurate over.
		/// For some chemicals, alternative coefficients that are usable for extreme temperatures are available in
		/// the <see cref="HighTempIdealGasCpConstants"/> dictionary.
		/// </summary>
		// Most from Sandler, appendix A.II
		// {R12, R134a, Pentane, Toluene} from Yaw's Handbook - Heat Capacities of Gases, Organic Compounds
		// Chlorbenzene from Knovel Critical Tables (2nd Ed, 2008)
		// Chlorine from NIST Chemistry Webbook (with regression fitting from Shomate Eqn)
		public static readonly Dictionary<Chemical, (double[] vals, double[] lims)> IdealGasCpConstants = new()
		{
			[Chemical.Ammonia]			= (new[] {  27.551,  2.563e-2,   0.990e-5,   -6.687e-9 }, new double[] { 273, 1500 }),
			[Chemical.Benzene]			= (new[] { -36.193, 48.444e-2, -31.548e-5,   77.573e-9 }, new double[] { 273, 1500 }),
			[Chemical.NButane]			= (new[] {   3.954, 37.126e-2, -18.326e-5,   34.979e-9 }, new double[] { 273, 1500 }),
			[Chemical.Isobutane]		= (new[] {  -7.908, 41.573e-2, -22.992e-5,   49.875e-9 }, new double[] { 273, 1500 }),
			[Chemical.CarbonDioxide]	= (new[] {  22.243,  5.977e-2,  -3.499e-5,    7.464e-9 }, new double[] { 273, 1800 }),
			[Chemical.CarbonMonoxide]	= (new[] {  28.142,  0.167e-2,   0.537e-5,   -2.221e-9 }, new double[] { 273, 1800 }),
			[Chemical.Chlorine]			= (new[] {  31.023,  1.555e-2,  -1.382e-5,   4.6817e-9 }, new double[] { 298, 1000 }),
			[Chemical.Chlorobenzene]	= (new[] {  4.9586,      0.16,  1.1484e-5 },			  new double[] { 298, 1000 }),
			[Chemical.Ethane]			= (new[] {   6.895, 17.255e-2,  -6.402e-5,    7.280e-9 }, new double[] { 273, 1500 }),
			[Chemical.Hydrogen]			= (new[] {  29.088, -0.192e-2,   0.400e-5,   -0.870e-9 }, new double[] { 273, 1800 }),
			[Chemical.HydrogenFluoride]	= (new[] {  30.130, -0.493e-2,   0.659e-5,   -1.573e-9 }, new double[] { 273, 2000 }),
			[Chemical.HydrogenChloride]	= (new[] {  30.310, -0.762e-2,   1.326e-5,   -4.335e-9 }, new double[] { 273, 1500 }),
			[Chemical.HydrogenSulfide]	= (new[] {  29.582,  1.309e-2,   0.571e-5,   -3.292e-9 }, new double[] { 273, 1800 }),
			[Chemical.Methane]			= (new[] {  19.875,  5.021e-2,   1.2683e-5, -11.004e-9 }, new double[] { 273, 1500 }),
			[Chemical.Nitrogen]			= (new[] {  28.883, -0.157e-2,   0.808e-5,   -2.871e-9 }, new double[] { 273, 1800 }),
			[Chemical.Oxygen]			= (new[] {  25.460,  1.519e-2,  -0.715e-5,    1.311e-9 }, new double[] { 273, 1800 }),
			[Chemical.NPentane]			= (new[] {   6.770, 45.398e-2, -22.448e-5,   42.459e-9 }, new double[] { 273, 1500 }),
			[Chemical.Isopentane]		= (new[] {  68.343, -0.1485,     1.879e-3,   -3.500e-6, 3.1436e-9, -1.41672e-12, 2.55785e-16 }, new double[] { 150, 1500 }),
			[Chemical.Propane]			= (new[] {  -4.042, 30.456e-2, -15.711e-5,   31.716e-9 }, new double[] { 273, 1500 }),
			[Chemical.R12]				= (new[] {  10.279,  0.3298,    -5.34244e-4,  4.92869e-7, -2.64231e-10, 7.69436e-14, -9.4595e-18 }, new double[] { 150, 1500 }),
			[Chemical.R134a]			= (new[] {  28.367,  0.1471,     4.4138e-4,  -1.28589e-6, 1.3589e-6, -6.63533e-13, 1.24581e-16 }, new double[] { 150, 1500 }),
			[Chemical.SulfurDioxide]	= (new[] {  25.762,  5.791e-2,  -3.809e-5,    8.607e-9 }, new double[] { 273, 1800 }),
			[Chemical.Toluene]			= (new[] {  47.375, -0.2201,     2.482e-3,   -4.9176e-6, 4.60396e-9, -2.11999e-12, 3.85054e-16 }, new double[] { 150, 1500 }),
			[Chemical.Water]			= (new[] {  25.460,  1.519e-2,  -0.715e-5,    1.311e-9 }, new double[] { 273, 1800 })
		};

		/// <summary>
		/// Stores alternative coefficients that are usable for extreme temperatures (T > 1800 K).
		/// It is not recommended to use this data unless absolutely necessary, as it is less accurate for lower
		/// and more reasonable temperatures.
		/// </summary>
		// from Sandler, appendix A.II
		public static readonly Dictionary<Chemical, (double[] vals, double[] lims)> HighTempIdealGasCpConstants = new()
		{
			[Chemical.CarbonMonoxide]	= (new[] { 27.113, 0.655e-2, -0.100e-5, 0 }, new double[] { 273, 3800 }),
			[Chemical.Hydrogen]			= (new[] { 26.879, 0.435e-2, -0.033e-5, 0 }, new double[] { 273, 3800 }),
			[Chemical.Nitrogen]			= (new[] { 27.318, 0.623e-2, -0.095e-5, 0 }, new double[] { 273, 3800 }),
			[Chemical.Oxygen]			= (new[] { 28.167, 0.630e-2, -0.075e-5, 0 }, new double[] { 273, 3800 }),
			[Chemical.Water]			= (new[] { 29.163, 1.449e-2, -0.202e-5, 0 }, new double[] { 273, 3800 }),
		};
	}
}
