using Core.VariableTypes;

namespace Core.EquationsOfState;

/// <summary>
/// Represents the modified solid-liquid-vapor (MSLV) eqauation of state and related functions as published by
/// Mo and Zhang et al. (2022). DOI 10.1021/acsomega.1c06142
/// Extends <see cref="EquationOfState"/>.
/// </summary>
public class ModSolidLiquidVaporEOS : EquationOfState
{
	public ModSolidLiquidVaporEOS(Chemical species) : base(species)
	{
		if (!ReducedCriticalEoSFittingParameters.ContainsKey(species)) throw new KeyNotFoundException("The provided species is not supported by this EoS.");

		// Define the EoS fitting parameters from their critical reduced counterparts.
		// a is defined as a method because it is temperature-dependent.
		b = ReducedCriticalEoSFittingParameters[species].b * CriticalMolarVolume();
		c = ReducedCriticalEoSFittingParameters[species].c * CriticalMolarVolume();
		d = ReducedCriticalEoSFittingParameters[species].d * CriticalMolarVolume();

		var acentricFactor = speciesData.acentricFactor;
		if (acentricFactor < 0.491) Kappa = 0.37464 + 1.54226 * acentricFactor - 0.26992 * acentricFactor * acentricFactor;
		else Kappa = 0.374642 + 1.48504 * acentricFactor - 0.165523 * Math.Pow(acentricFactor, 2) + 0.016666 * Math.Pow(acentricFactor, 3);
	}

	#region Parameters
	/// <summary>
	/// Stores critical molar volume data for all supported chemicals.
	/// Note that these cannot easily be calculated using the PhaseFinder because the critical molar volume
	/// is needed to caluclate the parameters for the equation of state.
	/// </summary>
	public static readonly Dictionary<Chemical, Volume> CriticalMolarVolumeData = new()
	{
		[Chemical.CarbonDioxide] = 0.094e-3,
		[Chemical.Ethane] = 0.1455e-3,
		[Chemical.HydrogenSulfide] = 0.0985e-3,
		[Chemical.Methane] = 0.0986e-3,
		[Chemical.Propane] = 0.2e-3,
	};

	/// <summary>
	/// Stores fitting parameters for the reduced form of the MSLV equation of state at the critical point.
	/// Reference Table 2, "EOS Constants for arc, brc, drc, and crc"
	/// </summary>
	public static readonly Dictionary<Chemical, (double a, double b, double c, double d)> ReducedCriticalEoSFittingParameters = new()
	{
		[Chemical.CarbonDioxide] = (0.4527902, 0.2790526, 0.3969935, 0.3965235),
		[Chemical.Ethane] = (0.4795142, 0.2970187, 0.3171983, 0.3171974),
		[Chemical.HydrogenSulfide] = (0.4801457, 0.2923996, 0.3520109, 0.3519409),
		[Chemical.Methane] = (0.4902265, 0.2989634, 0.3604034, 0.3603434),
		[Chemical.Propane] = (0.4741352, 0.2950876, 0.3007413, 0.3006134),
	};

	// Internal variables for easy access to the EoS fitting parameters.
	private double a(Temperature T)
	{
		var aRed = Alpha(T) * ReducedCriticalEoSFittingParameters[Species].a;
		return aRed * R * R * speciesData.critT * speciesData.critT / speciesData.critP;
	}
	private readonly double b;
	private readonly double c;
	private readonly double d;
	private readonly double Kappa;

	// The following four methods evaluate expressions that are common in equations derived from the Peng-Robinson EoS.
	private double A(Temperature T, Pressure P) { return a(T) * P / R / R / T / T; }
	private double B(Temperature T, Pressure P) { return b * P / R / T; }
	private double C(Temperature T, Pressure P) { return c * P / R / T; }
	private double D(Temperature T, Pressure P) { return d * P / R / T; }
	private double Alpha(Temperature T)
	{
		return Math.Pow(1 + Kappa * (1 - Math.Sqrt(T / speciesData.critT)), 2);
	}
	private double Da(Temperature T)
	{
		return -ReducedCriticalEoSFittingParameters[Species].a *
			Math.Pow(R * speciesData.critT, 2) / speciesData.critP * Kappa * Math.Sqrt(Alpha(T) / speciesData.critT / T);
	}
	#endregion

	public override Pressure Pressure(Temperature T, Volume VMol)
	{
		// Define reduced variables.
		var TRed = T / speciesData.critT;
		var VMolCrit = CriticalMolarVolume();
		var zCrit = CompressibilityFactor(speciesData.critT, speciesData.critP, VMolCrit);
		var VMolRed = VMol / VMolCrit;
		var aRed = a(T) * speciesData.critP / Math.Pow(R * speciesData.critT, 2);
		var bRed = b / VMolCrit;
		var cRed = c / VMolCrit;
		var dRed = d / VMolCrit;
		// Implement EoS.
		var PRed = TRed / (VMolRed - bRed) / zCrit * (VMolRed - dRed) / (VMolRed - cRed)
			- aRed * (VMolRed * VMolRed + 2 * bRed * VMolRed - bRed * bRed) / zCrit / zCrit;
		// Convert reduced pressure to absolute pressure.
		return PRed * speciesData.critP;
	}

	#region Pressure partial derivatives
	public override double PVPartialDerivative(Temperature T, Volume VMol)
	{
		var a = this.a(T);
		var term1 = 2 * a * (b + VMol) / (VMol * VMol + 2 * b * VMol - b * b);
		var term2 = -d * R * T * (b + c - 2 * VMol) + R * T * (b * c - VMol * VMol);
		return term1 + term2 / Math.Pow((b - VMol) * (c - VMol), 2);
	}
	#endregion

	public override Volume CriticalMolarVolume()
	{
		return CriticalMolarVolumeData[Species];
	}
	
	// Given in the research paper, see eqn 4.
	public override double FugacityCoeff(Temperature T, Pressure P, Volume VMol)
	{
		var sqrt2 = Math.Sqrt(2);
		var term1 = c * Math.Log(Math.Abs(1 - c / VMol));
		term1 -= b * Math.Log(Math.Abs(1 - b / VMol));
		term1 += d * Math.Log(Math.Abs((VMol - b) / (VMol - c)));
		term1 /= b - c;
		var term2 = Math.Log(Math.Abs((VMol + b * (1 + sqrt2)) / VMol + b * (1 - sqrt2)));
		term2 *= -a(T) / R / T / 2 / sqrt2 / b;
		var z = CompressibilityFactor(T, P, VMol);
		return term1 + term2 + z - Math.Log(z) - 1;
	}

	#region Z-equation and related methods
	public double ZEquation(Temperature T, Pressure P, Volume VMol)
	{
		var z = P * VMol / R / T;
		var A = this.A(T, P);
		var B = this.B(T, P);
		var C = this.C(T, P);
		var D = this.D(T, P);
		var coef0 = C * B * B * B + D * B * B - A * B * C;
		var coef1 = A * B - B * B - 2 * B * B * C + A * C - B * B * (B + C) - 2 * B * D;
		var coef2 = 2 * B - A - D + 2 * B * (B + C) + B * B - B * C;
		var coef3 = C - B + 1;
		return coef0 + z * coef1 + z * z * coef2 + z * z * z * coef3 - z * z * z * z;
	}

	public double ZFirstDerivative(Temperature T, Pressure P, Volume VMol)
	{
		var z = P * VMol / R / T;
		var A = this.A(T, P);
		var B = this.B(T, P);
		var C = this.C(T, P);
		var D = this.D(T, P);
		var coef1 = A * B - B * B - 2 * B * B * C + A * C - B * B * (B + C) - 2 * B * D;
		var coef2 = 2 * B - A - D + 2 * B * (B + C) + B * B - B * C;
		var coef3 = C - B + 1;
		return coef1 + 2 * z * coef2 + 3 * z * z * coef3 - 4 * z * z * z;
	}

	public double ZSecondDerivative(Temperature T, Pressure P, Volume VMol)
	{
		var z = P * VMol / R / T;
		var A = this.A(T, P);
		var B = this.B(T, P);
		var C = this.C(T, P);
		var D = this.D(T, P);
		var coef2 = 2 * B - A - D + 2 * B * (B + C) + B * B - B * C;
		var coef3 = C - B + 1;
		return 2 * coef2 + 3 * 2 * z * coef3 - 4 * 3 * z * z;
	}

	public double ZThirdDerivative(Temperature T, Pressure P, Volume VMol)
	{
		return 3 * 2 * (C(T, P) - B(T, P) + 1) - 4 * 3 * 2 * P * VMol / R / T;
	}

	public double ZFluidInflectionFinder(Temperature T, Pressure P)
	{
		// First, find the zero of the third derivative.
		// There is guaranteed to be exactly one inflection point on either side of that zero.
		double maxVMol = 1;
		double minVMol = c;
		double midVMol = (maxVMol + minVMol) / 2;
		while ((maxVMol - minVMol) > precisionLimit)
		{
			int maxVMolSign = Math.Sign(ZThirdDerivative(T, P, maxVMol));
			int midVMolSign = Math.Sign(ZThirdDerivative(T, P, midVMol));
			int minVMolSign = Math.Sign(ZThirdDerivative(T, P, minVMol));
			if (midVMolSign == maxVMolSign) maxVMol = midVMol;
			if (midVMolSign == minVMolSign) minVMol = midVMol;
			if (midVMolSign == 0) return midVMol;
			midVMol = (minVMol + maxVMol) / 2;
		}

		// Now, the inflection point of interest will be between the fitting parameter "c" and that zero.
		minVMol = c;
		maxVMol = midVMol;
		midVMol = (maxVMol + minVMol) / 2;
		while ((maxVMol - minVMol) > precisionLimit)
		{
			int maxVMolSign = Math.Sign(ZSecondDerivative(T, P, maxVMol));
			int midVMolSign = Math.Sign(ZSecondDerivative(T, P, midVMol));
			int minVMolSign = Math.Sign(ZSecondDerivative(T, P, minVMol));
			if (midVMolSign == maxVMolSign) maxVMol = midVMol;
			if (midVMolSign == minVMolSign) minVMol = midVMol;
			if (midVMolSign == 0) return midVMol;
			midVMol = (minVMol + maxVMol) / 2;
		}

		return midVMol;
	}

	public double ZTurnFinder(Temperature T, Pressure P, Volume minVMol, Volume maxVMol)
	{
		double midVMol = (maxVMol + minVMol) / 2;
		while ((maxVMol - minVMol) > precisionLimit)
		{
			int maxVMolSign = Math.Sign(ZFirstDerivative(T, P, maxVMol));
			int midVMolSign = Math.Sign(ZFirstDerivative(T, P, midVMol));
			int minVMolSign = Math.Sign(ZFirstDerivative(T, P, minVMol));
			if (midVMolSign == maxVMolSign) maxVMol = midVMol;
			if (midVMolSign == minVMolSign) minVMol = midVMol;
			if (midVMolSign == 0) return midVMol;
			midVMol = (minVMol + maxVMol) / 2;
		}
		return midVMol;
	}

	#endregion

	public override Dictionary<string, Volume> PhaseFinder(Temperature T, Pressure P, bool ignoreEquilibrium = false)
	{
		/* Because this MSLV is based on the Peng-Robinson equation of state (which is a cubic equation),
		 * a similar bisection-based algorithm should work.
		 * In order to come up with the ranges for the bisection algorithm, the isotherm needs to be split into 4 regions:
		 * one containing the solid regime, one containing the liquid regime, one containing the s-curve, and one containing
		 * the vapor regime. Luckily, because of how the fitting parameters of this equation are defined, the new solid regime
		 * is easily defined directly by the fitting parameters.
		 */

		/* The solid root, if it exists (and I'm fairly certain it always will because of the asymptote), will be within
		 * z=B and z=D (or VMol=b and VMol=d)
		 */
		var VMol_S = ZRootFinder(T, P, b, d);

		/* The fluid roots are a little more complex than that, but because this EoS guarntees that the two fluid roots exist above
		 * the fitting parameter c, the same rootfinding method used for the cubic equation works here as well. The only major
		 * difference is that the inflection point cannot easily be calculated analytically, so instead bisection will be used to
		 * find the inflection point between the liquid and vapor phases.
		 */
		Volume fluidInflectionPoint = ZFluidInflectionFinder(T, P);
		Volume fluidTurningPoint1 = ZTurnFinder(T, P, c, fluidInflectionPoint);
		Volume fluidTurningPoint2 = ZTurnFinder(T, P, fluidInflectionPoint, 1);
		Volume VMol_L = ZRootFinder(T, P, c, fluidTurningPoint1);
		Volume VMol_V = ZRootFinder(T, P, fluidTurningPoint2, 1);

		// Initialize an empty dictionary.
		var list = new Dictionary<string, Volume>();

		// If "ignoreEquilibrium" is set to true, we do not need to copmare fugacities to determine equilibrium phases.
		if (ignoreEquilibrium) { list.Add("solid", VMol_S); list.Add("liquid", VMol_L); list.Add("vapor", VMol_V); return list; }

		/* Now that the predicted phases have been found, estimate the fugacities (or more accurately, the fugacity coefficients)
		 * to determine whether that phase corresponds to a real state in the equilibrium. 
		 */
		return EquilibriumPhases(T, P);
	}

	public Volume ZRootFinder(Temperature T, Pressure P, Volume minVMol, Volume maxVMol)
	{
		double midVMol = (maxVMol + minVMol) / 2;
		while ((maxVMol - minVMol) > precisionLimit)
		{
			int maxVMolSign = Math.Sign(ZEquation(T, P, maxVMol));
			int midVMolSign = Math.Sign(ZEquation(T, P, midVMol));
			int minVMolSign = Math.Sign(ZEquation(T, P, minVMol));
			if (midVMolSign == maxVMolSign) maxVMol = midVMol;
			if (midVMolSign == minVMolSign) minVMol = midVMol;
			if (midVMolSign == 0) return midVMol;
			midVMol = (minVMol + maxVMol) / 2;
		}
		return midVMol;
	}

	public Pressure SublimationPressure(Temperature T)
	{
		throw new NotImplementedException();
	}

	#region Departure functions
	
	public override Enthalpy DepartureEnthalpy(Temperature T, Pressure P, Volume VMol)
	{
		//throw new NotImplementedException();
		return 1;
	}

	public override Entropy DepartureEntropy(Temperature T, Pressure P, Volume VMol)
	{
		//throw new NotImplementedException();
		return 1;
	}

	#endregion
}
