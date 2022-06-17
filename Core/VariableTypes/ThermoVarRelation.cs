namespace Core.VariableTypes;

public enum ThermoVarRelations
{
	/// <summary>
	/// Apply to thermodynamic variables for which defining a "relation" doesn't mean anything
	/// </summary>
	Undefined,
	
	RealMolar, IGMolar, 
	PartialMolar,
	Departure, Change,
	Mixing, Excess,
	OfVaporization, OfSublimation, OfFusion,
	OfFormation,
	
	Temperature,
	
	Pressure, VaporPressure, PartialPressure,
	
	/// <summary>
	/// Apply to <see cref="MoleFraction"/> when fraction represents a mol% of a component in a system or phase
	/// </summary>
	ComponentFraction,
	/// <summary>
	/// Apply to <see cref="MoleFraction"/> when fraction represents a mol% of a phase in a system or about a phase
	/// </summary>
	PhaseFraction
}