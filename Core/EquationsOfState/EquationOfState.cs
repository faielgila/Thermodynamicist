using Core.VariableTypes;

namespace Core.EquationsOfState;

public abstract class EquationOfState
{
	public const double R = Constants.R;
	public const double precisionLimit = Constants.PrecisionLimit;

	public bool UseHighTempData = false;
	public readonly (Temperature refT, Pressure refP) ReferenceState = (298.15, 100e3);
	public readonly Chemical Species;
	public (double molarMass, Temperature critT, Pressure critP, double acentricFactor, Temperature boilT) speciesData;
	public (double[] vals, double[] lims) speciesCpData;
	public (double[] vals, double[] lims) speciesHighTempCpData;

	protected EquationOfState(Chemical species, (Temperature T, Pressure P) referenceState = default)
	{
		ReferenceState = referenceState;
		Species = species;
		speciesData = Constants.ChemicalData[species];
		speciesCpData = Constants.IdealGasCpConstants[species];
		//TODO: when high-temp calculations are fleshed out, reactivate this line.
		//speciesHighTempCpData = Constants.HighTempIdealGasCpConstants[species];
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

	#region State Variables - Enthalpy

	/// <summary>
	/// Calculates the enthalpy change between two states assuming ideal gas behavior.
	/// Ideal enthalpy change is a pure function of temperature.
	/// </summary>
	/// <param name="T1">Initial temperature</param>
	/// <param name="T2">Final temperature</param>
	/// <returns>Molar Enthalpy, change</returns>
	/// <exception cref="NotImplementedException">Use of high-temperature Cp data is not currently supported.</exception>
	public MolarEnthalpy IdealMolarEnthalpyChange(Temperature T1, Temperature T2)
	{
		double[] c;
		if (!UseHighTempData)
		{
			// TODO: show warning if T1 or T2 is outside the TLimits specified in the data
			var lowerTLimit = speciesCpData.lims[0];
			var upperTLimit = speciesCpData.lims[1];
			c = speciesCpData.vals;
		}
		else
		{
			throw new NotImplementedException();
		}

		var deltaT2 = T2 * T2 - T1 * T1;
		var deltaT3 = T2 * T2*T2 - T1 * T1*T1;
		var deltaT4 = Math.Pow(T2,4) - Math.Pow(T1,4);
		var value = c[0] * (T2 - T1) + c[1]/2 * deltaT2 + c[2]/3 * deltaT3 + c[3]/4 * deltaT4;
		return new MolarEnthalpy(value, ThermoVarRelations.Change);
	}

	public abstract MolarEnthalpy DepartureEnthalpy(Temperature T, Pressure P, MolarVolume VMol);
	
	/// <summary>
	/// Calculates the enthalpy change between two states using departure functions and convenient paths.
	/// </summary>
	/// <returns></returns>
	public MolarEnthalpy MolarEnthalpyChange
		(Temperature T1, Pressure P1, MolarVolume VMol1, Temperature T2, Pressure P2, MolarVolume VMol2)
	{
		var pathA = DepartureEnthalpy(T1, P1, VMol1);
		var pathB = IdealMolarEnthalpyChange(T1, T2);
		var pathC = DepartureEnthalpy(T2, P2, VMol2);
		var totalPath = -pathA + pathB + pathC;
		return new MolarEnthalpy(totalPath, ThermoVarRelations.Change);
	}
	
	public MolarEnthalpy ReferenceMolarEnthalpy(Temperature T, Pressure P, MolarVolume VMol)
	{
		var pathB = IdealMolarEnthalpyChange(273.15+25, T);
		var pathC = DepartureEnthalpy(T, P, VMol);
		var totalPath = pathB + pathC;
		return new MolarEnthalpy(totalPath, ThermoVarRelations.Change);
	}

	#endregion

	#region State Variables - Entropy
	
	public abstract MolarEntropy DepartureEntropy(Temperature T, Pressure P, MolarVolume VMol);
	
	public MolarEntropy IdealMolarEntropyChange(Temperature T1, Pressure P1, Temperature T2, Pressure P2)
	{
		double[] c;
		if (!UseHighTempData)
		{
			// TODO: show warning if T1 or T2 is outside the TLimits specified in the data
			var lowerTLimit = speciesCpData.lims[0];
			var upperTLimit = speciesCpData.lims[1];
			c = speciesCpData.vals;
		}
		else
		{
			throw new NotImplementedException();
		}
		
		var deltaT2 = T2 * T2 - T1 * T1;
		var deltaT3 = T2 * T2*T2 - T1 * T1*T1;
		var value = c[0] * Math.Log(T2 / T1) + c[1] * (T2 - T1) + c[2] / 2 * deltaT2 + c[3] / 3 * deltaT3;
		value -= R * Math.Log(P2 / P1);
		return new MolarEntropy(value, ThermoVarRelations.Change);
	}

	public MolarEntropy MolarEntropyChange
		(Temperature T1, Pressure P1, MolarVolume VMol1, Temperature T2, Pressure P2, MolarVolume VMol2)
	{
		var pathA = DepartureEntropy(T1, P1, VMol1);
		var pathB = IdealMolarEntropyChange(T1, P1, T2, P2);
		var pathC = DepartureEntropy(T2, P2, VMol2);
		var totalPath = -pathA + pathB + pathC;
		return new MolarEntropy(totalPath, ThermoVarRelations.Change);
	}
	
	public MolarEntropy ReferenceMolarEntropy(Temperature T, Pressure P, MolarVolume VMol)
	{
		var pathB = IdealMolarEntropyChange(273.15+25, 100e3, T, P);
		var pathC = DepartureEntropy(T, P, VMol);
		var totalPath = pathB + pathC;
		return new MolarEntropy(totalPath);
	}

	#endregion

	#region State Variables - other

	public MolarInternalEnergy ReferenceMolarInternalEnergy(Temperature T, Pressure P, MolarVolume VMol)
	{
		return new MolarInternalEnergy(ReferenceMolarEnthalpy(T, P, VMol) - P * VMol);
	}

	public MolarGibbsEnergy ReferenceMolarGibbsEnergy(Temperature T, Pressure P, MolarVolume VMol)
	{
		var H = ReferenceMolarEnthalpy(T, P, VMol);
		var S = ReferenceMolarEntropy(T, P, VMol);
		return new MolarGibbsEnergy(H - T * S);
	}
	
	public MolarHelmholtzEnergy ReferenceMolarHelmholtzEnergy(Temperature T, Pressure P, MolarVolume VMol)
	{
		var U = ReferenceMolarInternalEnergy(T, P, VMol);
		var S = ReferenceMolarEntropy(T, P, VMol);
		return new MolarHelmholtzEnergy(U - T * S);
	}

	#endregion

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

	public (double Z, MolarInternalEnergy U, MolarEnthalpy H, MolarEntropy S,
		MolarGibbsEnergy G, MolarHelmholtzEnergy A, double f)
		GetAllStateVariables(Temperature T, Pressure P, MolarVolume VMol)
	{
		var Z = CompressibilityFactor(T, P, VMol);
		var U = ReferenceMolarInternalEnergy(T, P, VMol);
		var H = ReferenceMolarEnthalpy(T, P, VMol);
		var S = ReferenceMolarEntropy(T, P, VMol);
		var G = ReferenceMolarGibbsEnergy(T, P, VMol);
		var A = ReferenceMolarHelmholtzEnergy(T, P, VMol);
		var f = FugacityCoeff(T, P, VMol);

		return (Z, U, H, S, G, A, f);
	}
}