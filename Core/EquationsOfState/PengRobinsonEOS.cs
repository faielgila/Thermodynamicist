using Core.VariableTypes;

namespace Core.EquationsOfState;

public class PengRobinsonEOS : CubicEquationOfState
{
	private double b;
	private double Kappa;
	
	private double a(Temperature T) { return Math.Pow(1 + Kappa * (1 - Math.Sqrt(T / speciesData.critT)), 2); }

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

	public PengRobinsonEOS(Chemical species) : base(species)
	{
		speciesData = Constants.ChemicalData[Species];
		var acentricFactor = speciesData.acentricFactor;
		Kappa = 0.3764 + 1.54226 * acentricFactor - 0.26992 * acentricFactor * acentricFactor;
		b = 0.07780 * R * speciesData.critT / speciesData.critP; 
	}

	public override Pressure Pressure(Temperature T, MolarVolume VMol)
	{
		return R * T / (VMol - b) - a(T) / (VMol * VMol + 2 * b * VMol - b * b);
	}

	public override double FugacityCoeff(Temperature T, Pressure P, MolarVolume VMol)
	{
		throw new NotImplementedException();
	}

	public override double ZCubicEqn(Temperature T, Pressure P, MolarVolume VMol)
	{
		var z = P * VMol / R / T;
		var A = a(T) * P / (R * R * T * T);
		var B = b * P / R / T;
		var term3 = z * z * z;
		var term2 = (-1 + B) * z * z;
		var term1 = (A - 3 * B * B - 2 * B) * z;
		var term0 = -A * B + B * B + B * B * B;
		return term3 + term2 + term1 + term0;
	}

	public override double ZCubicDerivative(Temperature T, Pressure P, MolarVolume VMol)
	{
		throw new NotImplementedException();
	}

	public override double ZCubicInflectionPoint(Temperature T, Pressure P)
	{
		throw new NotImplementedException();
	}
}