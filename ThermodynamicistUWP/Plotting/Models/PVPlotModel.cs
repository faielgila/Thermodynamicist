using OxyPlot;
using OxyPlot.Axes;
using Core;
using Core.VariableTypes;
using Core.EquationsOfState;
using ThermodynamicistUWP.Plotting;
using System.Threading.Tasks;

namespace ThermodynamicistUWP
{
	public class PVPlotModel
	{
		public PlotModel Model { get; private set; }

		public EquationOfState EoS { get; set; }

		private bool ShowSCurves { get; set; }

		public PVPlotModel(EquationOfState equationOfState, bool showSCurves = false)
		{
			EoS = equationOfState;
			ShowSCurves = showSCurves;
			Update();
		}

		public void Update()
		{
			Model = new PlotModel { Title = Constants.ChemicalNames[EoS.Species] + " pressure-volume isotherms" };
			var critT = EoS.speciesData.critT;

			// Generate a list of temperatures to plot isotherms for.
			var temps = new LinearEnumerable(critT-50, critT+10, 10);

			// P-V plots do not work well for the MSLV EoS.
			if (EoS.GetType().Name == "ModSolidLiquidVaporEOS") return;

			// Add the pressure-volume (true) isotherm for each temperature to the plot. Uses parallelization.
			Parallel.ForEach(temps, T => Model.Series.Add(PVPlotFunctionFactory.FS_Isotherm(EoS, T)));

			// Add the pressure-volume s-curve isotherm for each temperature to the plot. Uses parallelization.
			Parallel.ForEach(temps, T => { if (T < critT) Model.Series.Add(PVPlotFunctionFactory.FS_IsothermSCurve(EoS, T, ShowSCurves)); });

			// Add the vapor-liquid equilibrium region to the plot.
			Model.Series.Add(PVPlotFunctionFactory.LS_VaporLiquidEqRegion(EoS));

			// Define a maximum molar volume to stop plotting.
			Volume maxVMol = 0.001;

			// Add axes.
			Model.Axes.Add(new LinearAxis {
				Position = AxisPosition.Bottom, Minimum = 3e-5, Maximum = maxVMol, Title = "Molar Volume [m³/mol]"
			});
			Model.Axes.Add(Common.PressureLogarithmicAxis());
		}
	}
}