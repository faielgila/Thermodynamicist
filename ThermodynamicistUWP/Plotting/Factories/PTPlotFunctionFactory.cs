using Core.EquationsOfState;
using Core;
using OxyPlot.Series;
using OxyPlot;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ThermodynamicistUWP.Plotting.Factories
{
	class PTPlotFunctionFactory
	{
		/// <summary>
		/// Generates a list of (temperature, pressure) points describing the liquid-vapor phase boundary in pressure-temperature space.
		/// </summary>
		/// <param name="EoS">Equation of State, stores species and reference state</param>
		/// <returns>list of tuples, (molar volume in [m³/mol], pressure in [Pa])</returns>
		public static List<(double T, double P)> EvaporationCurve(EquationOfState EoS)
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
		public static LineSeries LS_EvaporationCurve(EquationOfState EoS)
		{
			var line = new LineSeries
			{
				LineStyle = LineStyle.Solid,
				Color = OxyColors.Black,
				StrokeThickness = 4,
				LineJoin = LineJoin.Round,
			};

			var points = EvaporationCurve(EoS);
			foreach (var (T, P) in points)
			{
				line.Points.Add(new DataPoint(T, P));
			}

			return line;
		}
	}
}
