namespace ThermodynamicistUWP
{
	using OxyPlot;
	using OxyPlot.Axes;
	using OxyPlot.Series;
	using Core.EquationsOfState;
	using ThermodynamicistUWP.Plotting;
	using Core;
	using System.Threading.Tasks;

	public class GTViewModel
	{
		public PlotModel Model { get; private set; }

		public EquationOfState EoS { get; set; }

		public GTViewModel(EquationOfState equationOfState)
		{
			EoS = equationOfState;
			Update();
		}

		public void Update()
		{
			Model = new PlotModel { Title = Constants.ChemicalNames[EoS.Species] + " molar Gibbs energy-temperature plot" };

			// Add curves for each phase to the plot.
			Model.Series.Add(FunctionFactory.LS_GTCurve(EoS, 101325, "liquid"));
			Model.Series.Add(FunctionFactory.LS_GTCurve(EoS, 101325, "vapor"));

			Model.Axes.Add(new LinearAxis {
				Position = AxisPosition.Bottom, Minimum = 250, Maximum = EoS.speciesData.critT+50, Title = "Temperature [K]"
			});
			Model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Left, Minimum = -1e3, Maximum = 1e4, Title = "ref. molar Gibbs energy [J/mol]"
			});
		}
	}
}