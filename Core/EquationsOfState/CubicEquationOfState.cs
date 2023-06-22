using Core.VariableTypes;

namespace Core.EquationsOfState;

/// <summary>
/// Abstract representation of any cubic equation of state.
/// Extends <see cref="EquationOfState"/>.
/// </summary>
/// See also: <seealso cref="PengRobinsonEOS"/>, <seealso cref="VanDerWaalsEOS"/>.
/// </summary>
public abstract class CubicEquationOfState : EquationOfState
{
	protected CubicEquationOfState(Chemical species) : base(species) { }

	public override Volume CriticalMolarVolume()
	{
		return ZCubicInflectionPoint(speciesData.critT, speciesData.critP);
	}

	#region Partial derivatives

	/// <summary>
	/// Analytically calculates the derivative of pressure with respect to molar volume, holding temperature constant.
	/// </summary>
	/// <param name="T">temperature, in [K]</param>
	/// <param name="VMol">molar volume, in [m³/mol]</param>
	/// <returns>change in P relative to change in V, (∂P/∂V)|T, in [J/mol]</returns>
	public abstract double PVPartialDerivative(Temperature T, Volume VMol);

	#endregion

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

	public override (Volume L, Volume V) PhaseFinder(Temperature T, Pressure P, bool ignoreEquilibrium = false)
	{
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
		Volume VMol_L = ZCubicRootFinder(T, P, 0, turningPoint1);
		Volume VMol_V = ZCubicRootFinder(T, P, turningPoint2, 1);
		
		// If "ignoreEquilibrium" is set to true, we do not need to copmare fugacities to determine equilibrium phases.
		if (ignoreEquilibrium) { return (VMol_L, VMol_V); }
		
		/* Now that the predicted phases have been found, we can calculate the fugacity of each phase to determine whether
		 * the predicted phase equilibrium corresponds to a real equilibrium state. If the fugacities are roughly equal,
		 * then the two phases are likely in equilibrium. If one fugacity is larger than the other, then the system will
		 * prefer the lower fugacity phase over the higher one, and as such only the phase with the lower fugacity exists
		 * at that state. This code uses the fugacity coefficient instead of fugacity directly since it requires
		 * slightly less calculation and doesn't require dividing by a large number (which reduces precision).
		 */
		double f_L = FugacityCoeff(T, P, VMol_L);
		double f_V = FugacityCoeff(T, P, VMol_V);

		/* Note that 0.1 is used here instead of the precision limit because the exponential nature of the fugacity
		 * coefficient means that a small difference in precision leads to quite a different number. In the reality of
		 * numerical solutions for these kinds of equations, a difference between fugacities of 0.1 is more than enough
		 * to conclude that the system in is phase equilibrium.
		 */
		if (Math.Abs(f_L - f_V) < 0.1) return (VMol_L, VMol_V);
		if (f_L > f_V) return (double.NaN, VMol_V);
		if (f_L < f_V) return (VMol_L, double.NaN);
		
		// This statement should never be reached! No phases were found.
		// TODO: Throw an exception if this line is reached.
		return (double.NaN, double.NaN);
	}

	public override Pressure? VaporPressure(Temperature T)
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
		var h = Math.Pow(10, -17 + 1 / speciesData.critT - 1 / T);
		var P = Pressure(T, VMol);
		while (P <= 0)
		{
			VMol += h * PVPartialDerivative(T, VMol);
			P = Pressure(T, VMol);
		}

		var v = PhaseFinder(T, P, true); // get the molar volumes for the two phases
		var f_L = Fugacity(T, P, v.L); // fugacity for the liquid phase
		var f_V = Fugacity(T, P, v.V); // fugacity for the vapor phase

		// Increment the initial pressure guess until precision is reached.
		// taken from Sandler, Figure 7.5-1
		while (Math.Abs(f_L / f_V - 1) > precisionLimit * Math.Pow(10, 5))
		{
			P = P * f_L / f_V;
			v = PhaseFinder(T, P, true);
			f_L = Fugacity(T, P, v.L);
			f_V = Fugacity(T, P, v.V);
		}

		return new Pressure(P, ThermoVarRelations.VaporPressure);
	}

	/// <summary>
	/// Determines which phases are present at equilibrium given the state.
	/// Compares fugacity coefficients of each phase.
	/// </summary>
	/// <param name="T">temperature, measured in [K]</param>
	/// <param name="P">pressure, measured in [Pa]</param>
	/// <param name="VMol_L">molar volume of liquid phase, measured in [m³/mol]</param>
	/// <param name="VMol_V">molar volume of vapor phase, measured in [m³/mol]</param>
	/// <returns>
	/// "0" for phase if phase is not preferred in equilibrium.
	/// "1" for phase if phase is preferred in equilibrium.
	/// "2" for phase if phase is non-physical (if fugacity returns "NaN").
	/// </returns>
	/// TODO: Figure out how to make this a method (moved to EquationOfState.cs) which is generalized to work with any number of output phases.
	public (int L, int V) IsStateInPhaseEquilbirum(Temperature T, Pressure P, Volume VMol_L, Volume VMol_V)
	{
		double f_L = FugacityCoeff(T, P, VMol_L);
		double f_V = FugacityCoeff(T, P, VMol_V);
		if (Math.Abs(f_L - f_V) < 0.1) return (1, 1);
		if (f_L > f_V) return (0, 1);
		if (f_L < f_V) return (1, 0);
		if (double.IsNaN(f_L) && !double.IsNaN(f_V)) return (2, 1);
		if (!double.IsNaN(f_L) && double.IsNaN(f_V)) return (1, 2);

		// This statement should never be reached!
		// TODO: Throw an exception if this line is reached.
		return (0, 0);
	}
}