using Core.VariableTypes;
using System.Linq;

namespace Core.EquationsOfState;

/// <summary>
/// Represents the modified solid-liquid-vapor (MSLV) eqauation of state and related functions as published by
/// Mo and Zhang et al. (2022). DOI 10.1021/acsomega.1c06142
/// Extends <see cref="EquationOfState"/>.
/// </summary>
public class ModSolidLiquidVaporEOS : EquationOfState
{
	public ModSolidLiquidVaporEOS(Chemical species) : base(species, new List<string> { "solid", "liquid", "vapor" })
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

	private double A(Temperature T, Pressure P) { return a(T) * P / R / R / T / T; }
	private double B(Temperature T, Pressure P) { return b * P / R / T; }
	private double C(Temperature T, Pressure P) { return c * P / R / T; }
	private double D(Temperature T, Pressure P) { return d * P / R / T; }
	private double Alpha(Temperature T)
	{
		return Math.Pow(1 + Kappa * (1 - Math.Sqrt(T / speciesData.critT)), 2);
	}
	private double aRedTDerivative(Temperature T)
	{
		var aRedCrit = ReducedCriticalEoSFittingParameters[Species].a;
		return -aRedCrit * Kappa / Math.Sqrt(T / speciesData.critT) * (Kappa + 1) + aRedCrit * Kappa * Kappa;
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
		return Math.Exp(term1 + term2 + z - Math.Log(z) - 1);
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

	public double ZJoltPoint(Temperature T, Pressure P)
	{
		return R * T / P * (C(T, P) - B(T, P) + 1) / 4;
	}

	public double ZInflectionFinder(Temperature T, Pressure P, Volume minVMol, Volume maxVMol)
	{
		// Now, the inflection point of interest will be between the fitting parameter "c" and the jolt point.
		double midVMol = (maxVMol + minVMol) / 2;
		while ((maxVMol - minVMol) > precisionLimit)
		{
			int maxVMolSign = Math.Sign(ZSecondDerivative(T, P, maxVMol));
			int midVMolSign = Math.Sign(ZSecondDerivative(T, P, midVMol));
			int minVMolSign = Math.Sign(ZSecondDerivative(T, P, minVMol));
//			if (minVMolSign == maxVMolSign) throw new ArithmeticException("Bisection root-finding algorithm failed: boundary signs are equal.");
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

	#endregion

	#region Departure functions

	public override Enthalpy DepartureEnthalpy(Temperature T, Pressure P, Volume VMol)
	{
		var PCrit = speciesData.critP;
		var TCrit = speciesData.critT;
		var VMolCrit = CriticalMolarVolume();
		double zCrit = PCrit * VMolCrit / TCrit / R;

		double TRed = T / TCrit;
		double VMolRed = VMol / VMolCrit;
		// See eqns 27 and 28 in the cited paper
		double aRed = PCrit * a(T) / R / R / TCrit / TCrit;
		double bRed = ReducedCriticalEoSFittingParameters[Species].b;
		double cRed = ReducedCriticalEoSFittingParameters[Species].c;
		double dRed = ReducedCriticalEoSFittingParameters[Species].d;

		// See eqn 6 in the cited paper (w/ some simplifications)
		var val = aRedTDerivative(T) - aRed / TRed;
		var sqrt2 = Math.Sqrt(2);
		val *= Math.Log(Math.Abs((VMolRed + bRed * (1 + sqrt2)) / (VMolRed + bRed * (1 - sqrt2))));
		val /= 2 * sqrt2 * bRed * zCrit;
		val += (cRed - dRed) / (VMolRed - cRed) + (bRed - dRed) / (VMolRed - bRed);
		val += CompressibilityFactor(T, P, VMol) - 1;

		return val * R * T;
	}

	public override Entropy DepartureEntropy(Temperature T, Pressure P, Volume VMol)
	{
		var PCrit = speciesData.critP;
		var TCrit = speciesData.critT;
		var VMolCrit = CriticalMolarVolume();
		double zCrit = PCrit * VMolCrit / TCrit / R;

		double TRed = T / TCrit;
		double VMolRed = VMol / VMolCrit;
		// See eqns 27 and 28 in the cited paper
		double aRed = PCrit * a(T) / R / R / TCrit / TCrit;
		double bRed = ReducedCriticalEoSFittingParameters[Species].b;
		double cRed = ReducedCriticalEoSFittingParameters[Species].c;
		double dRed = ReducedCriticalEoSFittingParameters[Species].d;

		// See eqn 5 in the cited paper (w/ some simplifications)
		var term1 = cRed * Math.Log(Math.Abs(1 - cRed / VMolRed));
		term1 += dRed * Math.Log(Math.Abs((VMolRed - bRed) / (VMolRed - cRed)));
		term1 -= bRed * Math.Log(Math.Abs(1 - bRed / VMolRed));
		term1 /= cRed - bRed;
		var sqrt2 = Math.Sqrt(2);
		var term2 = Math.Log(Math.Abs((VMolRed + bRed * (1 + sqrt2)) / VMolRed + bRed * (1 - sqrt2)));
		term2 *= aRed / TRed / zCrit / bRed / 2 / sqrt2;
		term2 += (bRed - dRed) / (VMolRed - bRed);
		term2 += (cRed - dRed) / (VMolRed - cRed);
		var term3 = Math.Log(Math.Abs((VMolRed + bRed * (1 + sqrt2)) / VMolRed + bRed * (1 - sqrt2)));
		term3 *= (aRedTDerivative(T) - aRed/TRed) / zCrit / bRed / 2 / sqrt2;

		var val = term1 + term2 + term3 + Math.Log(CompressibilityFactor(T,P,VMol));
		return val * R;
	}

	#endregion

	#region Phase equilibrum
	public override Dictionary<string, Volume> PhaseFinder(Temperature T, Pressure P, bool ignoreEquilibrium = false)
	{
        // Initialize the empty list.
        var list = new Dictionary<string, Volume>();

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

		/* The fluid roots are a little more complex than that, but because this EoS is still a polynomial in terms of z,
		 * derivatives w/rt z are easy to calculate and Rolle's theorem can easily be used to find regions of the function
		 * with only one root. However, this is a degree 4 polynomial, so the third derivative is linear and not the second.
		 * The root between turningPoint1 and turningPoint2 is the s-curve root and is non-physical, so is not calculated.
		 * The root below turningPoint0 is the solid root and was already calculated.
		 */
		Volume joltPoint = ZJoltPoint(T, P);
		Volume inflectionPoint0 = ZInflectionFinder(T, P, b, joltPoint);
		Volume inflectionPoint1 = ZInflectionFinder(T, P, joltPoint, 1);
		Volume turningPoint0 = ZTurnFinder(T, P, b, inflectionPoint0);
		Volume turningPoint1 = ZTurnFinder(T, P, inflectionPoint0, inflectionPoint1);
		Volume turningPoint2 = ZTurnFinder(T, P, inflectionPoint1, 1);

		/* If the Z at turningPoint1 is positive, then there will be no real roots corresponding to the liquid phase.
		 * If the Z at turningPoint2 is negative, then there will be no real roots corresponding to the vapor phase.
		 */
		bool flagRealRoot_L = ZEquation(T, P, turningPoint1) <= 0;
		bool flagRealRoot_V = ZEquation(T, P, turningPoint2) >= 0;

		Volume VMol_L = flagRealRoot_L ? ZRootFinder(T, P, turningPoint0, turningPoint1) : double.NaN;
		Volume VMol_V = flagRealRoot_V ? ZRootFinder(T, P, turningPoint2, 1) : double.NaN;

        // If "ignoreEquilibrium" is set to true, we do not need to copmare fugacities to determine equilibrium phases.
        if (ignoreEquilibrium) { list.Add("solid", VMol_S); list.Add("liquid", VMol_L); list.Add("vapor", VMol_V); return list; }

		/* Now that the predicted phases have been found, estimate the fugacities (or more precisely, the fugacity coefficients)
		 * to determine whether that phase corresponds to a real state in the equilibrium. 
		 */
		return EquilibriumPhases(T, P);
	}

	public override Pressure VaporPressure(Temperature T)
	{
		return double.NaN;
	}

	public Pressure SublimationPressure(Temperature T)
	{
		return double.NaN;
	}

	public override Temperature BoilingTemperature(Pressure P)
	{
		/* Use a variant of Newton's method of rootfinding starting near the critical point and travelling down the curve
		 * until the specified pressure is reached.
		 * Convergence of this algorithm is almost certain because these curves are continuous and
		 * monotonic in their domain.
		 */
		var guessT = speciesData.critT - 0.5;
		var guessPvap = VaporPressure(guessT);

		// Check if the boiling temperature exists at the given pressure.
		var phasesKeys = EquilibriumPhases(P);
		var findPhases = new List<string> { "liquid", "vapor" };
		if (double.IsNaN(guessPvap) || findPhases.All(phasesKeys.Contains))
		{
			return new Temperature(double.NaN, ThermoVarRelations.SaturationTemperature);
		}

		while (Math.Abs(P - guessPvap) >= 10)
		{
			// Approximate local derivative with backward difference.
			var dT = -0.5;
			var Pvap1 = VaporPressure(guessT);
			var Pvap2 = VaporPressure(guessT + dT);
			var dPdT = (Pvap2 - Pvap1) / dT;
			// Use local derivative to come up with a new guess for the boiling temperature.
			guessT -= (guessPvap - P) / dPdT;
			// Calculate vapor pressure at this new guess. Loop ends if this guessPvap is close to P.
			guessPvap = VaporPressure(guessT);
		}

		return new Temperature(guessT, ThermoVarRelations.SaturationTemperature);
	}
	#endregion
}
