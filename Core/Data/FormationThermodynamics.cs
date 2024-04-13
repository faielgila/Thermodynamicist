using Core.VariableTypes;

namespace Core.Data;
class FormationThermodynamics
{
	/// <summary>
	/// Stores standard formation enthalpies in J/mol.
	/// </summary>=
	// From NIST Webbook
	public static readonly Dictionary<Chemical, (Enthalpy enthalpy, string phase)> StandardFormationEnthalpy = new()
	{
		[Chemical.Ammonia]			= (new Enthalpy(-45.94e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.Benzene]			= (new Enthalpy( 49.04e3,	ThermoVarRelations.OfFusion), "liquid"),
		[Chemical.NButane]			= (new Enthalpy(-125.6e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.Isobutane]		= (new Enthalpy(-134.2e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.CarbonDioxide]	= (new Enthalpy(-393.51e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.CarbonMonoxide]	= (new Enthalpy(-110.53e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.Ethane]			= (new Enthalpy(-83.8e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.Hydrogen]			= (new Enthalpy( 0,			ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.HydrogenFluoride]	= (new Enthalpy(-273.3e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.HydrogenChloride]	= (new Enthalpy(-92.31e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.HydrogenSulfide]	= (new Enthalpy(-20.6e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.Methane]			= (new Enthalpy(-74.87e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.Nitrogen]			= (new Enthalpy( 191.609e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.Oxygen]			= (new Enthalpy( 0,			ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.NPentane]			= (new Enthalpy(-173.5e3,	ThermoVarRelations.OfFusion), "liquid"),
		[Chemical.Isopentane]		= (new Enthalpy(-178.2e3,	ThermoVarRelations.OfFusion), "liquid"),
		[Chemical.Propane]			= (new Enthalpy(-104.7e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.R12]				= (new Enthalpy(-491.62e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.R134a]			= (new Enthalpy(-877.8e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.SulfurDioxide]	= (new Enthalpy(-296.81e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.Toluene]			= (new Enthalpy( 12.1e3,	ThermoVarRelations.OfFusion), "liquid"),
		[Chemical.Water]			= (new Enthalpy(-285.83e3,	ThermoVarRelations.OfFusion), "liquid")
	};

	/// <summary>
	/// Stores standard formation entropies in J/mol/K.
	/// </summary>=
	// From NIST Webbook
	public static readonly Dictionary<Chemical, (Enthalpy enthalpy, string phase)> StandardFormationEntropy = new()
	{
		[Chemical.Ammonia]			= (new Enthalpy(-45.94e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.Benzene]			= (new Enthalpy( 49.04e3,	ThermoVarRelations.OfFusion), "liquid"),
		[Chemical.NButane]			= (new Enthalpy(-125.6e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.Isobutane]		= (new Enthalpy(-134.2e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.CarbonDioxide]	= (new Enthalpy(-393.51e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.CarbonMonoxide]	= (new Enthalpy(-110.53e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.Ethane]			= (new Enthalpy(-83.8e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.Hydrogen]			= (new Enthalpy( 0,			ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.HydrogenFluoride]	= (new Enthalpy(-273.3e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.HydrogenChloride] = (new Enthalpy(-92.31e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.HydrogenSulfide]	= (new Enthalpy(-20.6e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.Methane]			= (new Enthalpy(-74.87e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.Nitrogen]			= (new Enthalpy(191.609e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.Oxygen]			= (new Enthalpy( 0,			ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.NPentane]			= (new Enthalpy(-173.5e3,	ThermoVarRelations.OfFusion), "liquid"),
		[Chemical.Isopentane]		= (new Enthalpy(-178.2e3,	ThermoVarRelations.OfFusion), "liquid"),
		[Chemical.Propane]			= (new Enthalpy(-104.7e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.R12]				= (new Enthalpy(-491.62e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.R134a]			= (new Enthalpy(-877.8e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.SulfurDioxide]	= (new Enthalpy(-296.81e3,	ThermoVarRelations.OfFusion), "vapor"),
		[Chemical.Toluene]			= (new Enthalpy( 12.1e3,	ThermoVarRelations.OfFusion), "liquid"),
		[Chemical.Water]			= (new Enthalpy(-285.83e3,	ThermoVarRelations.OfFusion), "liquid")
	};
}
