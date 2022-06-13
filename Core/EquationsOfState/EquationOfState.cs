using Core.VariableTypes;

namespace Core.EquationsOfState;

public abstract class EquationOfState
{
	public const double R = Constants.R;
	public const double precisionLimit = Constants.precisionLimit;
	
	public readonly Chemical Species;
	public (double molarMass, Temperature critT, Pressure critP, double acentricFactor, Temperature boilT) speciesData;

	protected EquationOfState(Chemical species)
	{
		Species = species; 
	}

	/// <summary>
	/// Returns the pressure of the system in the given state, defined by temperature and molar volume.
	/// </summary>
	/// <param name="T">temperature, measured in [K]</param>
	/// <param name="VMol">molar volume, measured in [m³/mol]</param>
	/// <returns>pressure, measured in [Pa]</returns>
	public abstract Pressure Pressure(Temperature T, MolarVolume VMol);

	/// <summary>
	/// Returns the fugacity coefficient of the system in the given state, defined by temperature, pressure, and molar volume.
	/// </summary>
	/// <param name="T">temperature, measured in [K]</param>
	/// <param name="P">pressure, measured in [Pa]</param>
	/// <param name="VMol">molar volume, measured in [m³/mol]</param>
	/// <returns>fugacity coefficient f/P, unitless</returns>
	public abstract double FugacityCoeff(Temperature T, Pressure P, MolarVolume VMol);

	/// <summary>
	/// Uses the equation of state to find the phases present in a system given a temperature and pressure.
	/// Checks for valid equilibrium using the fugacity coefficient.
	/// </summary>
	/// <param name="T">temperature, measured in [K]</param>
	/// <param name="P">pressure, measured in [Pa]</param>
	/// <returns>
	/// molar volume at each the liquid and vapor phases, measured in [m³/mol].
	/// Returns 0 if the phase is not present.</returns>
	public abstract MolarVolume[] PhaseFinder(Temperature T, Pressure P);
}