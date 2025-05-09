using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Data
{
    class ArrheniusEqnParameters
    {
        // Westbrook & Dryer, Chemical kinetic modeling of hydrocarbon combustion (ISSN 0360-1285)
        // https://doi.org/10.1016/0360-1285(84)90118-7
        // Table 4 (p. 27)
        public static readonly Dictionary<Chemical, (double A, double E)> ArrEqnParam_Combustion = new ()
        {
            //
        };
    }
}
