using System;
using System.Collections.Generic;
using System.Text;

namespace Core.VariableTypes
{
    public class ThermoVariable : IComparable<ThermoVariable>
    {
        public double Value { get; }
        public ThermoVarRelations Relation { get; }

        public ThermoVariable(double value, ThermoVarRelations relation)
        {
            Value = value;
            Relation = relation;
        }

		public int CompareTo(ThermoVariable other)
		{
			if (Value < other.Value) return -1;
            if (Value == other.Value) return 0;
            if (Value > other.Value) return 1;
            else return 0;
		}
	}
}
