using Core;
using Core.EquationsOfState;
using Core.VariableTypes;
using OxyPlot;
using OxyPlot.Series;
using System;

namespace ThermodynamicistUWP.Plotting
{

	public static class FunctionFactory
	{
		/// <summary>
		/// Creates a double-to-double function which represents an isotherm
		/// in pressure-volume space.
		/// </summary>
		/// <param name="EoS">Equation of State, stores species and reference state</param>
		/// <param name="T">Temperature, in [K]</param>
		/// <param name="usePvap">Option to ignore vapor pressure in favor of the s-curve in the VLE region.</param>
		public static Func<double, double> PVIsotherm(EquationOfState EoS, Temperature T, bool usePvap = true)
		{
			// If set to ignore vapor pressure, simply return the uncorrected isotherm.
			if (!usePvap) return VMol => EoS.Pressure(T, VMol);

			var critT = EoS.speciesData.critT;
			// Vapor pressures (and VLE) exist only below the critical temperature.
			if (T < critT)
			{
				var Pvap = EoS.VaporPressure(T);
				var (VMol_L, VMol_V) = EoS.PhaseFinder(T, Pvap);
				return VMol =>
				{
					// Inside the s-curve region, replace the function value with the vapor pressure
					if (VMol > VMol_L && VMol < VMol_V) { return Pvap; }
					// Otherwise, the isotherm is unchanged.
					else return EoS.Pressure(T, VMol);
				};
			}
			else
			{
				// Above the critical temperature, simply return the true isotherm.
				return VMol => EoS.Pressure(T, VMol);
			}
		}

        /// <summary>
        /// Generates a FunctionSeries for use in OxyPlot representing an isotherm
        /// in pressure-volume space.
		/// Adds colors using <see cref="Display.Colors.HueTemperatureMap"/>.
		/// If <paramref name="usePvap"/> is false, the Series is dotted to show non-real behavior.
        /// </summary>
        /// <param name="EoS">Equation of State, stores species and reference state</param>
        /// <param name="T">Temperature, in [K]</param>
        /// <param name="usePvap">Option to ignore vapor pressure in favor of the s-curve in the VLE region.</param>
        public static FunctionSeries FS_PVIsotherm(EquationOfState EoS, Temperature T, bool usePvap = true)
		{
            var FS = new FunctionSeries(PVIsotherm(EoS, T, usePvap), 3e-5, 5e-4, 500, "T = " + (double)T + "K")
            {
                Color = OxyColor.FromHsv(new double[] { Display.Colors.HueTemperatureMap(T, EoS.speciesData.critT), 1, 1 })
            };
            if (!usePvap) FS.Dashes = new double[] { 1, 2, 3 };
			return FS;
		}
	}
}
