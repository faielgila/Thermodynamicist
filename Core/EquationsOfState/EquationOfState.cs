using Core.VariableTypes;

namespace Core.EquationsOfState;

/// <summary>
/// Abstract class which stores an equation of state and its related functions,
/// such as fugacity and phase finder algorithms.
/// </summary>
public abstract class EquationOfState
{
	/// <inheritdoc cref="Constants.R"/>
	public const double R = Constants.R;
	public const double precisionLimit = Constants.PrecisionLimit;

	/// <summary>
	/// Toggles check and use of high-temperature data.
	/// </summary>
	public bool UseHighTempData = false;

	/// <summary>
	/// The reference state to which all real and ideal gas properties are measured.
	/// </summary>
	public readonly (Temperature refT, Pressure refP) ReferenceState;

	public readonly Chemical Species;
	public (double molarMass, Temperature critT, Pressure critP, double acentricFactor, Temperature boilT) speciesData;
	public (double[] vals, double[] lims) speciesCpData;
	public (double[] vals, double[] lims) speciesHighTempCpData;

	protected EquationOfState(Chemical species, (Temperature T, Pressure P) referenceState)
	{
		ReferenceState = referenceState;
		Species = species;
		speciesData = Constants.ChemicalData[species];
		speciesCpData = Constants.IdealGasCpConstants[species];
		//TODO: when high-temp calculations are fleshed out, reactivate this line.
		//speciesHighTempCpData = Constants.HighTempIdealGasCpConstants[species];
	}

    protected EquationOfState(Chemical species)
    {
        ReferenceState = (298.15, 100e3);
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
    public abstract Pressure Pressure(Temperature T, Volume VMol);

    /// <summary>
    /// Returns the compressibility factor in the given state,
	/// defined by temperature, pressure, and molar volume.
    /// </summary>
    /// <param name="T">temperature, in [K]</param>
	/// <param name="P">pressure, in [Pa]</param>
    /// <param name="VMol">molar volume, in [m³/mol]</param>
    /// <returns>compressibility factor = PV/RT, unitless</returns>
    public double CompressibilityFactor(Temperature T, Pressure P, Volume VMol)
	{
		return P * VMol / (R * T);
	}

    /// <summary>
    /// Returns the fugacity coefficient of the system in the given state,
    /// defined by temperature, pressure, and molar volume.
    /// </summary>
    /// <param name="T">temperature, in [K]</param>
    /// <param name="P">pressure, in [Pa]</param>
    /// <param name="VMol">molar volume, in [m³/mol]</param>
    /// <returns>fugacity coefficient φ, unitless</returns>
    public abstract double FugacityCoeff(Temperature T, Pressure P, Volume VMol);

    /// <summary>
    /// Calculates the fugacity of the system in the given state.
    /// </summary>
    /// <param name="T">temperature, in [K]</param>
    /// <param name="P">pressure, in [Pa]</param>
    /// <param name="VMol">molar volume, in [m³/mol]</param>
    /// <returns>fugacity, unitless. f = φP</returns>
    public double Fugacity(Temperature T, Pressure P, Volume VMol) { return FugacityCoeff(T, P, VMol) * P; }

    #region State Variables - Enthalpy

    /// <summary>
    /// Calculates the departure enthalpy, which is the difference between the real enthalpy and
    /// the ideal enthalpy at a given state defined by temperature, pressure, and molar volume.
    /// </summary>
    /// <param name="T">temperature, in [K]</param>
    /// <param name="P">pressure, in [Pa]</param>
    /// <param name="VMol">molar volume, in [m³/mol]</param>
    /// <returns>Molar Enthalpy, departure</returns>
	/// <remarks>Departure is dependent on the equation of state used, so this must be defined
	/// for each EoS.</remarks>
    public abstract Enthalpy DepartureEnthalpy(Temperature T, Pressure P, Volume VMol);

    /// <summary>
    /// Calculates the enthalpy change between two states assuming ideal gas behavior.
    /// Ideal enthalpy change is a pure function of temperature.
    /// </summary>
    /// <param name="T1">Initial temperature</param>
    /// <param name="T2">Final temperature</param>
    /// <returns>Molar Enthalpy, change</returns>
    /// <exception cref="NotImplementedException">
    /// Use of high-temperature Cp data is not currently supported.</exception>
    public Enthalpy IdealMolarEnthalpyChange(Temperature T1, Temperature T2)
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
		return new Enthalpy(value, ThermoVarRelations.Change);
	}
	
	/// <summary>
	/// Calculates the enthalpy change between two states using departure functions and convenient paths.
	/// </summary>
	/// <returns>Molar Enthalpy, change</returns>
	public Enthalpy MolarEnthalpyChange
		(Temperature T1, Pressure P1, Volume VMol1, Temperature T2, Pressure P2, Volume VMol2)
	{
		var pathA = DepartureEnthalpy(T1, P1, VMol1);
		var pathB = IdealMolarEnthalpyChange(T1, T2);
		var pathC = DepartureEnthalpy(T2, P2, VMol2);
		var totalPath = -pathA + pathB + pathC;
		return new Enthalpy(totalPath, ThermoVarRelations.Change);
	}

    /// <summary>
    /// Calculates the enthalpy at a given state with respect to the reference state. 
    /// </summary>
    /// <param name="T">temperature, in [K]</param>
    /// <param name="P">pressure, in [Pa]</param>
    /// <param name="VMol">molar volume, in [m³/mol]</param>
    /// <returns>Molar Enthalpy, real</returns>
    public Enthalpy ReferenceMolarEnthalpy(Temperature T, Pressure P, Volume VMol)
	{
		var pathA = IdealMolarEnthalpyChange(ReferenceState.refT, T);
		var pathB = DepartureEnthalpy(T, P, VMol);
		var totalPath = pathA + pathB;
		return new Enthalpy(totalPath, ThermoVarRelations.RealMolar);
	}

    #endregion

    #region State Variables - Entropy

    /// <summary>
    /// Calculates the departure entropy, which is the difference between the real entropy and
    /// the ideal entropy at a given state defined by temperature, pressure, and molar volume.
    /// </summary>
    /// <param name="T">temperature, in [K]</param>
    /// <param name="P">pressure, in [Pa]</param>
    /// <param name="VMol">molar volume, in [m³/mol]</param>
    /// <returns>Molar Entropy, departure</returns>
	/// Departure is dependent on the equation of state used, so this must be defined for each EoS.</remarks>
    public abstract Entropy DepartureEntropy(Temperature T, Pressure P, Volume VMol);

    /// <summary>
    /// Calculates the entropy change between two states assuming ideal gas behavior.
    /// Ideal entropy change is a pure function of temperature.
    /// </summary>
    /// <param name="T1">Initial temperature</param>
    /// <param name="T2">Final temperature</param>
    /// <returns>Molar entropy, change</returns>
    /// <exception cref="NotImplementedException">
    /// Use of high-temperature Cp data is not currently supported.</exception>
    public Entropy IdealMolarEntropyChange(Temperature T1, Pressure P1, Temperature T2, Pressure P2)
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
		return new Entropy(value, ThermoVarRelations.Change);
	}

    /// <summary>
    /// Calculates the entropy change between two states using departure functions and convenient paths.
    /// </summary>
    /// <returns>Molar Entropy, change</returns>
    public Entropy MolarEntropyChange
		(Temperature T1, Pressure P1, Volume VMol1, Temperature T2, Pressure P2, Volume VMol2)
	{
		var pathA = DepartureEntropy(T1, P1, VMol1);
		var pathB = IdealMolarEntropyChange(T1, P1, T2, P2);
		var pathC = DepartureEntropy(T2, P2, VMol2);
		var totalPath = -pathA + pathB + pathC;
		return new Entropy(totalPath, ThermoVarRelations.Change);
	}

    /// <summary>
    /// Calculates the entropy at a given state with respect to the reference state.
    /// </summary>
    /// <param name="T">temperature, in [K]</param>
    /// <param name="P">pressure, in [Pa]</param>
    /// <param name="VMol">molar volume, in [m³/mol]</param>
    /// <returns>Molar entropy, real</returns>
    public Entropy ReferenceMolarEntropy(Temperature T, Pressure P, Volume VMol)
	{
		var pathB = IdealMolarEntropyChange(273.15+25, 100e3, T, P);
		var pathC = DepartureEntropy(T, P, VMol);
		var totalPath = pathB + pathC;
		return new Entropy(totalPath);
	}

    #endregion

    #region State Variables - other

    /// <summary>
    /// Calculates the internal energy at a given state with respect to the reference state.
    /// </summary>
    /// <param name="T">temperature, in [K]</param>
    /// <param name="P">pressure, in [Pa]</param>
    /// <param name="VMol">molar volume, in [m³/mol]</param>
    /// <returns>Molar Internal Energy, real. U = H-PV</returns>
    public InternalEnergy ReferenceMolarInternalEnergy(Temperature T, Pressure P, Volume VMol)
	{
		return new InternalEnergy(ReferenceMolarEnthalpy(T, P, VMol) - P * VMol);
	}

    /// <summary>
    /// Calculates the Gibbs free energy at a given state with respect to the reference state.
    /// </summary>
    /// <param name="T">temperature, in [K]</param>
    /// <param name="P">pressure, in [Pa]</param>
    /// <param name="VMol">molar volume, in [m³/mol]</param>
    /// <returns>Molar Gibbs Energy, real. G = H-TS</returns>
    public GibbsEnergy ReferenceMolarGibbsEnergy(Temperature T, Pressure P, Volume VMol)
	{
		var H = ReferenceMolarEnthalpy(T, P, VMol);
		var S = ReferenceMolarEntropy(T, P, VMol);
		return new GibbsEnergy(H - T * S);
	}

    /// <summary>
    /// Calculates the Helmholtz free energy at a given state with respect to the reference state.
    /// </summary>
    /// <param name="T">temperature, in [K]</param>
    /// <param name="P">pressure, in [Pa]</param>
    /// <param name="VMol">molar volume, in [m³/mol]</param>
    /// <returns>Molar Helmholtz Energy, real. A = U-TS</returns>
    public HelmholtzEnergy ReferenceMolarHelmholtzEnergy(Temperature T, Pressure P, Volume VMol)
	{
		var U = ReferenceMolarInternalEnergy(T, P, VMol);
		var S = ReferenceMolarEntropy(T, P, VMol);
		return new HelmholtzEnergy(U - T * S);
	}

	#endregion

	/// <summary>
	/// Uses the equation of state to find the phases present in a system given a temperature and pressure.
	/// Checks for valid equilibrium using the fugacity coefficient.
	/// </summary>
	/// <param name="T">temperature, in [K]</param>
	/// <param name="P">pressure, in [Pa]</param>
	/// <param name="ignoreEquilibrium">skips fugacity comparison which determines phase equilibrium state</param>
	/// <returns>
	/// Molar volume at each the liquid and vapor phases, in [m³/mol].
	/// Returns 0 if the phase is not present.</returns>
	public abstract (Volume L, Volume V) PhaseFinder(Temperature T, Pressure P, bool ignoreEquilibrium = false);

	public (double Z, InternalEnergy U, Enthalpy H, Entropy S, GibbsEnergy G, HelmholtzEnergy A, double f)
		GetAllStateVariables(Temperature T, Pressure P, Volume VMol)
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