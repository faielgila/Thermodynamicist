namespace Core.VariableTypes
{
	public abstract class ThermoVariable(double value, ThermoVarRelations relation, string units) : IComparable<ThermoVariable>
	{
		public double Value { get; set; } = value;
		public ThermoVarRelations Relation { get; set; } = relation;
		public string Units { get; set; } = units;

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

		public static implicit operator double(ThermoVariable T) => T.Value;

		public string ToEngrNotation() { return Value.ToEngrNotation() + " " + Units; }
		public string ToEngrNotation(int sigfigs) { return Value.ToEngrNotation(sigfigs) + " " + Units; }

		public double RoundToSigfigs(int sigfigs) { return Value.RoundToSigfigs(sigfigs); }
	}
}
