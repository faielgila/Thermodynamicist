using Core.VariableTypes;

namespace Core.EquationsOfState;

public class PengRobinsonEOS : CubicEquationOfState
{
	private double b;
	private double Kappa;
	
	public PengRobinsonEOS(Chemical species) : base(species)
	{
		speciesData = Constants.ChemicalData[Species];
		var acentricFactor = speciesData.acentricFactor;
		Kappa = 0.37464 + (1.54226 - 0.26992 * acentricFactor) * acentricFactor;
		b = 0.07780 * R * speciesData.critT / speciesData.critP; 
	}

	private double a(Temperature T)
	{
		var critT = speciesData.critT;
		var critP = speciesData.critP;
		var aleph = 0.45724 * Math.Pow(R * critT, 2) / critP;
		var alpha = Math.Pow(1 + Kappa * (1 - Math.Sqrt(T / critT)), 2);
		return aleph * alpha;
	}
	
	private double A(Temperature T, Pressure P, MolarVolume VMol) { return a(T) * P / R / R / T / T; }
	private double B(Temperature T, Pressure P, MolarVolume VMol) { return b * P / R / T; }

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

	public override Pressure Pressure(Temperature T, MolarVolume VMol)
	{
		return R * T / (VMol - b) - a(T) / (VMol * VMol + 2 * b * VMol - b * b);
	}

	// from Sandler, eqn 7.4-14
	public override double FugacityCoeff(Temperature T, Pressure P, MolarVolume VMol)
	{
		var sqrt2 = Math.Sqrt(2);
		var z = CompressibilityFactor(T, P, VMol);
		var A = this.A(T, P, VMol);
		var B = this.B(T, P, VMol);
		var termLog = (z + B + sqrt2 * B) / (z + B - sqrt2 * B);
		var LogFugacityCoeff = z - 1 - Math.Log(z - B) - A / (2 * sqrt2 * B) * Math.Log(termLog);
		return Math.Exp(LogFugacityCoeff);
	}

	public override double ZCubicEqn(Temperature T, Pressure P, MolarVolume VMol)
	{
		var z = CompressibilityFactor(T, P, VMol);
		var A = this.A(T, P, VMol);
		var B = this.B(T, P, VMol);
		var term3 = z * z * z;
		var term2 = (-1 + B) * z * z;
		var term1 = (A - 3 * B * B - 2 * B) * z;
		var term0 = -A * B + B * B + B * B * B;
		return term3 + term2 + term1 + term0;
	}

	public override double ZCubicDerivative(Temperature T, Pressure P, MolarVolume VMol)
	{
		var z = CompressibilityFactor(T, P, VMol);
		var A = this.A(T, P, VMol);
		var B = this.B(T, P, VMol);
		return 3 * z * z + 2 * (-1 + B) * z + (A - 3 * B * B - 2 * B);
	}

	public override double ZCubicInflectionPoint(Temperature T, Pressure P)
	{
		return R * T / (3 * P) - b;
	}
}