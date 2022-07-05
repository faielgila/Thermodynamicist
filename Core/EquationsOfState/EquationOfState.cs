using Core.VariableTypes;

namespace Core.EquationsOfState;

public abstract class EquationOfState
{
	public const double R = Constants.R;
	public const double precisionLimit = Constants.PrecisionLimit;

	public bool UseHighTempData = false;
	public readonly Chemical Species;
	public (double molarMass, Temperature critT, Pressure critP, double acentricFactor, Temperature boilT) speciesData;
	public (double[] vals, double[] lims) speciesCpData;
	public (double[] vals, double[] lims) speciesHighTempCpData;

	protected EquationOfState(Chemical species)
	{
		Species = species;
		speciesData = Constants.ChemicalData[species];
		speciesCpData = Constants.IdealGasCpConstants[species];
		speciesHighTempCpData = Constants.HighTempIdealGasCpConstants[species];
	}

	/// <summary>
	/// Returns the pressure of the system in the given state, defined by temperature and molar volume.
	/// </summary>
	/// <param name="T">temperature, measured in [K]</param>
	/// <param name="VMol">molar volume, measured in [m³/mol]</param>
	/// <returns>pressure, measured in [Pa]</returns>
	public abstract Pressure Pressure(Temperature T, MolarVolume VMol);

	public double CompressibilityFactor(Temperature T, Pressure P, MolarVolume VMol)
	{
		return P * VMol / (R * T);
	}

	/// <summary>
	/// Returns the fugacity coefficient of the system in the given state, defined by temperature, pressure, and molar volume.
	/// </summary>
	/// <param name="T">temperature, measured in [K]</param>
	/// <param name="P">pressure, measured in [Pa]</param>
	/// <param name="VMol">molar volume, measured in [m³/mol]</param>
	/// <returns>fugacity coefficient f/P, unitless</returns>
	public abstract double FugacityCoeff(Temperature T, Pressure P, MolarVolume VMol);

	public double Fugacity(Temperature T, Pressure P, MolarVolume VMol) { return FugacityCoeff(T, P, VMol) * P; }

	/// <summary>
	/// Uses the equation of state to find the phases present in a system given a temperature and pressure.
	/// Checks for valid equilibrium using the fugacity coefficient.
	/// </summary>
	/// <param name="T">temperature, measured in [K]</param>
	/// <param name="P">pressure, measured in [Pa]</param>
	/// <param name="ignoreEquilibrium">skips fugacity comparison which determines phase equilibrium state</param>
	/// <returns>
	/// molar volume at each the liquid and vapor phases, measured in [m³/mol].
	/// Returns 0 if the phase is not present.</returns>
	public abstract (MolarVolume L, MolarVolume V) PhaseFinder(Temperature T, Pressure P, bool ignoreEquilibrium = false);

	public abstract
		(double Z, MolarInternalEnergy U, MolarEnthalpy H, MolarEntropy S,
		MolarGibbsEnergy G, MolarHelmholtzEnergy A, double f)
		GetAllStateVariables(Temperature T, Pressure P, MolarVolume VMol);
}