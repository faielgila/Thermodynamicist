using OxyPlot.Axes;

namespace ThermodynamicistUWP.Plotting
{
	internal class Common
	{
		public static LogarithmicAxis PressureLogarithmicAxis(AxisPosition position = AxisPosition.Left)
		{
			return new LogarithmicAxis
			{
				Position = position,
				Minimum = 1e7,
				Maximum = 2.5e7,
				Title = "Pressure [Pa]"
			};
		}
	}
}
