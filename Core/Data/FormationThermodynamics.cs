using Core.VariableTypes;

namespace Core.Data;
class FormationThermodynamics
{
	public static readonly Dictionary<Chemical, string> StandardFormationPhase = new()
	{
		[Chemical.Acetone]			= "liquid",
		[Chemical.Ammonia]			= "vapor",
		[Chemical.Benzene]			= "liquid",
		[Chemical.NButane]			= "vapor",
		[Chemical.Isobutane]		= "vapor",
		[Chemical.CarbonDioxide]	= "vapor",
		[Chemical.CarbonMonoxide]	= "vapor",
		[Chemical.Chlorine]			= "vapor",
		[Chemical.Chlorobenzene]	= "liquid",
		[Chemical.Ethane]			= "vapor",
		[Chemical.Hydrogen]			= "vapor",
		[Chemical.HydrogenFluoride]	= "vapor",
		[Chemical.HydrogenChloride]	= "vapor",
		[Chemical.HydrogenSulfide]	= "vapor",
		[Chemical.Methane]			= "vapor",
		[Chemical.Nitrogen]			= "vapor",
		[Chemical.Oxygen]			= "vapor",
		[Chemical.NPentane]			= "liquid",
		[Chemical.Isopentane]		= "liquid",
		[Chemical.Propane]			= "vapor",
		[Chemical.NPropanol]		= "liquid",
		[Chemical.R12]				= "vapor",
		[Chemical.R134a]			= "vapor",
		[Chemical.SulfurDioxide]	= "vapor",
		[Chemical.Toluene]			= "liquid",
		[Chemical.Water]			= "liquid"
	};

	/// <summary>
	/// Stores standard formation enthalpies in J/mol.
	/// </summary>=
	// Most data taken from NIST Webbook
	// R12, NPropanol from Yaw's Handbook of Thermodynamic Properties for Hydrocarbons and Chemicals (ISBN 978-1-60119-797-9)
	// Chlorbenzene from Knovel Critical Tables (2nd Ed, 2008)
	public static readonly Dictionary<Chemical, Enthalpy> StandardFormationEnthalpy = new()
	{
		[Chemical.Acetone]			= new Enthalpy(-217.1e3,	ThermoVarRelations.OfFormation),
		[Chemical.Ammonia]			= new Enthalpy(-45.94e3,	ThermoVarRelations.OfFormation),
		[Chemical.Benzene]			= new Enthalpy( 49.04e3,	ThermoVarRelations.OfFormation),
		[Chemical.NButane]			= new Enthalpy(-125.6e3,	ThermoVarRelations.OfFormation),
		[Chemical.Isobutane]		= new Enthalpy(-134.2e3,	ThermoVarRelations.OfFormation),
		[Chemical.CarbonDioxide]	= new Enthalpy(-393.51e3,	ThermoVarRelations.OfFormation),
		[Chemical.CarbonMonoxide]	= new Enthalpy(-110.53e3,	ThermoVarRelations.OfFormation),
		[Chemical.Chlorine]			= new Enthalpy( 0,			ThermoVarRelations.OfFormation),
		[Chemical.Chlorobenzene]	= new Enthalpy( 51.1e3,		ThermoVarRelations.OfFormation),
		[Chemical.Ethane]			= new Enthalpy(-83.8e3,		ThermoVarRelations.OfFormation),
		[Chemical.Hydrogen]			= new Enthalpy( 0,			ThermoVarRelations.OfFormation),
		[Chemical.HydrogenFluoride]	= new Enthalpy(-273.3e3,	ThermoVarRelations.OfFormation),
		[Chemical.HydrogenChloride]	= new Enthalpy(-92.31e3,	ThermoVarRelations.OfFormation),
		[Chemical.HydrogenSulfide]	= new Enthalpy(-20.6e3,		ThermoVarRelations.OfFormation),
		[Chemical.Methane]			= new Enthalpy(-74.87e3,	ThermoVarRelations.OfFormation),
		[Chemical.Nitrogen]			= new Enthalpy( 0,			ThermoVarRelations.OfFormation),
		[Chemical.Oxygen]			= new Enthalpy( 0,			ThermoVarRelations.OfFormation),
		[Chemical.NPentane]			= new Enthalpy(-173.5e3,	ThermoVarRelations.OfFormation),
		[Chemical.Isopentane]		= new Enthalpy(-178.2e3,	ThermoVarRelations.OfFormation),
		[Chemical.Propane]			= new Enthalpy(-104.7e3,	ThermoVarRelations.OfFormation),
		[Chemical.NPropanol]		= new Enthalpy(-255.204e3,	ThermoVarRelations.OfFormation),
		[Chemical.R12]				= new Enthalpy(-491.62e3,	ThermoVarRelations.OfFormation),
		[Chemical.R134a]			= new Enthalpy(-877.8e3,	ThermoVarRelations.OfFormation),
		[Chemical.SulfurDioxide]	= new Enthalpy(-296.81e3,	ThermoVarRelations.OfFormation),
		[Chemical.Toluene]			= new Enthalpy( 12.1e3,		ThermoVarRelations.OfFormation),
		[Chemical.Water]			= new Enthalpy(-285.83e3,	ThermoVarRelations.OfFormation)
	};

	/// <summary>
	/// Stores standard formation Gibbs free energies in J/mol.
	/// </summary>=
	// Most data from Sandler's Appendix A.IV
	// {R12, R134a} from Yaw's Handbook of Thermodynamic Properties for Hydrocarbons and Chemicals (ISBN 978-1-60119-797-9)
	// Chlorbenzene from Knovel Critical Tables (2nd Ed, 2008)
	public static readonly Dictionary<Chemical, GibbsEnergy> StandardFormationGibbsEnergy = new()
	{
		[Chemical.Acetone]			= new GibbsEnergy(-152.6e3, ThermoVarRelations.OfFormation),
		[Chemical.Ammonia]			= new GibbsEnergy(-16.5e3,	ThermoVarRelations.OfFormation),
		[Chemical.Benzene]			= new GibbsEnergy( 124.5e3,	ThermoVarRelations.OfFormation),
		[Chemical.NButane]			= new GibbsEnergy(-16.6e3,	ThermoVarRelations.OfFormation),
		[Chemical.Isobutane]		= new GibbsEnergy(-16.6e3,	ThermoVarRelations.OfFormation),
		[Chemical.CarbonDioxide]	= new GibbsEnergy(-394.4e3,	ThermoVarRelations.OfFormation),
		[Chemical.CarbonMonoxide]	= new GibbsEnergy(-137.2e3,	ThermoVarRelations.OfFormation),
		[Chemical.Chlorine]			= new GibbsEnergy( 0,		ThermoVarRelations.OfFormation),
		[Chemical.Chlorobenzene]	= new GibbsEnergy( 98.31e3,	ThermoVarRelations.OfFormation),
		[Chemical.Ethane]			= new GibbsEnergy(-31.9e3,	ThermoVarRelations.OfFormation),
		[Chemical.Hydrogen]			= new GibbsEnergy( 0,		ThermoVarRelations.OfFormation),
		[Chemical.HydrogenFluoride]	= new GibbsEnergy(-273.2e3,	ThermoVarRelations.OfFormation),
		[Chemical.HydrogenChloride] = new GibbsEnergy(-95.3e3,	ThermoVarRelations.OfFormation),
		[Chemical.HydrogenSulfide]	= new GibbsEnergy(-33.6e3,	ThermoVarRelations.OfFormation),
		[Chemical.Methane]			= new GibbsEnergy(-50.5e3,	ThermoVarRelations.OfFormation),
		[Chemical.Nitrogen]			= new GibbsEnergy( 0,		ThermoVarRelations.OfFormation),
		[Chemical.Oxygen]			= new GibbsEnergy( 0,		ThermoVarRelations.OfFormation),
		[Chemical.NPentane]			= new GibbsEnergy(-8.7e3,	ThermoVarRelations.OfFormation),
		[Chemical.Isopentane]		= new GibbsEnergy(-8.7e3,	ThermoVarRelations.OfFormation),
		[Chemical.Propane]			= new GibbsEnergy(-24.3e3,	ThermoVarRelations.OfFormation),
		[Chemical.NPropanol]		= new GibbsEnergy(-159.8e3,	ThermoVarRelations.OfFormation),
		[Chemical.R12]				= new GibbsEnergy(-451.7e3,	ThermoVarRelations.OfFormation),
		[Chemical.R134a]			= new GibbsEnergy(-838.4e3,	ThermoVarRelations.OfFormation),
		[Chemical.SulfurDioxide]	= new GibbsEnergy(-300.2e3,	ThermoVarRelations.OfFormation),
		[Chemical.Toluene]			= new GibbsEnergy( 113.6e3,	ThermoVarRelations.OfFormation),
		[Chemical.Water]			= new GibbsEnergy(-237.1e3,	ThermoVarRelations.OfFormation)
	};
	
	public static Entropy StandardFormationEntropy(Chemical chemical)
	{
		var T = Constants.StandardConditions.T;
		Enthalpy H = StandardFormationEnthalpy[chemical];
		GibbsEnergy G = StandardFormationGibbsEnergy[chemical];
		return new Entropy((H - G) / T, ThermoVarRelations.OfFormation);
	}
}
