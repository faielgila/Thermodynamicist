namespace ThermodynamicistUWP
{
	using OxyPlot;
	using OxyPlot.Axes;
	using OxyPlot.Series;
	using Core.EquationsOfState;
	using ThermodynamicistUWP.Plotting;
	using Core;
	using Core.VariableTypes;

	public class GenericPlotModel
	{
		public PlotModel Model { get; private set; }

		public LineSeries LS;

		public string PlotTitle;
		public string XAxisLabel;
		public string YAxisLabel;
		public double[] XAxisLimits;
		public double[] YAxisLimits;

		public GenericPlotModel(
			LineSeries plotData, string plotTitle, string xAxisLabel, string yAxisLabel,
			double[] xAxisLimits, double[] yAxisLimits)
		{
			LS = plotData;
			PlotTitle = plotTitle;
			XAxisLabel = xAxisLabel;
			YAxisLabel = yAxisLabel;
			XAxisLimits = xAxisLimits;
			YAxisLimits = yAxisLimits;

			Update();
		}

		public void Update()
		{
			Model = new PlotModel { Title = PlotTitle };

			Model.Series.Add(LS);

			Model.Axes.Add(new LinearAxis {
				Position = AxisPosition.Bottom, Minimum = XAxisLimits[0], Maximum = XAxisLimits[1], Title = XAxisLabel
			});
			Model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Left, Minimum = YAxisLimits[0], Maximum = YAxisLimits[1], Title = YAxisLabel
			});
		}
	}
}