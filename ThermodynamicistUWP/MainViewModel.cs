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

			Temperature[] temps = { 275, 280, 285, 290, 295, 300, 305 };

			foreach (Temperature T in temps)
			{
				var name = "T = " + (double)T + "K";
				Model.Series.Add(new FunctionSeries(FunctionFactory.PVIsotherm(EoS, T), 3e-5, 0.0004, 1000, name));
			}

			Model.Axes.Add(new LinearAxis {
				Position = AxisPosition.Bottom, Minimum = 0.00005, Maximum = 0.0004, Title = "Molar Volume [m³/mol]"
			});
			Model.Axes.Add(new LinearAxis {
				Position = AxisPosition.Left, Minimum = 3e6, Maximum = 8.5e6, Title = "Pressure [Pa]"
			});
		}
	}
}