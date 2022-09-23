using Core.VariableTypes;
using Core.EquationsOfState;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Text;
using ThermodynamicistUWP.Plotting;

namespace Core.Plotting
{
    public class DataHandler
    {
        public static ScatterSeries PVIsothemSeries(Temperature T)
        {
            var EoS = new PengRobinsonEOS(Chemical.Oxygen);
            var lineSeries = new ScatterSeries();

            var volumes = new LinearEnumerable(1e-5, 50e-3, 1e-5);
            
            foreach (var volume in volumes)
            {
                lineSeries.Points.Add(new ScatterPoint(volume, EoS.Pressure(T, volume)));
            }

            return lineSeries;
        }
    }
}
