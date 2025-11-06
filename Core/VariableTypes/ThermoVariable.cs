namespace Core.VariableTypes
{
	public abstract class ThermoVariable : IComparable<ThermoVariable>
	{
		public ThermoVariable(double value, ThermoVarRelations relation, string units)
		{
			Value = value;
			Relation = relation;
			Units = units;
		}

		public ThermoVariable()
		{
			Value = 0;
			Relation = ThermoVarRelations.Undefined;
			Units = "";
		}

		public double Value { get; set; }
		public ThermoVarRelations Relation { get; set; }
		public string Units { get; set; }

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

		public override bool Equals(object? obj)
		{
			if (obj is double d) return double.IsNaN(d) == double.IsNaN(Value) && d == Value;

			return obj is ThermoVariable variable &&
				(double.IsNaN(Value) == double.IsNaN(variable.Value) ||
				Value == variable.Value) &&
				Relation == variable.Relation &&
				Units == variable.Units;
		}
	}
}
