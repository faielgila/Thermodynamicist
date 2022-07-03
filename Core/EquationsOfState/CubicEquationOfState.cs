using Core.VariableTypes;

namespace Core.EquationsOfState;

public abstract class CubicEquationOfState : EquationOfState
{
	protected CubicEquationOfState(Chemical species) : base(species) { }

	/// <summary>
	/// Represents the equation of state as a cubic function of the compressibility factor, z = P*VMol/(R*T).
	/// </summary>
	/// <param name="T">temperature, measured in [K]</param>
	/// <param name="P">pressure, measured in [Pa]</param>
	/// <param name="VMol">molar volume, measured in [m³/mol]</param>
	/// <returns>value defined by the equation z³+αz²+βz+γ.</returns>
	public abstract double ZCubicEqn(Temperature T, Pressure P, MolarVolume VMol);

	/// <summary>
	/// Represents the derivative of the cubic equation, defined in <see cref="ZCubicEqn"/>.
	/// </summary>
	/// <param name="T">temperature, measured in [K]</param>
	/// <param name="P">pressure, measured in [Pa]</param>
	/// <param name="VMol">molar volume, measured in [m³/mol]</param>
	/// <returns>value of the derivative of the cubic equation of state</returns>
	public abstract double ZCubicDerivative(Temperature T, Pressure P, MolarVolume VMol);

	/// <summary>
	/// Gives the value of the molar volume of the inflection point of the cubic equation.
	/// </summary>
	/// <param name="T">temperature, measured in [K]</param>
	/// <param name="P">pressure, measured in [Pa]</param>
	/// <returns>molar volume, measured in [m³/mol]</returns>
	public abstract double ZCubicInflectionPoint(Temperature T, Pressure P);
	
	/// <inheritdoc cref="EquationOfState.PhaseFinder"/>
	public override (MolarVolume L, MolarVolume V) PhaseFinder(Temperature T, Pressure P, bool ignoreEquilibrium = false)
	{
		/*
		In order to find the roots of the cubic equation, the bisection algorithm needs a range to check for which
		there is only one root (or no roots; the algorithm has been implemented to return 0 if a root is not found).
		Calculus guarantees that there is only one root between each turning point of the function, which means there
		is one root from 0 to the first turning point, one root between the turning points, and one root from the
		second turning point to "infinity" (this code stops at 1, since "one cubic meter per mole" can well be assumed
		to be a vacuum and thus is not of interest for these calculations).
		To get the turning points, we apply the same reasoning to find the roots of the derivative. Conveniently,
		the inflection point of a cubic is a single point, which means the value of VMol at that point is analytically
		defined and does not need a root-finding algorithm.
		Once the inflection point is calculated, the two turning points are found. Once the turning points are found,
		the bisection algorithm is applied to the cubic equation over each range
		(excluding the root between the turning points, since it does not correspond to any real state).
		*/
		MolarVolume inflectionVMol = ZCubicInflectionPoint(T, P);
		MolarVolume turningPoint1 = ZCubicTurnFinder(T, P, 0, inflectionVMol);
		MolarVolume turningPoint2 = ZCubicTurnFinder(T, P, inflectionVMol, 1);
		MolarVolume VMol_L = ZCubicRootFinder(T, P, 0, turningPoint1);
		MolarVolume VMol_V = ZCubicRootFinder(T, P, turningPoint2, 1);
		
		// If "ignoreEquilibrium" is set to true, we do not need to copmare fugacities to determine equilibrium phases.
		if (ignoreEquilibrium) { return (VMol_L, VMol_V); }
		
		/*
		Now that the predicted phases have been found, we can calculate the fugacity of each phase to determine whether
		the predicted phase equilibrium corresponds to a real equilibrium state. If the fugacities are roughly equal,
		then the two phases are likely in equilibrium. If one fugacity is larger than the other, then the system will
		prefer the lower fugacity phase over the higher one, and as such only the phase with the lower fugacity exists
		at that state. This code uses the fugacity coefficient instead of fugacity directly since it requires
		slightly less calculation and doesn't require dividing by a large number (which reduces precision).
		*/
		double f_L = FugacityCoeff(T, P, VMol_L);
		double f_V = FugacityCoeff(T, P, VMol_V);
		
		/* Note that 0.1 is used here instead of the precision limit because the exponential nature of the fugacity
		 coefficient means that a small different in precision leads to quite a different number. In the reality of
		 numerical solutions for these kinds of equations, a difference between fugacities of 0.1 is more than enough
		 to conclude that the system in is phase equilibrium.
		 */
		if (Math.Abs(f_L - f_V) < 0.1) return ( VMol_L, VMol_V );
		if (f_L > f_V) return ( 0, VMol_V );
		if (f_L < f_V) return ( VMol_L, 0 );
		
		// This statement should never be reached!
		// TODO: Throw an exception if this line is reached.
		return ( 0, 0 );
	}

	/// <summary>
	/// Finds a turning point of the cubic form of the equation of state within the given bounds for molar volume.
	/// Applies the bisection method of root-finding to the first derivative of the cubic form.
	/// <remarks>Only use this finder if you are certain only one turning point exists in the given range!</remarks>
	/// </summary>
	/// <param name="T">temperature, measured in [K]</param>
	/// <param name="P">pressure, measured in [Pa]</param>
	/// <param name="minVMol">minimum molar volume, measured in [m³/mol]</param>
	/// <param name="maxVMol">maximum molar volume, measured in [m³/mol]</param>
	public MolarVolume ZCubicTurnFinder(Temperature T, Pressure P, MolarVolume minVMol, MolarVolume maxVMol)
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
	public MolarVolume ZCubicRootFinder(Temperature T, Pressure P, MolarVolume minVMol, MolarVolume maxVMol)
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
}