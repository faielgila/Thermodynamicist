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
	using System.Linq;
	using Windows.UI.Xaml;

	public class PTViewModel
	{
		public PlotModel Model { get; private set; }

		public EquationOfState EoS { get; set; }

		public PTViewModel(EquationOfState equationOfState)
		{
			EoS = equationOfState;
			Update();
		}

		public void Update()
		{
			Model = new PlotModel { Title = Constants.ChemicalNames[EoS.Species] + " pressure-temperature phase diagram" };

			// Add the vapor-liquid transition curve to the plot.
			Model.Series.Add(FunctionFactory.LS_PTEvaporationCurve(EoS));

			Model.Axes.Add(new LinearAxis {
				Position = AxisPosition.Bottom, Minimum = 250, Maximum = EoS.speciesData.critT+50, Title = "Temperature [K]"
			});
			Model.Axes.Add(Common.PressureLogarithmicAxis());
		}
	}
}