using Core.VariableTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Multicomponent.ActivityModels;
class VanLaarActivityModel
{
	public static (double a, double b) VanLaarCoefficients(Chemical species1, Chemical species2, Temperature? T = null)
	{
		var binMix = (species1, species2);
		(double, double) coef;

		throw new NotImplementedException("van Laar activity coefficient model is not available yet.");
		
		//return coef;
	}

	// See Sandler, tbl 9.5-1
	private static Dictionary<(Chemical, Chemical), (double a, double b)> VanLaarActivityModelData = new()
	{
		//
	};
}
