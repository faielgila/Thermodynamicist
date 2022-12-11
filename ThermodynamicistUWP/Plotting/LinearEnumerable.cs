using System;
using System.Collections;
using System.Collections.Generic;

namespace ThermodynamicistUWP.Plotting
{
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>Contributed by Yoshi Askharoun.</remarks>
	public class LinearEnumerable : IEnumerable<double>
    {
        public double Min { get; }
        public double Max { get; }
        public double Increment { get; }

        public LinearEnumerable(double min, double max, double increment = 1)
        {
            Min = min;
            Max = max;
            Increment = increment;
        }

        public IEnumerator<double> GetEnumerator() => new LinearEnumerator(Min, Max, Increment);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class LinearEnumerator : IEnumerator<double>
        {
            public LinearEnumerator(double min, double max, double increment)
            {
                if (min >= max)
                    throw new ArgumentException($"{nameof(min)} cannot be greater than {nameof(max)}.");

                Min = min;
                Max = max;
                Increment = increment;
            }

            public double Min { get; }
            public double Max { get; }
            public double Increment { get; }

            public double Current { get; private set; } = double.NaN;

            object IEnumerator.Current => this.Current;

            public bool MoveNext()
            {
                if (double.IsNaN(Current))
                {
                    Current = Min;
                    return true;
                }

                var next = Current + Increment;
                if (next > Max)
                    return false;

                Current = next;
                return true;
            }

            public void Reset()
            {
                Current = double.NaN;
            }

            public void Dispose()
            {
            }
        }
    }
}
