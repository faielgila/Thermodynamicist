using Core.VariableTypes;

namespace Core.EquationsOfState;

/// <summary>
/// Abstract representation of any cubic equation of state.
/// Extends <see cref="EquationOfState"/>.
/// See also: <seealso cref="PengRobinsonEOS"/>, <seealso cref="VanDerWaalsEOS"/>.
/// </summary>
public abstract class CubicEquationOfState(Chemical species) : EquationOfState(species, ["liquid", "vapor"])
{
	public override Volume CriticalMolarVolume()
	{
		return ZCubicInflectionPoint(speciesData.critT, speciesData.critP);
	}

	#region Cubic form and derivatives

	/// <summary>
	/// Represents the equation of state as a cubic function of the compressibility factor, z = P*VMol/(R*T).
	/// </summary>
	/// <param name="T">temperature, measured in [K]</param>
	/// <param name="P">pressure, measured in [Pa]</param>
	/// <param name="VMol">molar volume, measured in [m³/mol]</param>
	/// <returns>value defined by the equation z³+αz²+βz+γ.</returns>
	public abstract double ZCubicEqn(Temperature T, Pressure P, Volume VMol);

	/// <summary>
	/// Represents the derivative of the cubic equation, defined in <see cref="ZCubicEqn"/>.
	/// </summary>
	/// <param name="T">temperature, measured in [K]</param>
	/// <param name="P">pressure, measured in [Pa]</param>
	/// <param name="VMol">molar volume, measured in [m³/mol]</param>
	/// <returns>value of the derivative of the cubic equation of state</returns>
	public abstract double ZCubicDerivative(Temperature T, Pressure P, Volume VMol);

	/// <summary>
	/// Gives the value of the molar volume of the inflection point of the cubic equation.
	/// </summary>
	/// <param name="T">temperature, measured in [K]</param>
	/// <param name="P">pressure, measured in [Pa]</param>
	/// <returns>molar volume, measured in [m³/mol]</returns>
	public abstract Volume ZCubicInflectionPoint(Temperature T, Pressure P);

	/// <summary>
	/// Finds a turning point of the cubic form of the equation of state within the given bounds for molar volume.
	/// Applies the bisection method of root-finding to the first derivative of the cubic form.
	/// <remarks>Only use this finder if you are certain only one turning point exists in the given range!</remarks>
	/// </summary>
	/// <param name="T">temperature, measured in [K]</param>
	/// <param name="P">pressure, measured in [Pa]</param>
	/// <param name="minVMol">minimum molar volume, measured in [m³/mol]</param>
	/// <param name="maxVMol">maximum molar volume, measured in [m³/mol]</param>
	public Volume ZCubicTurnFinder(Temperature T, Pressure P, Volume minVMol, Volume maxVMol)
	{
		double midVMol = (minVMol + maxVMol) / 2;
		while ((maxVMol - minVMol) > precisionLimit)
		{
			int maxVMolSign = Math.Sign(ZCubicDerivative(T, P, maxVMol));
			int midVMolSign = Math.Sign(ZCubicDerivative(T, P, midVMol));
			int minVMolSign = Math.Sign(ZCubicDerivative(T, P, minVMol));
			if (midVMolSign == maxVMolSign) maxVMol = midVMol;
			if (midVMolSign == minVMolSign) minVMol = midVMol;
			if (midVMolSign == 0) return midVMol;

			midVMol = (minVMol + maxVMol) / 2;
		}
		return midVMol;
	}

	/// <summary>
	/// Finds a root of the cubic form of the equation of state within the given bounds for molar volume.
	/// Applies the bisection method of root-finding to the cubic form.
	/// <remarks>Only use this finder if you are certain only one turning point exists in the given range!</remarks>
	/// </summary>
	/// <param name="T">temperature, measured in [K]</param>
	/// <param name="P">pressure, measured in [Pa]</param>
	/// <param name="minVMol">minimum molar volume, measured in [m³/mol]</param>
	/// <param name="maxVMol">maximum molar volume, measured in [m³/mol]</param>
	public Volume ZCubicRootFinder(Temperature T, Pressure P, Volume minVMol, Volume maxVMol)
	{
		double midVMol = (minVMol + maxVMol) / 2;
		while ((maxVMol - minVMol) > precisionLimit)
		{
			int maxVMolSign = Math.Sign(ZCubicEqn(T, P, maxVMol));
			int midVMolSign = Math.Sign(ZCubicEqn(T, P, midVMol));
			int minVMolSign = Math.Sign(ZCubicEqn(T, P, minVMol));
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
		// Define the empty list.
		var list = new Dictionary<string, Volume>();

		/* In order to find the roots of the cubic equation, the bisection algorithm needs a range to check for which
		 * there is only one root (or no roots; the algorithm has been implemented to return 0 if a root is not found).
		 * Calculus guarantees that there is only one root between each turning point of the function, which means there
		 * is one root from 0 to the first turning point, one root between the turning points, and one root from the
		 * second turning point to "infinity" (this code stops at 1, since "one cubic meter per mole" can well be assumed
		 * to be a vacuum and thus is not of interest for these calculations).
		 * To get the turning points, we apply the same reasoning to find the roots of the derivative. Conveniently,
		 * the inflection point of a cubic is a single point, which means the value of VMol at that point is analytically
		 * defined and does not need a root-finding algorithm.
		 * Once the inflection point is calculated, the two turning points are found. Once the turning points are found,
		 * the bisection algorithm is applied to the cubic equation over each range
		 * (excluding the root between the turning points, since it does not correspond to any real state).
		 */
		Volume inflectionVMol = ZCubicInflectionPoint(T, P);
		Volume turningPoint1 = ZCubicTurnFinder(T, P, 0, inflectionVMol);
		Volume turningPoint2 = ZCubicTurnFinder(T, P, inflectionVMol, 1);

		/* If the Z at the first turning point is negative, then there will be no real roots other than the single root
		 * corresponding to the vapor phase.
		 * Similarly, if the Z at the second turning point is positive, there will be no real roots other than
		 * the liquid root.
		 * Thus, further calculation of those roots would be useless.
		 */
		bool flagRealRoot_L = ZCubicEqn(T, P, turningPoint1) >= 0;
		bool flagRealRoot_V = ZCubicEqn(T, P, turningPoint2) <= 0;

		Volume VMol_L = flagRealRoot_L ? ZCubicRootFinder(T, P, 0, turningPoint1) : double.NaN;
		Volume VMol_V = flagRealRoot_V ? ZCubicRootFinder(T, P, turningPoint2, 1) : double.NaN;

		// If one of the vapor or liquid roots are not real roots, then there is no equilibrium to determine.
		if (!flagRealRoot_V) { list.Add("liquid", VMol_L); return list; }
		if (!flagRealRoot_L) { list.Add("vapor", VMol_V); return list; }

		// If "ignoreEquilibrium" is set to true, we do not need to copmare fugacities to determine equilibrium phases.
		// Note that at this point, the only way to reach this part of the logic is if both the vapor and liquid roots exist.
		if (ignoreEquilibrium) { list.Add("liquid", VMol_L); list.Add("vapor", VMol_V); return list; }

		/* Now that the predicted phases have been found, estimate the fugacities (or more precisely, the fugacity coefficients)
		 * to determine whether that phase corresponds to a real state in the equilibrium. 
		 */
		return EquilibriumPhases(T, P);
	}

	public override Pressure VaporPressure(Temperature T)
	{
		// Check if a vapor pressure exists at the temperature.
		if (T >= speciesData.critT) { return new Pressure(double.NaN, ThermoVarRelations.VaporPressure); }

		/* Initial guess must be within the S-curve region of the isotherm or this method will not converge.
		 * Because the critical point is the state for which the liquid and vapor phases will diverge from
		 * (as the temperature and/or pressure drops below the critical point), the critical volume must
		 * be in-between the volumes of the liquid and vapor phases; that is, the critical volume always
		 * lies inside the s-curve region. That means that the critical volume provides a perfect starting point.
		 * This fulfills the first of two requirements for a good initial guess for the Sandler algorithm employed below.
		 */
		var VMol = CriticalMolarVolume();

		/* Because the critical molar volume is guaranteed to be inside the s-curve region, simple
		 * gradient ascent can be applied until the pressure is positive. If the pressure is already positive,
		 * then the ascent loop is skipped.
		 * This fulfills the second requirement for a good initial guess.
		 */
		// The learning rate has to adapt to the exaggerated shape of isotherms far below the critical temperature.
		var h = Math.Pow(10, -16.5 + 2 / speciesData.critT - 2 / T);
		var P = Pressure(T, VMol);
		while (P <= 0)
		{
			VMol += h * PVPartialDerivative(T, VMol);
			P = Pressure(T, VMol);
		}

		var v = PhaseFinder(T, P, true); // get the molar volumes for the two phases
		var f_L = Fugacity(T, P, v["liquid"]); // fugacity for the liquid phase
		var f_V = Fugacity(T, P, v["vapor"]); // fugacity for the vapor phase

		// Increment the initial pressure guess until precision is reached.
		// taken from Sandler, Figure 7.5-1
		while (Math.Abs(f_L / f_V - 1) > precisionLimit * Math.Pow(10, 5))
		{
			P = P * f_L / f_V;
			v = PhaseFinder(T, P, true);
			f_L = Fugacity(T, P, v["liquid"]);
			f_V = Fugacity(T, P, v["vapor"]);
		}

		return new Pressure(P, ThermoVarRelations.VaporPressure);
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
		//var phasesKeys = EquilibriumPhases(P);
		//var findPhases = new List<string> { "liquid", "vapor" };
		//if (double.IsNaN(guessPvap) || findPhases.All(phasesKeys.Contains))
		//{
		//	return new Temperature(double.NaN, ThermoVarRelations.SaturationTemperature);
		//}

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

	/// <summary>
	/// Calculates the enthalpy change between liquid-vapor equilibrium, i.e. the vaporization enthalpy.
	/// If either the vapor or liquid phase are not present, the enthalpy will be returned as NaN.
	/// </summary>
	/// <param name="T">temperature, in [K]</param>
	/// <param name="P">pressure, in [Pa]</param>
	/// <param name="phaseFrom">initial phase</param>
	/// <param name="phaseTo">final phase</param>
	/// <returns>If it exists, vaporization enthalpy, in [J/mol]; If is does not exist, NaN</returns>
	public override Enthalpy PhaseChangeEnthalpy(Temperature T, Pressure P, string phaseFrom, string phaseTo)
	{
		// Check for phase change and direction of change
		if (string.Equals(phaseFrom, phaseTo)) return new Enthalpy(0, ThermoVarRelations.Change);
		if (string.Equals(phaseFrom, "liquid") && string.Equals(phaseTo, "vapor" )) return  VaporizationEnthalpy(T, P);
		if (string.Equals(phaseFrom, "vapor" ) && string.Equals(phaseTo, "liquid")) return -VaporizationEnthalpy(T, P);
		else return double.NaN;

		// Calculates the vaporization enthalpy at the given temperature and pressure.
		Enthalpy VaporizationEnthalpy(Temperature T, Pressure P)
		{
			var phases = PhaseFinder(T, P, true);
			if (!phases.ContainsKey("vapor") || !phases.ContainsKey("liquid")) return double.NaN;
			var H_V = ReferenceMolarEnthalpy(T, P, phases["vapor"].Value);
			var H_L = ReferenceMolarEnthalpy(T, P, phases["liquid"].Value);
			return new Enthalpy(H_V - H_L, ThermoVarRelations.OfVaporization);
		}
	}

	/// <summary>
	/// Calculates the entropy change between liquid-vapor equilibrium, i.e. the vaporization enthalpy.
	/// If either the vapor or liquid phase are not present, the entropy will be returned as NaN.
	/// </summary>
	/// <param name="T">temperature, in [K]</param>
	/// <param name="P">pressure, in [Pa]</param>
	/// <param name="phaseFrom">initial phase</param>
	/// <param name="phaseTo">final phase</param>
	/// <returns>If it exists, vaporization entropy, in [J/K/mol]; If is does not exist, NaN</returns>
	public override Entropy PhaseChangeEntropy(Temperature T, Pressure P, string phaseFrom, string phaseTo)
	{
		// Check for phase change and direction of change
		if (string.Equals(phaseFrom, phaseTo)) return new Entropy(0, ThermoVarRelations.Change);
		if (string.Equals(phaseFrom, "liquid") && string.Equals(phaseTo, "vapor")) return VaporizationEntropy(T, P);
		if (string.Equals(phaseFrom, "vapor") && string.Equals(phaseTo, "liquid")) return -VaporizationEntropy(T, P);
		else return double.NaN;

		// Calculates the vaporization enthalpy at the given temperature and pressure.
		Entropy VaporizationEntropy(Temperature T, Pressure P)
		{
			var phases = PhaseFinder(T, P, true);
			if (!phases.ContainsKey("vapor") || !phases.ContainsKey("liquid")) return double.NaN;
			var S_V = ReferenceMolarEntropy(T, P, phases["vapor"].Value);
			var S_L = ReferenceMolarEntropy(T, P, phases["liquid"].Value);
			return new Entropy(S_V - S_L, ThermoVarRelations.OfVaporization);
		}
	}

	/// <summary>
	/// Equivalent to the BoilingTemperature method.
	/// </summary>
	/// <returns>boiling temperature [K]</returns>
	/// <exception cref="KeyNotFoundException">Thrown when phaseFrom or phaseTo is not modeled by the EoS.</exception>
	public override Temperature PhaseChangeTemperature(Pressure P, string phaseFrom, string phaseTo)
	{
		if (!ModeledPhases.Contains(phaseFrom))
			throw new KeyNotFoundException($"Cubic EoS only models vapor and liquid. {phaseFrom} phase is not supported.");
		if (!ModeledPhases.Contains(phaseTo))
			throw new KeyNotFoundException($"Cubic EoS only models vapor and liquid. {phaseTo} phase is not supported.");
		else return BoilingTemperature(P);
	}

	/// <summary>
	/// Equivalent to the VaporPressure method.
	/// </summary>
	/// <returns>liquid-vapor pressure [Pa]</returns>
	/// <exception cref="KeyNotFoundException">Thrown when phaseFrom or phaseTo is not modeled by the EoS.</exception>
	public override Pressure PhaseChangePressure(Temperature T, string phaseFrom, string phaseTo)
	{
		if (!ModeledPhases.Contains(phaseFrom))
			throw new KeyNotFoundException($"Cubic EoS only models vapor and liquid. {phaseFrom} phase is not supported.");
		if (!ModeledPhases.Contains(phaseTo))
			throw new KeyNotFoundException($"Cubic EoS only models vapor and liquid. {phaseTo} phase is not supported.");
		else return VaporPressure(T);
	}
}