namespace ThermodynamicistUWP
{
    using System;

    using OxyPlot;
    using OxyPlot.Series;

    public class MainViewModel
    {
        public MainViewModel()
        {
            this.MyModel = new PlotModel { Title = "Example 1" };
            this.MyModel.Series.Add(new FunctionSeries(Math.Exp, 0, 10, 0.1, "exp(x)"));
        }

        public PlotModel MyModel { get; private set; }
    }
}