using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Core.EquationsOfState;
using ThermodynamicistUWP.Plotting;
using Core;
using System.Threading.Tasks;
using ThermodynamicistUWP.Plotting.Factories;

namespace ThermodynamicistUWP
{
	public class PTPlotModel
	{
		public PlotModel Model { get; private set; }

		public EquationOfState EoS { get; set; }

		public PTPlotModel(EquationOfState equationOfState)
		{
			EoS = equationOfState;
			Update();
		}

		public void Update()
		{
			Model = new PlotModel { Title = Constants.ChemicalNames[EoS.Species] + " pressure-temperature phase diagram" };

			if (EoS.GetType().Name != "ModSolidLiquidVaporEOS")
			{
				// Add the vapor-liquid transition curve to the plot.
				Model.Series.Add(PTPlotFunctionFactory.LS_EvaporationCurve(EoS));
			}

			if (EoS.GetType().Name == "ModSolidLiquidVaporEOS")
			{
				// Add the solid-liquid transition curve to the plot.
				//Model.Series.Add(FunctionFactory.LS_PTMeltingCurve(EoS));

				// Add the solid-gas transition curve to the plot.
				//Model.Series.Add(FunctionFactory.LS_PTSublimationCurve(EoS));
			}

			Model.Axes.Add(new LinearAxis {
				Position = AxisPosition.Bottom, Minimum = 250, Maximum = EoS.speciesData.critT+50, Title = "Temperature [K]"
			});
			Model.Axes.Add(Common.PressureLogarithmicAxis());
		}
	}
}