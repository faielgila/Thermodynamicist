using Core.VariableTypes;

namespace Core.EquationsOfState;

public class PengRobinsonEOS : CubicEquationOfState
{
	private double b;
	private double Kappa;
	
	public PengRobinsonEOS(Chemical species) : base(species)
	{
		var acentricFactor = speciesData.acentricFactor;
		Kappa = 0.37464 + (1.54226 - 0.26992 * acentricFactor) * acentricFactor;
		b = 0.07780 * R * speciesData.critT / speciesData.critP; 
	}

	#region Parameters

	private double a(Temperature T)
	{
		var critT = speciesData.critT;
		var critP = speciesData.critP;
		var aleph = 0.45724 * Math.Pow(R * critT, 2) / critP;
		var alpha = Math.Pow(1 + Kappa * (1 - Math.Sqrt(T / critT)), 2);
		return aleph * alpha;
	}
	
	private double A(Temperature T, Pressure P) { return a(T) * P / R / R / T / T; }
	private double B(Temperature T, Pressure P) { return b * P / R / T; }

	private double Alpha(Temperature T)
	{
		return Math.Pow(1 + Kappa * (1 - Math.Sqrt(T / speciesData.critT)), 2);
	}

	private double Da(Temperature T)
	{
		var critTemp = speciesData.critT;
		var critPres = speciesData.critP;
		return -0.45724 * Math.Pow(R * critTemp, 2) / critPres * Kappa * Math.Sqrt(Alpha(T) / critTemp / T);
	}
	
	#endregion

	public override Pressure Pressure(Temperature T, MolarVolume VMol)
	{
		return R * T / (VMol - b) - a(T) / (VMol * VMol + 2 * b * VMol - b * b);
	}

	// from Sandler, eqn 7.4-14
	public override double FugacityCoeff(Temperature T, Pressure P, MolarVolume VMol)
	{
		var sqrt2 = Math.Sqrt(2);
		var z = CompressibilityFactor(T, P, VMol);
		var A = this.A(T, P);
		var B = this.B(T, P);
		var termLog = (z + B + sqrt2 * B) / (z + B - sqrt2 * B);
		var LogFugacityCoeff = z - 1 - Math.Log(z - B) - A / (2 * sqrt2 * B) * Math.Log(termLog);
		return Math.Exp(LogFugacityCoeff);
	}

	#region Cubic and related equations
	public override double ZCubicEqn(Temperature T, Pressure P, MolarVolume VMol)
	{
		var z = CompressibilityFactor(T, P, VMol);
		var A = this.A(T, P);
		var B = this.B(T, P);
		var term3 = z * z * z;
		var term2 = (-1 + B) * z * z;
		var term1 = (A - 3 * B * B - 2 * B) * z;
		var term0 = -A * B + B * B + B * B * B;
		return term3 + term2 + term1 + term0;
	}

	public override double ZCubicDerivative(Temperature T, Pressure P, MolarVolume VMol)
	{
		var z = CompressibilityFactor(T, P, VMol);
		var A = this.A(T, P);
		var B = this.B(T, P);
		return 3 * z * z + 2 * (-1 + B) * z + (A - 3 * B * B - 2 * B);
	}

	public override double ZCubicInflectionPoint(Temperature T, Pressure P)
	{
		return R * T / (3 * P) - b;
	}
	
	#endregion
	
	#region State functions - Enthalpy

	// from Sandler, eqn 6.4-29
	public MolarEnthalpy DepartureEnthalpy(Temperature T, Pressure P, MolarVolume VMol)
	{
		var sqrt2 = Math.Sqrt(2);
		var da = this.Da(T);
		var a = this.a(T);
		var z = CompressibilityFactor(T, P, VMol);
		var B = this.B(T, P);
		var logPiece = (z + B + sqrt2 * B) / (z + B - sqrt2 * B);
		var value = R * T * (z - 1) + (T * da - a) / (2 * sqrt2 * b) * Math.Log(logPiece);
		return new MolarEnthalpy(value, ThermoVarRelations.Departure);
	}

	public MolarEnthalpy IdealMolarEnthalpyChange(Temperature T1, Temperature T2)
	{
		double[] c;
		if (!UseHighTempData)
		{
			// TODO: show warning if T1 or T2 is outside the TLimits specified in the data
			var lowerTLimit = speciesCpData.lims[0];
			var upperTLimit = speciesCpData.lims[1];
			c = speciesCpData.vals;
		}
		else
		{
			throw new NotImplementedException();
		}

		var deltaT2 = T2 * T2 - T1 * T1;
		var deltaT3 = T2 * T2*T2 - T1 * T1*T1;
		var deltaT4 = Math.Pow(T2,4) - Math.Pow(T1,4);
		var value = c[0] * (T2 - T1) + c[1]/2 * deltaT2 + c[2]/3 * deltaT3 + c[3]/4 * deltaT4;
		return new MolarEnthalpy(value, ThermoVarRelations.Change);
	}

	public MolarEnthalpy MolarEnthalpyChange
		(Temperature T1, Pressure P1, MolarVolume VMol1, Temperature T2, Pressure P2, MolarVolume VMol2)
	{
		var pathA = DepartureEnthalpy(T1, P1, VMol1);
		var pathB = IdealMolarEnthalpyChange(T1, T2);
		var pathC = DepartureEnthalpy(T2, P2, VMol2);
		var totalPath = -pathA + pathB + pathC;
		return new MolarEnthalpy(totalPath, ThermoVarRelations.Change);
	}
	
	public MolarEnthalpy ReferenceMolarEnthalpy(Temperature T, Pressure P, MolarVolume VMol)
	{
		var pathB = IdealMolarEnthalpyChange(273.15+25, T);
		var pathC = DepartureEnthalpy(T, P, VMol);
		var totalPath = pathB + pathC;
		return new MolarEnthalpy(totalPath, ThermoVarRelations.Change);
	}

	#endregion
	
	#region State functions - Entropy

	// from Sandler, eqn 6.4-30
	public MolarEntropy DepartureEntropy(Temperature T, Pressure P, MolarVolume VMol)
	{
		var sqrt2 = Math.Sqrt(2);
		var da = this.Da(T);
		var a = this.a(T);
		var z = CompressibilityFactor(T, P, VMol);
		var B = this.B(T, P);
		var logPiece = (z + B + sqrt2 * B) / (z + B - sqrt2 * B);
		var value = R * Math.Log(z - B) + da / (2 * sqrt2 * b) * Math.Log(logPiece);
		return new MolarEntropy(value, ThermoVarRelations.Departure);
	}

	public MolarEntropy IdealMolarEntropyChange(Temperature T1, Pressure P1, Temperature T2, Pressure P2)
	{
		double[] c;
		if (!UseHighTempData)
		{
			// TODO: show warning if T1 or T2 is outside the TLimits specified in the data
			var lowerTLimit = speciesCpData.lims[0];
			var upperTLimit = speciesCpData.lims[1];
			c = speciesCpData.vals;
		}
		else
		{
			throw new NotImplementedException();
		}
		
		var deltaT2 = T2 * T2 - T1 * T1;
		var deltaT3 = T2 * T2*T2 - T1 * T1*T1;
		var value = c[0] * Math.Log(T2 / T1) + c[1] * (T2 - T1) + c[2] / 2 * deltaT2 + c[3] / 3 * deltaT3;
		value -= R * Math.Log(P2 / P1);
		return new MolarEntropy(value, ThermoVarRelations.Change);
	}

	public MolarEntropy MolarEntropyChange
		(Temperature T1, Pressure P1, MolarVolume VMol1, Temperature T2, Pressure P2, MolarVolume VMol2)
	{
		var pathA = DepartureEntropy(T1, P1, VMol1);
		var pathB = IdealMolarEntropyChange(T1, P1, T2, P2);
		var pathC = DepartureEntropy(T2, P2, VMol2);
		var totalPath = -pathA + pathB + pathC;
		return new MolarEntropy(totalPath, ThermoVarRelations.Change);
	}
	
	public MolarEntropy ReferenceMolarEntropy(Temperature T, Pressure P, MolarVolume VMol)
	{
		var pathB = IdealMolarEntropyChange(273.15+25, 100e3, T, P);
		var pathC = DepartureEntropy(T, P, VMol);
		var totalPath = pathB + pathC;
		return new MolarEntropy(totalPath, ThermoVarRelations.Change);
	}

	#endregion
}