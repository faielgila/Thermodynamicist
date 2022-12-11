using Core.EquationsOfState;
using Core.VariableTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThermodynamicistUWP.Plotting
{

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

		public static Func<double, double> PVTrueIsotherm(EquationOfState EoS, Temperature T)
		{
            var critT = EoS.speciesData.critT;
			if (T < critT)
			{
				var Pvap = EoS.VaporPressure(T);
				var (VMol_L, VMol_V) = EoS.PhaseFinder(T, Pvap);
				return VMol =>
				{
					if (VMol > VMol_L && VMol < VMol_V) { return Pvap; }
					else return EoS.Pressure(T, VMol);
				};
			}
			else
			{
				return VMol => EoS.Pressure(T, VMol);
			}
		}
	}
}
