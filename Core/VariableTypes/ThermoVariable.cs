using System;
using System.Collections.Generic;
using System.Text;

namespace Core.VariableTypes
{
    public class ThermoVariable
    {
        public double Value { get; }
        public ThermoVarRelations Relation { get; }

        public ThermoVariable(double value, ThermoVarRelations relation)
        {
            Value = value;
            Relation = relation;
        }
    }
}
