using Core.EquationsOfState;
using Core.VariableTypes;
using Core;
using OxyPlot.Series;
using OxyPlot;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermodynamicistUWP.Plotting.Factories
{
	class GTPlotFunctionFactory
	{
		/// <summary>
		/// Generates a list of (temperature, Gibbs energy) points for a given phase.
		/// </summary>
		/// <param name="EoS">Equation of State, stores species and reference state</param>
		/// <param name="P">pressure, in [Pa]</param>
		/// <param name="phaseKey">phase, in string form</param>
		/// <returns>list of tuples, (temperature in [K], reference molar Gibbs free energy in [J/mol])</returns>
		public static List<(double T, double G)> PhaseCurve(EquationOfState EoS, Pressure P, string phaseKey)
		{
			// Initialize output list.
			var points = new ConcurrentBag<(double T, double G)>();

			var critT = EoS.speciesData.critT;
			var minTemp = critT / 100;
			var maxTemp = critT - 10;
			var temps = new LinearEnumerable(minTemp, maxTemp, 0.1);

			Parallel.ForEach(temps, T =>
			{
				// Get all phases present at the current temperature and pressure.
				var phases = EoS.PhaseFinder(T, P, ignoreEquilibrium: true);
				// If the current phase is not present, there are no points to plot.
				if (!phases.ContainsKey(phaseKey)) return;
				// Extract molar volume for the current phase.
				var VMol = phases[phaseKey];
				// Calculate molar Gibbs energy for the state and add the point to the list.
				var G = EoS.ReferenceMolarGibbsEnergy(T, P, VMol);
				points.Add((T, G));
			});

			return points.OrderBy(x => x.T).ToList();
		}

		/// <summary>
		/// Generates a LineSeries which represents a constant-pressure "slice" of the Gibbs free energy surface
		/// in pressure-temperature space.
		/// </summary>
		/// <param name="EoS">Equation of State, stores species and reference state</param>
		/// <param name="P">pressure, in [Pa]</param>
		/// <param name="phaseKey">phase, in string form</param>
		public static LineSeries LS_PhaseCurve(EquationOfState EoS, Pressure P, string phaseKey)
		{
			var line = new LineSeries
			{
				LineStyle = LineStyle.Solid,
				//Color = OxyColors.Black,
				StrokeThickness = 4,
				LineJoin = LineJoin.Round,
				Title = phaseKey
			};

			var points = PhaseCurve(EoS, P, phaseKey);
			foreach (var (T, G) in points)
			{
				line.Points.Add(new DataPoint(T, G));
			}

			return line;
		}
	}
}
