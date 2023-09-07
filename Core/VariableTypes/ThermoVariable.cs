namespace Core.VariableTypes
{
	public class ThermoVariable : IComparable<ThermoVariable>
	{
		public double Value { get; set; }
		public ThermoVarRelations Relation { get; set; }

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

        public override string ToString()
        {
            return Value.ToString();
        }

        public string ToEngrNotation() { return Value.ToEngrNotation(); }
		public string ToEngrNotation(int sigfigs) { return Value.ToEngrNotation(sigfigs); }
	}
}
