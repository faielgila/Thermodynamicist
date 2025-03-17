namespace Core.VariableTypes;

public enum ThermoVarRelations
{
	/// <summary>
	/// Apply to thermodynamic variables for which defining a "relation" is not relevant
	/// </summary>
	Undefined,
	
	RealMolar, IGMolar,
	PartialMolar,
	Departure, Change,
	Mixing, Excess,
	OfVaporization, OfSublimation, OfFusion,
	OfFormation,
	OfReaction,
	
	Temperature, SaturationTemperature,
	
	Pressure, VaporPressure, PartialPressure,
	
	/// <summary>
	/// Apply to <see cref="MoleFraction"/> when fraction represents a mol% of a component in a single phase
	/// </summary>
	ComponentFraction,
	/// <summary>
	/// Apply to <see cref="MoleFraction"/> when fraction represents a mol% of a phase in a system
	/// </summary>
	PhaseFraction
}