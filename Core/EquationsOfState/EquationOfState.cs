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
	public readonly (Temperature refT, Pressure refP) ReferenceState = (298.15, 100e3);

	public readonly Chemical Species;
	public (double molarMass, Temperature critT, Pressure critP, double acentricFactor, Temperature boilT) speciesData;
	public (double[] vals, double[] lims) speciesCpData;
	public (double[] vals, double[] lims) speciesHighTempCpData;

	public List<string> ModeledPhases;

	protected EquationOfState(Chemical species, (Temperature T, Pressure P) referenceState, List<string> modeledPhases)
	{
		ReferenceState = referenceState;
		Species = species;
		speciesData = Constants.ChemicalData[species];
		speciesCpData = Data.HeatCapacityParameters.IdealGasCpConstants[species];
		ModeledPhases = modeledPhases;
		//TODO: when high-temp calculations are fleshed out, reactivate this line.
		//speciesHighTempCpData = Constants.HighTempIdealGasCpConstants[species];
	}

	protected EquationOfState(Chemical species, List<string> modeledPhases)
	{
		Species = species;
		speciesData = Constants.ChemicalData[species];
		speciesCpData = Data.HeatCapacityParameters.IdealGasCpConstants[species];
		ModeledPhases = modeledPhases;
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

	#region Pressure partial derivatives
	/// <summary>
	/// Analytically calculates the derivative of pressure with respect to molar volume, holding temperature constant.
	/// </summary>
	/// <param name="T">temperature, in [K]</param>
	/// <param name="VMol">molar volume, in [m³/mol]</param>
	/// <returns>change in P relative to change in V, (∂P/∂V)|T, in [J/mol]</returns>
	public abstract double PVPartialDerivative(Temperature T, Volume VMol);
	#endregion

	/// <summary>
	/// Calculates the molar volume of the species at the critical point using the given equation of state.
	/// </summary>
	/// <returns>critical molar volume, in [m³/mol]</returns>
	public abstract Volume CriticalMolarVolume();

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

	// TODO: Implement generic algorithm for the thermal expansion coefficient (α)
	// Update enthalpy/entropy changes to include pressure term
	//public abstract double ThermalExpansionCoeff(Temperature T, Pressure P, Volume VMol);

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
	/// <returns>molar enthalpy (type departure), in [J/mol]</returns>
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

		// Evaluates the integral of ideal heat capacity with respect to temperature from T1 to T2.
		// ΔH* = ∫ Cp* dT
		double value = 0;
		var deltaT = new double[c.Length];
		for (int i = 0; i < c.Length; i++)
		{
			deltaT[i] = Math.Pow(T2, i + 1) - Math.Pow(T1, i + 1);
			value += c[i] * deltaT[i] / (i + 1);
		}
		return new Enthalpy(value, ThermoVarRelations.Change);
	}
	
	/// <summary>
	/// Calculates the enthalpy change between two states using departure functions and convenient paths.
	/// </summary>
	/// <returns>molar enthalpy (type change), in [J/mol]</returns>
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
	/// <returns>molar enthalpy (type real), in [J/mol]</returns>
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
	public abstract Entropy DepartureEntropy(Temperature T, Pressure P, Volume VMol);

	/// <summary>
	/// Calculates the entropy change between two states assuming ideal gas behavior.
	/// Ideal entropy change is a pure function of temperature.
	/// </summary>
	/// <param name="T1">Initial temperature</param>
	/// <param name="T2">Final temperature</param>
	/// <returns>Molar entropy, change</returns>
	/// <exception cref="NotImplementedException"> Use of high-temperature Cp data is not currently supported.</exception>
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

	#region Phase equilibrium

	/// <summary>
	/// Uses the equation of state to find the phases present in a system given a temperature and pressure.
	/// Checks for valid equilibrium using the fugacity coefficient.
	/// </summary>
	/// <param name="T">temperature, in [K]</param>
	/// <param name="P">pressure, in [Pa]</param>
	/// <param name="ignoreEquilibrium">skips fugacity comparison which determines phase equilibrium state</param>
	/// <returns> Dictionary: phase name stored as the keystring, and molar volume for each phase [m³/mol].</returns>
	public abstract Dictionary<string, Volume> PhaseFinder(Temperature T, Pressure P, bool ignoreEquilibrium = false);

	/// <summary>
	/// Creates a list of the phases present at equilibrium by comparing fugacities.
	/// </summary>
	/// <param name="T">temperature, measured in [K]</param>
	/// <param name="P">pressure, measured in [Pa]</param>
	/// <returns> Dictionary: phase name stored as the keystring, and molar volume for each phase [m³/mol].</returns>
	public Dictionary<string, Volume> EquilibriumPhases(Temperature T, Pressure P)
	{
		var phases = PhaseFinder(T, P, ignoreEquilibrium: true);
		var phasesEquil = new Dictionary<string, Volume>();

		// First, calculate the fugacity coefficient for all phases.
		var fugacityCoeffs = new Dictionary<string, double>();
		foreach (var phaseKey in phases.Keys)
		{
			fugacityCoeffs.Add(phaseKey, FugacityCoeff(T, P, phases[phaseKey]));
		}

		var minFugacityCoeff = fugacityCoeffs.Values.Min();
		foreach (var currentKey in phases.Keys)
		{
			// Calculate the fugacity for the current phase.
			var currentFugacityCoeff = FugacityCoeff(T, P, phases[currentKey]);

			// If the fugacity does not exist (i.e., imaginary and represented by NaN), it's definitely not in equilibrium.
			if (double.IsNaN(currentFugacityCoeff)) { continue; }
			// If the fugacity is not close to the minimum fugacity phase, it's probably not in equilibrium.
			if (Math.Abs(currentFugacityCoeff - minFugacityCoeff) >= 0.01) { continue; }

			// Otherwise, it is rougly equal and will be in equilibrium.
			phasesEquil.Add(currentKey, phases[currentKey]);
		}
		return phasesEquil;
	}

	/// <summary>
	/// Creates a list of all phases present at a constant temperature but varying pressure.
	/// Does not return molar volumes, since those would be dependent on pressure.
	/// </summary>
	/// <param name="T">temperature, in [K]</param>
	/// <param name="dP">step size for pressure, in [Pa]</param>
	/// <returns>List: phase name stored as a string</returns>
	// TODO : Implement a smarter algorithm for searching that doesn't require fixed step sizes
	// that may accidentally skip small phase regions. Maybe search the Gibbs free energy curve
	// directly, monitoring the derivatives of each phase to decide how large the step size should be?
	public List<string> EquilibriumPhases(Temperature T, double dP = 10)
	{
		var critP = speciesData.critP;
		var pressures = new LinearEnumerable(critP * 1e-4, critP - 10, dP);

		var phases = new List<string>();
		foreach(Pressure P in pressures)
		{
			var phaseKeys = EquilibriumPhases(T, P).Keys;
			phaseKeys.ToList();
			phases.AddRange(phaseKeys);
		}

		return phases;
	}

	/// <summary>
	/// Creates a list of all phases present at a constant temperature but varying temperature.
	/// Does not return molar volumes, since those would be dependent on temperature.
	/// </summary>
	/// <param name="T">temperature, in [K]</param>
	/// <param name="dP">step size for pressure, in [Pa]</param>
	/// <returns>List: phase name stored as a string</returns>
	// TODO : Implement a smarter algorithm for searching that doesn't require fixed step sizes
	// that may accidentally skip small phase regions. Maybe search the Gibbs free energy curve
	// directly, monitoring the derivatives of each phase to decide how large the step size should be?
	public List<string> EquilibriumPhases(Pressure P, double dT = 0.5)
	{
		var critT = speciesData.critT;
		var temps = new LinearEnumerable(critT / 100, critT - 10, dT);

		var phases = new List<string>();
		foreach (Temperature T in temps)
		{
			var phaseKeys = EquilibriumPhases(T, P).Keys;
			phaseKeys.ToList();
			phases.AddRange(phaseKeys);
		}

		return phases;
	}

	/// <summary>
	/// Calculates the saturation pressure for liquid-vapor equilibrium, i.e. the vapor pressure.
	/// If the temperature is above the critical temperature, the vapor pressure will be returned as NaN.
	/// </summary>
	/// <param name="T">temperature, in [K]</param>
	/// <returns>If it exists, vapor pressure, in [Pa]; If does not exist, NaN</returns>
	public virtual Pressure VaporPressure(Temperature T)
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

	/// <summary>
	/// Calculates the saturation temperature for liquid-vapor equilibrium, i.e. the boiling temperature.
	/// If the pressure is above the critical pressure, the boiling temperature will be returned as NaN.
	/// </summary>
	/// <param name="P">pressure, in [Pa]</param>
	/// <returns>If it exists, boiling temperature, in [K]; If is does not exist, NaN</returns>
	public abstract Temperature BoilingTemperature(Pressure P);

	/// <summary>
	/// Calculates the enthalpy change between two phases.
	/// If either phase is not present, the enthalpy will be returned as NaN.
	/// </summary>
	/// <param name="T">temperature, in [K]</param>
	/// <param name="P">pressure, in [Pa]</param>
	/// <returns>If it exists, phase change enthalpy, in [J/mol]; If is does not exist, NaN</returns>
	public abstract Enthalpy PhaseChangeEnthalpy(Temperature T, Pressure P, string phaseFrom, string phaseTo);

	#endregion

	/// <summary>
	/// Gets every state variable for a pure component at the specified temperature, pressure, and molar volume.
	/// </summary>
	/// <param name="T">temperature, in [K]</param>
	/// <param name="P">pressure, in [Pa]</param>
	/// <param name="VMol">molar volume of phase, in [m³/mol]</param>
	/// <returns>List of variables: compressibility factor, molar internal energy,
	/// molar enthalpy, molar entropy, molar Gibbs energy, molar Helmholtz energy, and fugacity coefficient.</returns>
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