using Core;
using Core.EquationsOfState;
using Core.VariableTypes;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThermodynamicistUWP.Plotting
{

	public static class FunctionFactory
	{
		/// <summary>
		/// Creates a double-to-double function which represents a true isotherm (i.e., with vapor pressure instead of the s-curve)
		/// in pressure-volume space.
		/// </summary>
		/// <param name="EoS">Equation of State, stores species and reference state</param>
		/// <param name="T">Temperature, in [K]</param>
		/// <param name="usePvap">Option to ignore vapor pressure in favor of the s-curve </param>
		public static Func<double, double> PVIsotherm(EquationOfState EoS, Temperature T, bool usePvap = true)
		{
			if (!usePvap) return VMol => EoS.Pressure(T, VMol);

			var critT = EoS.speciesData.critT;
			if (T < critT)
			{
				var Pvap = EoS.VaporPressure(T);
				var (VMol_L, VMol_V) = EoS.PhaseFinder(T, Pvap);
				return VMol =>
				{
					if (VMol > VMol_L && VMol < VMol_V) { return Pvap; }
					else return EoS.Pressure(T, VMol);
				};
			}
			else
			{
				return VMol => EoS.Pressure(T, VMol);
			}
		}
	
		public static FunctionSeries FS_PVIsotherm(EquationOfState EoS, Temperature T, bool usePvap = true)
		{
			double t = T - EoS.speciesData.critT;
			double hue = -.012 * t * t / 360 - 1.8 * t / 360 + 120/360;
			var FS = new FunctionSeries(PVIsotherm(EoS, T, usePvap), 3e-5, 5e-4, 500, "T = " + (double)T + "K");
			FS.Color = OxyColor.FromHsv(new double[] { hue, 1, 1 });
			if (!usePvap) FS.Dashes = new double[] { 1, 2, 3 };
			return FS;
		}
	}
}
