namespace Core.VariableTypes
{
	public abstract class ThermoVariable : IComparable<ThermoVariable>
	{
		public double Value { get; set; }
		public ThermoVarRelations Relation { get; set; }
		public string Units { get; set; }

		public ThermoVariable(double value, ThermoVarRelations relation, string units)
		{
			Value = value;
			Relation = relation;
			Units = units;
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

		public string ToEngrNotation() { return Value.ToEngrNotation() + " " + Units; }
		public string ToEngrNotation(int sigfigs) { return Value.ToEngrNotation(sigfigs) + " " + Units; }

		public double RoundToSigfigs(int sigfigs) { return Value.RoundToSigfigs(sigfigs); }
	}
}
