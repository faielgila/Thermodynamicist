using Core.EquationsOfState;
using Core.VariableTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThermodynamicistUWP.Plotting
{

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>Contributed by Yoshi Askharoun.</remarks>
    public static class FunctionFactory
    {
        /// <summary>
        /// Creates a double-to-double function which represents an isotherm in pressure-volume space
        /// </summary>
        /// <param name="EoS">Equation of State, stores species and reference state</param>
        /// <param name="T">Temperature, in [K]</param>
        public static Func<double, double> PVIsotherm(EquationOfState EoS, Temperature T)
        {
            return VMol => EoS.Pressure(T, VMol);
        }
    }
}
