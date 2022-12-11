namespace ThermodynamicistUWP
{
	using System;

	using OxyPlot;
	using OxyPlot.Axes;
	using OxyPlot.Series;

	using Core.VariableTypes;
	using Core.EquationsOfState;
	using ThermodynamicistUWP.Plotting;
	using Core;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	public class MainViewModel
	{
		public PlotModel Model { get; private set; }

		public EquationOfState EoS { get; set; }

		public MainViewModel(EquationOfState equationOfState)
		{
			EoS = equationOfState;
			Update();
		}

		public MainViewModel() {
			EoS = new VanDerWaalsEOS(Chemical.Water);
			Update();
		}

		public void Update()
		{
			Model = new PlotModel { Title = Constants.ChemicalNames[EoS.Species] + " pressure-volume isotherms" };
			var critT = Constants.ChemicalData[EoS.Species].critT;

			Temperature[] temps = { critT-30, critT-20, critT-10, critT-5, critT, critT+5 };

			// Add the pressure-volume (true) isotherm for each temperature to the plot. Uses parallelization.
			Parallel.ForEach(temps, T => Model.Series.Add(new FunctionSeries(FunctionFactory.PVTrueIsotherm(EoS, T), 3e-5, 5e-4, 500, "T = " + (double)T + "K")));

			Model.Axes.Add(new LinearAxis {
				Position = AxisPosition.Bottom, Minimum = 3e-5, Maximum = 5e-4, Title = "Molar Volume [m³/mol]"
			});
			Model.Axes.Add(new LinearAxis {
				Position = AxisPosition.Left, Minimum = 1e6, Maximum = 1e8, Title = "Pressure [Pa]"
			});
		}
	}
}