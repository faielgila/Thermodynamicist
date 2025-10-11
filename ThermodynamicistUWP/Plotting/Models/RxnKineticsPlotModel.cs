using Core;
using Core.EquationsOfState;
using Core.Reactions;
using Core.Reactions.Kinetics;
using Core.VariableTypes;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Documents;

namespace ThermodynamicistUWP
{
	public class RxnKineticsPlotModel
	{
		public PlotModel Model { get; private set; }

		private Reaction rxn;

		private Temperature T;

		private Pressure P;

		private double dt;

		private double maxtime;

		public RxnKineticsPlotModel(Reaction _rxn, Temperature _T, Pressure _P, double _dt, double _maxtime)
		{
			rxn = _rxn;
			T = _T;
			P = _P;
			dt = _dt;
			maxtime = _maxtime;
			Update();
		}

		public void Update()
		{
			Model = new PlotModel { Title = " Chemical kinetics transience plot" };
			
			var speciesList = rxn.ConvertSpeciesToChemicalList();
			var cVec = new MolarityVector();
			var seriesList = new Dictionary<Chemical, LineSeries>();
			var rng = new Random();
			foreach (var chemical in speciesList)
			{
				cVec.Add(chemical, 1);
				var series = new LineSeries()
				{
					LineStyle = (LineStyle)rng.Next(1, 6),
					//Color = OxyColors.Black,
					StrokeThickness = 4,
					LineJoin = LineJoin.Round,
					Title = Constants.ChemicalNames[chemical]
				};
				series.Points.Add(new DataPoint(0, 1));
				seriesList.Add(chemical, series);
			}

			for (int i = 1; i * dt < maxtime; i++)
			{
				var changes = rxn.rateLaw.SpeciesReactionRates(T, P, cVec);

				foreach (var chemical in speciesList)
				{
					if (!changes.ContainsKey(chemical))
					{
						seriesList[chemical].Points.Add(new DataPoint(dt * i, cVec[chemical]));
						continue;
					}
					cVec[chemical] += changes[chemical] * dt;
					seriesList[chemical].Points.Add(new DataPoint(dt * i, cVec[chemical]));
				}
			}

			// Add curves for each chemical to the plot.
			foreach (var item in seriesList)
			{
				Model.Series.Add(item.Value);
			}

			// Add axes.
			Model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Bottom,
				Minimum = 0,
				Maximum = maxtime,
				Title = "reaction time [s]"
			});
			Model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Left,
				Minimum = 0,
				Maximum = 2,
				Title = "molar concentration [mol/L]"
			});
		}
	}
}