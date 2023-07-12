using Core;
using Core.EquationsOfState;
using Core.VariableTypes;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ThermodynamicistUWP.Plotting
{

	public static class FunctionFactory
	{
		#region Pressure-Volume (PV) plotting

		/// <summary>
		/// Creates a double-to-double function which represents an isotherm in pressure-volume space.
		/// </summary>
		/// <param name="EoS">Equation of State, stores species and reference state</param>
		/// <param name="T">Temperature, in [K]</param>
		/// <param name="usePvap">Option to ignore vapor pressure in favor of the s-curve in the VLE region.</param>
		/// <returns>pressure as a function of molar volume</returns>
		public static Func<double, double> PVIsotherm(EquationOfState EoS, Temperature T, bool usePvap = true)
		{
			// If set to ignore vapor pressure, simply return the uncorrected isotherm.
			if (!usePvap) return VMol => EoS.Pressure(T, VMol);

			var critT = EoS.speciesData.critT;
			// Vapor pressures (and VLE) exist only below the critical temperature.
			if (T < critT)
			{
				var Pvap = EoS.VaporPressure(T);
				var phases = EoS.PhaseFinder(T, Pvap);
				return VMol =>
				{
					// Inside the s-curve region, replace the function value with the vapor pressure
					if (VMol > phases["liquid"] && VMol < phases["vapor"]) { return Pvap; }
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
			var FS = new FunctionSeries(PVIsotherm(EoS, T, usePvap), 3e-5, .002, 1000, "T = " + (double)T + "K")
			{
				Color = OxyColor.FromHsv(new double[] { Display.Colors.HueTemperatureMap(T, EoS.speciesData.critT), 1, 1 })
			};
			return FS;
		}

		/// <summary>
		/// Generates a FunctionSeries which represents the s-curve region of an isotherm in pressure-volume space,
		/// to be used in OxyPlot.
		/// </summary>
		/// <param name="EoS">Equation of State, stores species and reference state</param>
		/// <param name="T">Temperature, in [K]</param>
		/// <returns>FunctionSeries with pressure as a function of molar volume</returns>
		/// <exception cref="ArgumentOutOfRangeException">The s-curve region only exists for isotherms below the critical temperature.</exception>
		public static FunctionSeries FS_PVIsothermSCurve(EquationOfState EoS, Temperature T, bool isVisible)
		{
			// Vapor pressures (and the VLE) exist only below the critical temperature.
			if (T >= EoS.speciesData.critT) throw new ArgumentOutOfRangeException("S-curves only exist for temperatures below the critical temperature!");

			var Pvap = EoS.VaporPressure(T);
			var EqVMol = EoS.PhaseFinder(T, Pvap);

			var FS = new FunctionSeries(PVIsotherm(EoS, T, false), EqVMol["liquid"], EqVMol["vapor"], 50)
			{
				Color = OxyColor.FromHsv(new double[] { Display.Colors.HueTemperatureMap(T, EoS.speciesData.critT), 1, 1 }),
				Dashes = new double[] { 1, 2, 3 },
				IsVisible = isVisible
			};
			return FS;
		}

		/// <summary>
		/// Generates a list of (volume, pressure) points on the boundary of the liquid-vapor coexistance region.
		/// </summary>
		/// <param name="EoS">Equation of State, stores species and reference state</param>
		/// <returns>list of tuples, (molar volume in [m³/mol], pressure in [Pa])</returns>
		public static List<(double VMol, double P)> PVVaporLiquidEqRegion(EquationOfState EoS)
		{
			// Initialize the output list.
			var points = new List<(double VMol, double P)>();

			var critT = EoS.speciesData.critT;
			var temps = new LinearEnumerable(critT - 50, critT, 0.5);
			Parallel.ForEach(temps, T => {
				if (T < critT)
				{
					var Pvap = EoS.VaporPressure(T);
					var phases = EoS.PhaseFinder(T, Pvap);
					points.Add((phases["liquid"], Pvap));
					points.Add((phases["vapor"], Pvap));
				}
			});

			// Separately add the critical point to the plot.
			points.Add((EoS.CriticalMolarVolume(), EoS.speciesData.critP));

			// The curve will consist of randomly-ordered points, so for proper plotting sort by increasing VMol.
			points.Sort();

			return points;
		}

		/// <summary>
		/// Generates a LineSeries which represents the vapor-liquid coexistance region in pressure-volume space.
		/// </summary>
		/// <param name="EoS">Equation of State, stores species and reference state</param>
		public static LineSeries LS_PVVaporLiquidEqRegion(EquationOfState EoS)
		{
			var line = new LineSeries
			{
				LineStyle = LineStyle.Dash,
				Color = OxyColors.Black,
				StrokeThickness = 4,
				LineJoin = LineJoin.Round,
			};

			var points = PVVaporLiquidEqRegion(EoS);
			foreach (var (VMol, P) in points)
			{
				line.Points.Add(new DataPoint(VMol, P));
			}

			return line;
		}

		#endregion

		#region Pressure-Temperature (PT) plotting [phase diagram]

		/// <summary>
		/// Generates a list of (temperature, pressure) points describing the liquid-vapor phase boundary in pressure-temperature space.
		/// </summary>
		/// <param name="EoS">Equation of State, stores species and reference state</param>
		/// <returns>list of tuples, (molar volume in [m³/mol], pressure in [Pa])</returns>
		public static List<(double T, double P)> PTEvaporationCurve(EquationOfState EoS)
		{
			// Initialize the output list.
			var points = new List<(double T, double P)>();

			var critT = EoS.speciesData.critT;
			var temps = new LinearEnumerable(273, critT, 0.5);
			Parallel.ForEach(temps, T => {
				if (T < critT)
				{
					var Pvap = EoS.VaporPressure(T);
					points.Add((T, Pvap));
				}
			});

			// Separately add the critical point to the plot.
			points.Add((critT, EoS.speciesData.critP));

			// The curve will consist of randomly-ordered points, so for proper plotting sort by increasing VMol.
			points.Sort();

			return points;
		}

		/// <summary>
		/// Generates a LineSeries which represents the liquid-vapor phase boundary in pressure-temperature space.
		/// </summary>
		/// <param name="EoS">Equation of State, stores species and reference state</param>
		public static LineSeries LS_PTEvaporationCurve(EquationOfState EoS)
		{
			var line = new LineSeries
			{
				LineStyle = LineStyle.Solid,
				Color = OxyColors.Black,
				StrokeThickness = 4,
				LineJoin = LineJoin.Round,
			};

			var points = PTEvaporationCurve(EoS);
			foreach (var (T, P) in points)
			{
				line.Points.Add(new DataPoint(T, P));
			}

			return line;
		}

		#endregion
	}
}
