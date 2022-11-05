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
			EoS = new VanDerWaalsEOS(Core.Chemical.Water);
			Update();
		}

		public void Update()
		{
			Model = new PlotModel { Title = Constants.ChemicalNames[EoS.Species] + " pressure-volume isotherms" };

			Temperature[] temps = { 375, 382, 391, 400, 405.6, 408 };

			foreach (Temperature T in temps)
			{
				var name = "T = " + (double)T + "K";
				Model.Series.Add(new FunctionSeries(FunctionFactory.PVTrueIsotherm(EoS, T), 3e-5, 5e-4, 500, name));
			}

			Model.Axes.Add(new LogarithmicAxis {
				Position = AxisPosition.Bottom, Minimum = 3e-5, Maximum = 5e-4, Title = "Molar Volume [m³/mol]"
			});
			Model.Axes.Add(new LinearAxis {
				Position = AxisPosition.Left, Minimum = 1e6, Maximum = 1e8, Title = "Pressure [Pa]"
			});
		}
	}
}