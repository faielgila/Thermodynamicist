using Core.VariableTypes;

namespace Core.EquationsOfState;

/// <summary>
/// Represents the ideal gas law and related functions.
/// </summary>
public static class IdealGasLaw
{
	/// <inheritdoc cref="Constants.R"/>
	public const double R = Constants.R;
	
	/// <inheritdoc cref="EquationOfState.Pressure"/>
	public static Pressure Pressure(Temperature T, Volume VMol) { return R * T / VMol; }

	/// <summary>
	/// Returns the molar volume of the system in the given state,
	/// defined by temperature and pressure.
	/// </summary>
	/// <param name="T">temperature, in [K]</param>
	/// <param name="P">pressure, in [Pa]</param>
	/// <returns>ideal gas molar volume, in [m³/mol]</returns>
	public static Volume Volume(Temperature T, Pressure P) { return R * T / P; }
}