﻿namespace ThermodynamicistUWP
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

	public class MainViewModel
	{
		public PlotModel Model { get; private set; }

		public EquationOfState EoS { get; set; }

		private bool ShowSCurves { get; set; }

		public MainViewModel(EquationOfState equationOfState, bool showSCurves = false)
		{
			EoS = equationOfState;
			ShowSCurves = showSCurves;
			Update();
		}

		public MainViewModel() {
			EoS = new VanDerWaalsEOS(Chemical.Water);
			Update();
		}

		public void Update()
		{
			Model = new PlotModel { Title = Constants.ChemicalNames[EoS.Species] + " pressure-volume isotherms" };
			var critT = EoS.speciesData.critT;

			Temperature[] temps = {
				critT-50, critT-20, critT-10, critT-5,
				critT, 273.15 };

			// Add the pressure-volume (true) isotherm for each temperature to the plot. Uses parallelization.
			Parallel.ForEach(temps, T => Model.Series.Add(FunctionFactory.FS_PVIsotherm(EoS, T)));
            // Add the pressure-volume s-curve isotherm for each temperature to the plot. Uses parallelization.
			if (ShowSCurves) Parallel.ForEach(temps, T => Model.Series.Add(FunctionFactory.FS_PVIsotherm(EoS, T, false)));

			Model.Axes.Add(new LinearAxis {
				Position = AxisPosition.Bottom, Minimum = 3e-5, Maximum = .001, Title = "Molar Volume [m³/mol]"
			});
			Model.Axes.Add(new LinearAxis {
				Position = AxisPosition.Left, Minimum = 1e5, Maximum = 4e7, Title = "Pressure [Pa]"
			});
		}
	}
}