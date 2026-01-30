using Core.VariableTypes;
using System.Linq;

namespace Core.Multicomponent.ActivityModels;

public class UNIFACActivityModel(List<MixtureSpecies> _speciesList) : ActivityModel(_speciesList)
{

	public override ActivityModel Copy()
	{
		var speciesListCopy = new List<MixtureSpecies>();
		foreach (var item in speciesList) speciesListCopy.Add(item.Copy());
		return new UNIFACActivityModel(speciesListCopy);
	}

	#region Parameters

	private enum FunctionalSubgroup
	{
		CarbonylCO, // For temporary testing

		Alkyl_1, // CH3, primary alkyl carbon (methyl group)
		Alkyl_2, // CH2, secondary alkyl carbon
		Alkyl_3, // CH, tertiary alkyl carbon
		Alkyl_4, // C, quaternary alkyl carbon
		TerminalAlkene, // CH2=CH, terminal alkene carbons
		InternalAlkene, // CH=CH, internal alkene carbons
		BranchAlkene_2, // CH2=C, disubstituted alkene carbons
		BranchAlkene_3, // CH=C, trisubstituted alkene carbons
		UnsubAromaticC, // aCH, unsubstituted aromatic carbon
		SubAromaticC, // aC, substituted aromatic carbon
		ArylMethane, // aCCH3, aromatic methyl carbon
		ArylLinearAlkane, // aCCH2, aromatic linear alkyl carbon
		ArylBranchAlkane, // aCCH, aromatic branched alkyl carbon
		Alcohol, // OH, alcohol
		Methanol, // CH3OH, methanol
		Water, // H2O, water
		ArylAlcohol, // aCOH, aromatic alcohol carbon
		MethylKetone, // CH3C=O, alk-2-one
		AlkylKetone, // CH2C=O, alk-n-one
		Aldehyde, // CH=O, aldehyde
		MethylEster, // CH3COO, methyl ester
		AlkylEster, // CH2COO, alkyl ester
		AlkylFormate, // HCOO, formate
		MethylEther, // CH3O, methyl ether
		LinearAlkylEther, // CH2O, linear alkyl ether
		BranchAlkylEther, // CHO, branched alkyl ether
		Tetrahydrofuran, // THF, tetrahydrofuran
		Methylamine, // CH3NH2, methylamine
		LinearAlkylPriAmine, // CH2NH2
		BranchAlkylPriAmine, // CHNH2
		MethylSecAmine, // CH3NH, methyl secondary amine (alkyl methylamine)
		LinearAlkylSecAmine, // CH2NH, linear alkyl secondary amine
		BranchAlkylSecAmine, // CHNH, branched alkyl secondary amine
		MethylTerAmine, // CH3N, methyl tertiary amine
		LinearAlkylTerAmine, // CH2N, linear alkyl tertiary amine
		ArylPriAmine, // aCNH2, aromatic primary amine carbon
		Pyridine, // C5H5N, pyridine
		Pyridine_2, // C5H4N, 2-subtitution pyridine
		Pyridine_2_3, // C5H3, 2,3-subtitution pyridine
		Acetonitrile, // CH3CN, acetonitrile (methyl cyanide)
		LinearAlkylCyanide, // CH2CN, linear alkyl cyanide
		CarboxylicAcid, // COOH, carboxylic acid
		FormicAcid, // HCOOH, formic acid
		ChloroAlkyl_1, // CH2Cl, chloroalkane primary carbon
		ChloroAlkyl_2, // CHCl, chloroalkane secondary carbon
		ChloroAlkyl_3, // CCl, chloroalkane tertiary carbon
		Dichloromethane, // CH2Cl2, dichloromethane (DCM)
		DichloroAlkyl_1, // CHCl2, dichloroalkane primary carbon
		DichloroAlkyl_2, // CCl2, dichloroalkane secondary carbon
		Trichloromethane, // CHCl3, trichloromethane
		TrichloroAlkyl, // CCl3, trichloroalkane carbon
		Tetrachloromethane, // CCl4, tetrachloromethane (carbon tetrachloride)
		ArylChloride, // aCCl, aromatic chloride carbon
		Nitromethane, // CH3NO2, nitromethane
		NitroAlkane_1, // CH2NO2, nitroalkane primary carbon
		NitroAlkane_2, // CHNO2, nitroalkane secondary carbon
		NitroArene, // aCNO2, nitroaromatic carbon
		CarbonDisulfide, // CS2, carbon disulfide
		Methylthiol, // CH3SH, methylthiol (methyl mercaptan)
		PriAlkylThiol, // CH2SH, primary alkyl thiol carbon
		Furfural, // furfural
		EthyleneGlycol, // DOH, ethylene glycol/diol
		IodoAlkane, // I, iodoalkane
		BromoAlkane, // Br, bromoalkane
		TerminalAlkyne, // CH≡C, terminal alkyne carbons
		InternalAlkyne, // C≡C, internal alkyne carbons
		Dimethylsulfoxide, // DMSO, dimethylsulfoxide
		Acryolonitrile, // ACRY, acrylonitrile
		ChloroAlkene, // Cl(C≡C), chloroalkene
		BranchAlkene_4, // C=C, tetrasubstituted alkene carbons
		ArylFluoride, // aF, aryl fluoride (carbon not included)
		Dimethylformamide, // DMF, dimethylformamide
		DialkylFormamide, // OCHN(CH2)2, dialkylformamide
		PerfluoroAlkane_1, // CF3, perfluoro/trifluoroalkane carbon
		PerfluoroAlkane_2, // CF2, perfluoro/difluoroalkane carbon
		PerfluoroAlkane_3, // CF, perfluoro/fluoroalkane carbon
		GenericEster, // COO, generic ester (no alkane carbons)
		Silane_1, // SiH3, primary silane
		Silane_2, // SiH2, secondary silane
		Silane_3, // SiH, tertiary silane
		Silane_4, // Si, quaternary silane
		Siloxane_2, // SiOH2, secondary silane
		Siloxane_3, // SiOH, tertiary silane
		Siloxane_4, // SiO, quaternary silane
		nMethylpyrrolidone, // NMP, n-methylpyrrolidone
		Trichlorofluoromethane, // CCl3F, trichlorofluoromethane (R11)
		DichloroFluoroAlkane, // CCl2F, dichlorofluoro alkane carbon
		Dichlorofluoromethane, // CHCl2F, dichlorofluoromethane (R21)
		ChloroFluoroAlkane, // CHClF, chlorofluoro alkane carbon
		ChloroDifluoroAlkane, // CClF2, chlorodifluoro alkane carbon
		Chlorodifluoromethane, // CHClF, chlorodifluoromethane (R22)
		Chlorotrifluoromethane, // CClF3, chlorotrifluoromethane (R13)
		Dichlorodifluoromethane, // CCl2F2, dichlorodifluoromethane (R12)
		Alkanamide, // amH2, alkanamide carbon (no extra N-C bonds)
		MethylAlkanamide, // amHCH3, N-methyl alkanamide carbons
		AlkylAlkanamide, // amCHCH2, linear N-alkyl alkanamide carbons
		DimethylAlkanamide, // am(CH3)2, N,N-dimethyl alknamaide carbons
		AlkylMethylAlkanamide, // amCH3CH2, linear N-alkyl N-methyl alkanamide carbons
		DialkylAlkanamide, // am(CH2)2, linear N,N-dialkyl alkanamide carbons
		AlkoxyEthanol, // C2H5O2, 2-alkoxy ethanol
		AlkoxyAlkanol, // C2H4O2, 2-alkoxy alkanol
		MethylSulfide, // CH3S, methyl sulfide
		LinearAlkylSulfide, // CH2S, linear alkyl sulfate
		BranchAlkylSulfide, // CHS, branched alkyl sulfate
		Morpholine, // MORPH, morpholine
		Thiophene, // C4H4S, thiophene
		ThiopheneSub_2, // C4H3S, 2-substitution thiophene
		ThiopheneSub_2_3, // C4H2S, 2,3-substitution thiophene
		Isocyanate, // NCO, isocyanate
		LinearAlkylSulfone, // C4H4S, linear dialkyl sulfone
		BranchAlkylSulfone, // CH2CHSU, linear alkyl branched alkyl sulfone
		Imidazole, // imidazol, imidazole
		Bistriflimide // BTI, bistriflimide
	}

	private enum FunctionalMaingroup
	{
		CarbonylCO, // For temporary testing

		AlkaneC,
		AlkeneC,
		AromaticC,
		ArylAlkane,
		Alcohol,
		Methanol,
		Water,
		ArylAlcohol,
		AlkylKetone,
		Aldehyde,
		AlkylEster,
		Formate,
		AlkylEther,
		AlkylPriAmine,
		AlkylSecAmine,
		AlkylTerAmine,
		ArylPriAmine,
		Pyridine,
		AlkylCyanide,
		CarboxylicAcid,
		AlkylChloride,
		AlkylDichloride,
		AlkylTrichloride,
		AlkylTetrachloride,
		ArylChloride,
		NitroAlkane,
		NitroArene,
		CarbonDisulfide,
		AlkylThiol,
		Furfural,
		EthyleneGlycol,
		IodoAlkane,
		BromoAlkane,
		AlkyneC,
		Dimethylsulfoxide,
		Acrylonitrile,
		ChloroAlkene,
		ArylFluoride,
		DialkylFormamide,
		PerfluoroAlkane,
		GenericEster,
		Silane,
		Siloxane,
		nMethylpyrrolidone,
		ChloroFluoroAlkane,
		AlkylAmide,
		AlkoxyAlkanol,
		AlkylSulfide,
		Morpholine,
		Thiophene,
		Isocyanate,
		AlkylSulfone,
		Imidazole,
		Bistriflimide
	}

	/// <summary>
	/// Lists all subgroups and their parent maingroup.
	/// </summary>
	private static readonly Dictionary<FunctionalSubgroup, FunctionalMaingroup> MaingroupSubgroupMap = new()
	{
		[FunctionalSubgroup.CarbonylCO] = FunctionalMaingroup.CarbonylCO, // For temporary testing

		[FunctionalSubgroup.Alkyl_1] = FunctionalMaingroup.AlkaneC,
		[FunctionalSubgroup.Alkyl_2] = FunctionalMaingroup.AlkaneC,
		[FunctionalSubgroup.Alkyl_3] = FunctionalMaingroup.AlkaneC,
		[FunctionalSubgroup.Alkyl_4] = FunctionalMaingroup.AlkaneC,
		[FunctionalSubgroup.TerminalAlkene] = FunctionalMaingroup.AlkeneC,
		[FunctionalSubgroup.InternalAlkene] = FunctionalMaingroup.AlkeneC,
		[FunctionalSubgroup.BranchAlkene_2] = FunctionalMaingroup.AlkeneC,
		[FunctionalSubgroup.BranchAlkene_3] = FunctionalMaingroup.AlkeneC,
		[FunctionalSubgroup.UnsubAromaticC] = FunctionalMaingroup.AromaticC,
		[FunctionalSubgroup.SubAromaticC] = FunctionalMaingroup.AromaticC,
		[FunctionalSubgroup.ArylMethane] = FunctionalMaingroup.ArylAlkane,
		[FunctionalSubgroup.ArylLinearAlkane] = FunctionalMaingroup.ArylAlkane,
		[FunctionalSubgroup.ArylBranchAlkane] = FunctionalMaingroup.ArylAlkane,
		[FunctionalSubgroup.Alcohol] = FunctionalMaingroup.Alcohol,
		[FunctionalSubgroup.Methanol] = FunctionalMaingroup.Alcohol,
		[FunctionalSubgroup.Water] = FunctionalMaingroup.Water,
		[FunctionalSubgroup.ArylAlcohol] = FunctionalMaingroup.ArylAlcohol,
		[FunctionalSubgroup.MethylKetone] = FunctionalMaingroup.AlkylKetone,
		[FunctionalSubgroup.AlkylKetone] = FunctionalMaingroup.AlkylKetone,
		[FunctionalSubgroup.Aldehyde] = FunctionalMaingroup.Aldehyde,
		[FunctionalSubgroup.MethylEster] = FunctionalMaingroup.AlkylEster,
		[FunctionalSubgroup.AlkylEster] = FunctionalMaingroup.AlkylEster,
		[FunctionalSubgroup.AlkylFormate] = FunctionalMaingroup.Formate,
		[FunctionalSubgroup.MethylEther] = FunctionalMaingroup.AlkylEther,
		[FunctionalSubgroup.LinearAlkylEther] = FunctionalMaingroup.AlkylEther,
		[FunctionalSubgroup.BranchAlkylEther] = FunctionalMaingroup.AlkylEther,
		[FunctionalSubgroup.Tetrahydrofuran] = FunctionalMaingroup.AlkylEther,
		[FunctionalSubgroup.Methylamine] = FunctionalMaingroup.AlkylPriAmine,
		[FunctionalSubgroup.LinearAlkylPriAmine] = FunctionalMaingroup.AlkylPriAmine,
		[FunctionalSubgroup.BranchAlkylPriAmine] = FunctionalMaingroup.AlkylPriAmine,
		[FunctionalSubgroup.MethylSecAmine] = FunctionalMaingroup.AlkylSecAmine,
		[FunctionalSubgroup.LinearAlkylSecAmine] = FunctionalMaingroup.AlkylSecAmine,
		[FunctionalSubgroup.BranchAlkylSecAmine] = FunctionalMaingroup.AlkylSecAmine,
		[FunctionalSubgroup.MethylTerAmine] = FunctionalMaingroup.AlkylTerAmine,
		[FunctionalSubgroup.LinearAlkylTerAmine] = FunctionalMaingroup.AlkylTerAmine,
		[FunctionalSubgroup.ArylPriAmine] = FunctionalMaingroup.ArylPriAmine,
		[FunctionalSubgroup.Pyridine] = FunctionalMaingroup.Pyridine,
		[FunctionalSubgroup.Pyridine_2] = FunctionalMaingroup.Pyridine,
		[FunctionalSubgroup.Pyridine_2_3] = FunctionalMaingroup.Pyridine,
		[FunctionalSubgroup.Acetonitrile] = FunctionalMaingroup.AlkylCyanide,
		[FunctionalSubgroup.LinearAlkylCyanide] = FunctionalMaingroup.AlkylCyanide,
		[FunctionalSubgroup.CarboxylicAcid] = FunctionalMaingroup.CarboxylicAcid,
		[FunctionalSubgroup.FormicAcid] = FunctionalMaingroup.CarboxylicAcid,
		[FunctionalSubgroup.ChloroAlkyl_1] = FunctionalMaingroup.AlkylChloride,
		[FunctionalSubgroup.ChloroAlkyl_2] = FunctionalMaingroup.AlkylChloride,
		[FunctionalSubgroup.ChloroAlkyl_3] = FunctionalMaingroup.AlkylChloride,
		[FunctionalSubgroup.Dichloromethane] = FunctionalMaingroup.AlkylDichloride,
		[FunctionalSubgroup.DichloroAlkyl_1] = FunctionalMaingroup.AlkylDichloride,
		[FunctionalSubgroup.DichloroAlkyl_2] = FunctionalMaingroup.AlkylDichloride,
		[FunctionalSubgroup.Trichloromethane] = FunctionalMaingroup.AlkylTrichloride,
		[FunctionalSubgroup.TrichloroAlkyl] = FunctionalMaingroup.AlkylTrichloride,
		[FunctionalSubgroup.Tetrachloromethane] = FunctionalMaingroup.AlkylTetrachloride,
		[FunctionalSubgroup.ArylChloride] = FunctionalMaingroup.ArylChloride,
		[FunctionalSubgroup.Nitromethane] = FunctionalMaingroup.NitroAlkane,
		[FunctionalSubgroup.NitroAlkane_1] = FunctionalMaingroup.NitroAlkane,
		[FunctionalSubgroup.NitroAlkane_2] = FunctionalMaingroup.NitroAlkane,
		[FunctionalSubgroup.NitroArene] = FunctionalMaingroup.NitroArene,
		[FunctionalSubgroup.CarbonDisulfide] = FunctionalMaingroup.CarbonDisulfide,
		[FunctionalSubgroup.Methylthiol] = FunctionalMaingroup.AlkylThiol,
		[FunctionalSubgroup.PriAlkylThiol] = FunctionalMaingroup.AlkylThiol,
		[FunctionalSubgroup.Furfural] = FunctionalMaingroup.Furfural,
		[FunctionalSubgroup.EthyleneGlycol] = FunctionalMaingroup.EthyleneGlycol,
		[FunctionalSubgroup.IodoAlkane] = FunctionalMaingroup.IodoAlkane,
		[FunctionalSubgroup.BromoAlkane] = FunctionalMaingroup.BromoAlkane,
		[FunctionalSubgroup.TerminalAlkyne] = FunctionalMaingroup.AlkyneC,
		[FunctionalSubgroup.InternalAlkyne] = FunctionalMaingroup.AlkyneC,
		[FunctionalSubgroup.Dimethylsulfoxide] = FunctionalMaingroup.Dimethylsulfoxide,
		[FunctionalSubgroup.Acryolonitrile] = FunctionalMaingroup.Acrylonitrile,
		[FunctionalSubgroup.ChloroAlkene] = FunctionalMaingroup.ChloroAlkene,
		[FunctionalSubgroup.BranchAlkene_4] = FunctionalMaingroup.AlkeneC,
		[FunctionalSubgroup.ArylFluoride] = FunctionalMaingroup.ArylFluoride,
		[FunctionalSubgroup.Dimethylformamide] = FunctionalMaingroup.DialkylFormamide,
		[FunctionalSubgroup.DialkylFormamide] = FunctionalMaingroup.DialkylFormamide,
		[FunctionalSubgroup.PerfluoroAlkane_1] = FunctionalMaingroup.PerfluoroAlkane,
		[FunctionalSubgroup.PerfluoroAlkane_2] = FunctionalMaingroup.PerfluoroAlkane,
		[FunctionalSubgroup.PerfluoroAlkane_3] = FunctionalMaingroup.PerfluoroAlkane,
		[FunctionalSubgroup.GenericEster] = FunctionalMaingroup.GenericEster,
		[FunctionalSubgroup.Silane_1] = FunctionalMaingroup.Silane,
		[FunctionalSubgroup.Silane_2] = FunctionalMaingroup.Silane,
		[FunctionalSubgroup.Silane_3] = FunctionalMaingroup.Silane,
		[FunctionalSubgroup.Silane_4] = FunctionalMaingroup.Silane,
		[FunctionalSubgroup.Siloxane_2] = FunctionalMaingroup.Siloxane,
		[FunctionalSubgroup.Siloxane_3] = FunctionalMaingroup.Siloxane,
		[FunctionalSubgroup.Siloxane_4] = FunctionalMaingroup.Siloxane,
		[FunctionalSubgroup.nMethylpyrrolidone] = FunctionalMaingroup.nMethylpyrrolidone,
		[FunctionalSubgroup.Trichlorofluoromethane] = FunctionalMaingroup.ChloroFluoroAlkane,
		[FunctionalSubgroup.DichloroFluoroAlkane] = FunctionalMaingroup.ChloroFluoroAlkane,
		[FunctionalSubgroup.Dichlorofluoromethane] = FunctionalMaingroup.ChloroFluoroAlkane,
		[FunctionalSubgroup.ChloroFluoroAlkane] = FunctionalMaingroup.ChloroFluoroAlkane,
		[FunctionalSubgroup.ChloroDifluoroAlkane] = FunctionalMaingroup.ChloroFluoroAlkane,
		[FunctionalSubgroup.Chlorodifluoromethane] = FunctionalMaingroup.ChloroFluoroAlkane,
		[FunctionalSubgroup.Chlorotrifluoromethane] = FunctionalMaingroup.ChloroFluoroAlkane,
		[FunctionalSubgroup.Dichlorodifluoromethane] = FunctionalMaingroup.ChloroFluoroAlkane,
		[FunctionalSubgroup.Alkanamide] = FunctionalMaingroup.AlkylAmide,
		[FunctionalSubgroup.MethylAlkanamide] = FunctionalMaingroup.AlkylAmide,
		[FunctionalSubgroup.AlkylAlkanamide] = FunctionalMaingroup.AlkylAmide,
		[FunctionalSubgroup.DimethylAlkanamide] = FunctionalMaingroup.AlkylAmide,
		[FunctionalSubgroup.AlkylMethylAlkanamide] = FunctionalMaingroup.AlkylAmide,
		[FunctionalSubgroup.DialkylAlkanamide] = FunctionalMaingroup.AlkylAmide,
		[FunctionalSubgroup.AlkoxyEthanol] = FunctionalMaingroup.AlkoxyAlkanol,
		[FunctionalSubgroup.AlkoxyAlkanol] = FunctionalMaingroup.AlkoxyAlkanol,
		[FunctionalSubgroup.MethylSulfide] = FunctionalMaingroup.AlkylSulfide,
		[FunctionalSubgroup.LinearAlkylSulfide] = FunctionalMaingroup.AlkylSulfide,
		[FunctionalSubgroup.BranchAlkylSulfide] = FunctionalMaingroup.AlkylSulfide,
		[FunctionalSubgroup.Morpholine] = FunctionalMaingroup.Morpholine,
		[FunctionalSubgroup.Thiophene] = FunctionalMaingroup.Thiophene,
		[FunctionalSubgroup.ThiopheneSub_2] = FunctionalMaingroup.Thiophene,
		[FunctionalSubgroup.ThiopheneSub_2_3] = FunctionalMaingroup.Thiophene,
		[FunctionalSubgroup.Isocyanate] = FunctionalMaingroup.Isocyanate,
		[FunctionalSubgroup.LinearAlkylSulfone] = FunctionalMaingroup.AlkylSulfone,
		[FunctionalSubgroup.BranchAlkylSulfone] = FunctionalMaingroup.AlkylSulfone,
		[FunctionalSubgroup.Imidazole] = FunctionalMaingroup.Imidazole,
		[FunctionalSubgroup.Bistriflimide] = FunctionalMaingroup.Bistriflimide
	};

	/// <summary>
	/// Lists all subgroups and their quantities for all UNIFAC-modeled chemicals.
	/// </summary>
	private static readonly Dictionary<Chemical, List<(FunctionalSubgroup subgroup, int nu)>> ChemicalSubgroupMap = new()
	{
		//[Chemical.Acetone] = [ (FunctionalSubgroup.MethylKetone, 1), (FunctionalSubgroup.Alkyl_1, 1) ],
		[Chemical.Acetone] = [ (FunctionalSubgroup.CarbonylCO, 1), (FunctionalSubgroup.Alkyl_1, 2) ],
		[Chemical.Benzene] = [ (FunctionalSubgroup.UnsubAromaticC, 6) ],
		[Chemical.NButane] = [ (FunctionalSubgroup.Alkyl_1, 2), (FunctionalSubgroup.Alkyl_2, 2) ],
		[Chemical.Isobutane] = [ (FunctionalSubgroup.Alkyl_1, 3), (FunctionalSubgroup.Alkyl_3, 1) ],
		[Chemical.Chlorobenzene] = [ (FunctionalSubgroup.ArylChloride, 1), (FunctionalSubgroup.UnsubAromaticC, 5) ],
		[Chemical.Ethane] = [ (FunctionalSubgroup.Alkyl_1, 2) ],
		//[Chemical.Methane] = [ (FuncSubgroup.Methane, 1) ],
		[Chemical.NPentane] = [ (FunctionalSubgroup.Alkyl_1, 2), (FunctionalSubgroup.Alkyl_2, 3) ],
		[Chemical.Isopentane] = [ (FunctionalSubgroup.Alkyl_1, 3), (FunctionalSubgroup.Alkyl_2, 1), (FunctionalSubgroup.Alkyl_3, 1) ],
		[Chemical.Propane] = [ (FunctionalSubgroup.Alkyl_1, 2), (FunctionalSubgroup.Alkyl_2, 1) ],
		[Chemical.NPropanol] = [ (FunctionalSubgroup.Alcohol, 1), (FunctionalSubgroup.Alkyl_1, 1), (FunctionalSubgroup.Alkyl_2, 2) ],
		[Chemical.R12] = [ (FunctionalSubgroup.Dichlorodifluoromethane, 1) ],
		[Chemical.Toluene] = [ (FunctionalSubgroup.ArylMethane, 1), (FunctionalSubgroup.UnsubAromaticC, 5) ],
		[Chemical.Water] = [ (FunctionalSubgroup.Water, 1) ]
	};

	/// <summary>
	/// Stores R (volume) and Q (surface area) parameters for UNIFAC functional groups.
	/// </summary>
	private static readonly Dictionary<FunctionalSubgroup, (double R, double Q)> SubgroupRQParameters = new()
	{
		[FunctionalSubgroup.CarbonylCO] = (0.7713, 0.640), // For temporary testing

		[FunctionalSubgroup.Alkyl_1] = (0.9011, 0.8480),
		[FunctionalSubgroup.Alkyl_2] = (0.6744, 0.5400),
		[FunctionalSubgroup.Alkyl_3] = (0.4469, 0.2280),
		[FunctionalSubgroup.Alkyl_4] = (0.2195, 0.0000),
		[FunctionalSubgroup.TerminalAlkene] = (1.3454, 1.1760),
		[FunctionalSubgroup.InternalAlkene] = (1.1167, 0.8670),
		[FunctionalSubgroup.BranchAlkene_2] = (1.1173, 0.9880),
		[FunctionalSubgroup.BranchAlkene_3] = (0.8886, 0.6760),
		[FunctionalSubgroup.UnsubAromaticC] = (0.5313, 0.4000),
		[FunctionalSubgroup.SubAromaticC] = (0.3652, 0.1200),
		[FunctionalSubgroup.ArylMethane] = (1.2663, 0.9680),
		[FunctionalSubgroup.ArylLinearAlkane] = (1.0396, 0.6600),
		[FunctionalSubgroup.ArylBranchAlkane] = (0.8121, 0.3480),
		[FunctionalSubgroup.Alcohol] = (1.0000, 1.2000),
		[FunctionalSubgroup.Methanol] = (1.4311, 1.4320),
		[FunctionalSubgroup.Water] = (0.9200, 1.4000),
		[FunctionalSubgroup.ArylAlcohol] = (0.8952, 0.6800),
		[FunctionalSubgroup.MethylKetone] = (1.6724, 1.4880),
		[FunctionalSubgroup.AlkylKetone] = (1.4457, 1.1800),
		[FunctionalSubgroup.Aldehyde] = (0.9980, 0.9480),
		[FunctionalSubgroup.MethylEster] = (1.9031, 1.7280),
		[FunctionalSubgroup.AlkylEster] = (1.6764, 1.4200),
		[FunctionalSubgroup.AlkylFormate] = (1.2420, 1.1880),
		[FunctionalSubgroup.MethylEther] = (1.4500, 1.0880),
		[FunctionalSubgroup.LinearAlkylEther] = (0.9183, 0.7800),
		[FunctionalSubgroup.BranchAlkylEther] = (0.6908, 0.4680),
		[FunctionalSubgroup.Tetrahydrofuran] = (0.9183, 1.1000),
		[FunctionalSubgroup.Methylamine] = (1.5959, 1.5440),
		[FunctionalSubgroup.LinearAlkylPriAmine] = (1.3692, 1.2360),
		[FunctionalSubgroup.BranchAlkylPriAmine] = (1.1417, 0.9240),
		[FunctionalSubgroup.MethylSecAmine] = (1.4337, 1.2440),
		[FunctionalSubgroup.LinearAlkylSecAmine] = (1.2070, 0.9360),
		[FunctionalSubgroup.BranchAlkylSecAmine] = (0.9795, 0.6240),
		[FunctionalSubgroup.MethylTerAmine] = (1.1865, 0.9400),
		[FunctionalSubgroup.LinearAlkylTerAmine] = (0.9597, 0.6320),
		[FunctionalSubgroup.ArylPriAmine] = (1.0600, 0.8160),
		[FunctionalSubgroup.Pyridine] = (2.9993, 2.1130),
		[FunctionalSubgroup.Pyridine_2] = (2.8332, 1.8330),
		[FunctionalSubgroup.Pyridine_2_3] = (2.6670, 1.5530),
		[FunctionalSubgroup.Acetonitrile] = (1.8701, 1.7240),
		[FunctionalSubgroup.LinearAlkylCyanide] = (1.6434, 1.4160),
		[FunctionalSubgroup.CarboxylicAcid] = (1.3013, 1.2240),
		[FunctionalSubgroup.FormicAcid] = (1.5280, 1.5320),
		[FunctionalSubgroup.ChloroAlkyl_1] = (1.4654, 1.2640),
		[FunctionalSubgroup.ChloroAlkyl_2] = (1.2380, 0.9520),
		[FunctionalSubgroup.ChloroAlkyl_3] = (1.0106, 0.7240),
		[FunctionalSubgroup.Dichloromethane] = (2.2564, 1.9880),
		[FunctionalSubgroup.DichloroAlkyl_1] = (2.0606, 1.6840),
		[FunctionalSubgroup.DichloroAlkyl_2] = (1.8016, 1.4480),
		[FunctionalSubgroup.Trichloromethane] = (2.8700, 2.4100),
		[FunctionalSubgroup.TrichloroAlkyl] = (2.6401, 2.1840),
		[FunctionalSubgroup.Tetrachloromethane] = (3.3900, 2.9100),
		[FunctionalSubgroup.ArylChloride] = (1.1562, 0.8440),
		[FunctionalSubgroup.Nitromethane] = (2.0086, 1.8680),
		[FunctionalSubgroup.NitroAlkane_1] = (1.7818, 1.5600),
		[FunctionalSubgroup.NitroAlkane_2] = (1.5544, 1.2480),
		[FunctionalSubgroup.NitroArene] = (1.4199, 1.1040),
		[FunctionalSubgroup.CarbonDisulfide] = (2.0570, 1.6500),
		[FunctionalSubgroup.Methylthiol] = (1.8770, 1.6760),
		[FunctionalSubgroup.PriAlkylThiol] = (1.6510, 1.3680),
		[FunctionalSubgroup.Furfural] = (3.1680, 2.4840),
		[FunctionalSubgroup.EthyleneGlycol] = (2.4088, 2.2480),
		[FunctionalSubgroup.IodoAlkane] = (1.2640, 0.9920),
		[FunctionalSubgroup.BromoAlkane] = (0.9492, 0.8320),
		[FunctionalSubgroup.TerminalAlkyne] = (1.2920, 1.0880),
		[FunctionalSubgroup.InternalAlkyne] = (1.0613, 0.7840),
		[FunctionalSubgroup.Dimethylsulfoxide] = (2.8266, 2.4720),
		[FunctionalSubgroup.Acryolonitrile] = (2.3144, 2.0520),
		[FunctionalSubgroup.ChloroAlkene] = (0.7910, 0.7240),
		[FunctionalSubgroup.BranchAlkene_4] = (0.6605, 0.4850),
		[FunctionalSubgroup.ArylFluoride] = (0.6948, 0.5240),
		[FunctionalSubgroup.Dimethylformamide] = (3.0856, 2.7360),
		[FunctionalSubgroup.DialkylFormamide] = (2.6322, 2.1200),
		[FunctionalSubgroup.PerfluoroAlkane_1] = (1.4060, 1.3800),
		[FunctionalSubgroup.PerfluoroAlkane_2] = (1.0105, 0.9200),
		[FunctionalSubgroup.PerfluoroAlkane_3] = (0.6150, 0.4600),
		[FunctionalSubgroup.GenericEster] = (1.3800, 1.2000),
		[FunctionalSubgroup.Silane_1] = (1.6035, 1.2632),
		[FunctionalSubgroup.Silane_2] = (1.4443, 1.0063),
		[FunctionalSubgroup.Silane_3] = (1.2853, 0.7494),
		[FunctionalSubgroup.Silane_4] = (1.0470, 0.4099),
		[FunctionalSubgroup.Siloxane_2] = (1.4838, 1.0621),
		[FunctionalSubgroup.Siloxane_3] = (1.3030, 0.7639),
		[FunctionalSubgroup.Siloxane_4] = (1.1044, 0.4657),
		[FunctionalSubgroup.nMethylpyrrolidone] = (3.9810, 3.2000),
		[FunctionalSubgroup.Trichlorofluoromethane] = (3.0356, 2.6440),
		[FunctionalSubgroup.DichloroFluoroAlkane] = (2.2287, 1.9160),
		[FunctionalSubgroup.Dichlorofluoromethane] = (2.4060, 2.1160),
		[FunctionalSubgroup.ChloroFluoroAlkane] = (1.6493, 1.4160),
		[FunctionalSubgroup.ChloroDifluoroAlkane] = (1.8174, 1.6480),
		[FunctionalSubgroup.Chlorodifluoromethane] = (1.9670, 1.8280),
		[FunctionalSubgroup.Chlorotrifluoromethane] = (2.1721, 2.1000),
		[FunctionalSubgroup.Dichlorodifluoromethane] = (2.6243, 2.3760),
		[FunctionalSubgroup.Alkanamide] = (1.4515, 1.2480),
		[FunctionalSubgroup.MethylAlkanamide] = (2.1905, 1.7960),
		[FunctionalSubgroup.AlkylAlkanamide] = (1.9637, 1.4880),
		[FunctionalSubgroup.DimethylAlkanamide] = (2.8589, 2.4280),
		[FunctionalSubgroup.AlkylMethylAlkanamide] = (2.6322, 2.1200),
		[FunctionalSubgroup.DialkylAlkanamide] = (2.4054, 1.8120),
		[FunctionalSubgroup.AlkoxyEthanol] = (2.1226, 1.9040),
		[FunctionalSubgroup.AlkoxyAlkanol] = (1.8952, 1.5920),
		[FunctionalSubgroup.MethylSulfide] = (1.6130, 1.3680),
		[FunctionalSubgroup.LinearAlkylSulfide] = (1.3863, 1.0600),
		[FunctionalSubgroup.BranchAlkylSulfide] = (1.1589, 0.7480),
		[FunctionalSubgroup.Morpholine] = (3.4740, 2.7960),
		[FunctionalSubgroup.Thiophene] = (2.8569, 2.1400),
		[FunctionalSubgroup.ThiopheneSub_2] = (2.6908, 1.8600),
		[FunctionalSubgroup.ThiopheneSub_2_3] = (2.5247, 1.5800),
		[FunctionalSubgroup.Isocyanate] = (1.0567, 0.7320),
		[FunctionalSubgroup.LinearAlkylSulfone] = (2.6869, 2.1200),
		[FunctionalSubgroup.BranchAlkylSulfone] = (2.4595, 1.8080),
		[FunctionalSubgroup.Imidazole] = (2.0260, 0.8680),
		[FunctionalSubgroup.Bistriflimide] = (5.7740, 4.9320),
	};

	/// <summary>
	/// Stores interaction parameters for UNIFAC maingroups.
	/// </summary>
	private static readonly Dictionary<(FunctionalMaingroup i, FunctionalMaingroup j), (double aij, double aji)> MaingroupInteractionParameters = new()
	{
		[(FunctionalMaingroup.CarbonylCO, FunctionalMaingroup.AlkaneC)] = (3000, 1565), // For temporary testing

		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.AlkeneC)] = (86.0200, -35.3600),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.AromaticC)] = (61.1300, -11.1200),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.ArylAlkane)] = (76.5000, -69.7000),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.Alcohol)] = (986.5000, 156.4000),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.Methanol)] = (697.2000, 16.5100),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.Water)] = (1318.0000, 300.0000),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.ArylAlcohol)] = (1333.0000, 275.8000),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.AlkylKetone)] = (476.4000, 26.7600),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.Aldehyde)] = (677.0000, 505.7000),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.AlkylEster)] = (232.1000, 114.8000),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.Formate)] = (507.0000, 329.3000),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.AlkylEther)] = (251.5000, 83.3600),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.AlkylPriAmine)] = (391.5000, -30.4800),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.AlkylSecAmine)] = (255.7000, 65.3300),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.AlkylTerAmine)] = (206.6000, -83.9800),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.ArylPriAmine)] = (920.7000, 1139.0000),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.Pyridine)] = (287.7700, -101.5600),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.AlkylCyanide)] = (597.0000, 24.8200),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.CarboxylicAcid)] = (663.5000, 315.3000),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.AlkylChloride)] = (35.9300, 91.4600),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.AlkylDichloride)] = (53.7600, 34.0100),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.AlkylTrichloride)] = (24.9000, 36.7000),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.AlkylTetrachloride)] = (104.3000, -78.4500),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.ArylChloride)] = (11.4400, 106.8000),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.NitroAlkane)] = (661.5000, -32.6900),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.NitroArene)] = (543.0000, 5541.0000),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.CarbonDisulfide)] = (153.6000, -52.6500),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.AlkylThiol)] = (184.4000, -7.4810),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.Furfural)] = (354.5500, -25.3100),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.EthyleneGlycol)] = (3025.0000, 139.9300),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.IodoAlkane)] = (335.8000, 128.0000),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.BromoAlkane)] = (479.5000, -31.5200),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.AlkyneC)] = (298.9000, -72.8800),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.Dimethylsulfoxide)] = (526.5000, 50.4900),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.Acrylonitrile)] = (689.0000, -165.9000),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.ChloroAlkene)] = (-4.1890, 47.4100),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.ArylFluoride)] = (125.8000, -5.1320),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.DialkylFormamide)] = (485.3000, -31.9500),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.PerfluoroAlkane)] = (-2.8590, 147.3000),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.GenericEster)] = (387.1000, 529.0000),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.Silane)] = (-450.4000, -34.3600),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.Siloxane)] = (252.7000, 110.2000),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.nMethylpyrrolidone)] = (220.3000, 13.8900),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.ChloroFluoroAlkane)] = (-5.8690, 30.7400),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.AlkylAmide)] = (390.9000, 27.9700),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.AlkoxyAlkanol)] = (553.3000, -11.9200),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.AlkylSulfide)] = (187.0000, 39.9300),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.Morpholine)] = (216.1000, -23.6100),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.Thiophene)] = (92.9900, -8.4790),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.Isocyanate)] = (699.1300, 456.1900),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.AlkylSulfone)] = (808.5900, 245.2100),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.Imidazole)] = (-1243.0000, 125.3600),
		[(FunctionalMaingroup.AlkaneC, FunctionalMaingroup.Bistriflimide)] = (637.6500, 221.5600),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.AromaticC)] = (38.8100, 3.4460),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.ArylAlkane)] = (74.1500, -113.6000),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.Alcohol)] = (524.1000, 457.0000),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.Methanol)] = (787.6000, -12.5200),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.Water)] = (270.6000, 496.1000),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.ArylAlcohol)] = (526.1000, 217.5000),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.AlkylKetone)] = (182.6000, 42.9200),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.Aldehyde)] = (448.7500, 56.3000),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.AlkylEster)] = (37.8500, 132.1000),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.Formate)] = (333.5000, 110.4000),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.AlkylEther)] = (214.5000, 26.5100),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.AlkylPriAmine)] = (240.9000, 1.1630),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.AlkylSecAmine)] = (163.9000, -28.7000),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.AlkylTerAmine)] = (61.1100, -25.3800),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.ArylPriAmine)] = (749.3000, 2000.0000),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.Pyridine)] = (280.5000, -47.6300),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.AlkylCyanide)] = (336.9000, -40.6200),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.CarboxylicAcid)] = (318.9000, 1264.0000),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.AlkylChloride)] = (-36.8700, 40.2500),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.AlkylDichloride)] = (58.5500, -23.5000),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.AlkylTrichloride)] = (-13.9900, 51.0600),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.AlkylTetrachloride)] = (-109.7000, 160.9000),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.ArylChloride)] = (100.1000, 70.3200),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.NitroAlkane)] = (357.5000, -1.9960),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.CarbonDisulfide)] = (76.3000, 16.6230),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.Furfural)] = (262.9000, 82.6400),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.BromoAlkane)] = (183.8000, 174.6000),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.AlkyneC)] = (31.1400, 41.3800),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.Dimethylsulfoxide)] = (179.0000, 64.0700),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.Acrylonitrile)] = (-52.8700, 573.0000),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.ChloroAlkene)] = (-66.4600, 124.2000),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.ArylFluoride)] = (359.3000, -131.7000),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.DialkylFormamide)] = (-70.4500, 249.0000),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.PerfluoroAlkane)] = (449.4000, 62.4000),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.GenericEster)] = (48.3300, 1397.0000),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.nMethylpyrrolidone)] = (86.4600, -16.1100),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.AlkylAmide)] = (200.2000, 9.7550),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.AlkoxyAlkanol)] = (268.1000, 132.4000),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.AlkylSulfide)] = (-617.0000, 543.6000),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.Morpholine)] = (62.5600, 161.1000),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.AlkylSulfone)] = (200.9400, 384.4500),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.Imidazole)] = (-861.4600, -391.8100),
		[(FunctionalMaingroup.AlkeneC, FunctionalMaingroup.Bistriflimide)] = (424.9300, 629.9600),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.ArylAlkane)] = (167.0000, -146.8000),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.Alcohol)] = (636.1000, 89.6000),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.Methanol)] = (637.3500, -50.0000),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.Water)] = (903.8000, 362.3000),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.ArylAlcohol)] = (1329.0000, 25.3400),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.AlkylKetone)] = (25.7700, 140.1000),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.Aldehyde)] = (347.3000, 23.3900),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.AlkylEster)] = (5.9940, 85.8400),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.Formate)] = (287.1000, 18.1200),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.AlkylEther)] = (32.1400, 52.1300),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.AlkylPriAmine)] = (161.7000, -44.8500),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.AlkylSecAmine)] = (122.8000, -22.3100),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.AlkylTerAmine)] = (90.4900, -223.9000),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.ArylPriAmine)] = (648.2000, 247.5000),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.Pyridine)] = (-4.4490, 31.8700),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.AlkylCyanide)] = (212.5000, -22.9700),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.CarboxylicAcid)] = (537.4000, 62.3200),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.AlkylChloride)] = (-18.8100, 4.6800),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.AlkylDichloride)] = (-144.4000, 121.3000),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.AlkylTrichloride)] = (-231.9000, 288.5000),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.AlkylTetrachloride)] = (3.0000, -4.7000),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.ArylChloride)] = (187.0000, -97.2700),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.NitroAlkane)] = (168.0400, 10.3800),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.NitroArene)] = (194.9000, 1824.0000),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.CarbonDisulfide)] = (52.0680, 21.4970),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.AlkylThiol)] = (-10.4300, 28.4100),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.Furfural)] = (-64.6900, 157.2900),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.EthyleneGlycol)] = (210.3660, 221.4000),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.IodoAlkane)] = (113.3000, 58.6800),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.BromoAlkane)] = (261.3000, -154.2000),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.AlkyneC)] = (154.2600, -101.1200),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.Dimethylsulfoxide)] = (169.9000, -2.5040),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.Acrylonitrile)] = (383.9000, -123.6000),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.ChloroAlkene)] = (-259.1000, 395.8000),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.ArylFluoride)] = (389.3000, -237.2000),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.DialkylFormamide)] = (245.6000, -133.9000),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.PerfluoroAlkane)] = (22.6700, 140.6000),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.GenericEster)] = (103.5000, 317.6000),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.Silane)] = (-432.3000, 787.9000),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.Siloxane)] = (238.9000, 234.4000),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.nMethylpyrrolidone)] = (30.0400, -23.8800),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.ChloroFluoroAlkane)] = (-88.1100, 167.9000),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.AlkoxyAlkanol)] = (333.3000, -86.8800),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.Morpholine)] = (-59.5800, 142.9000),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.Thiophene)] = (-39.1600, 23.9300),
		[(FunctionalMaingroup.AromaticC, FunctionalMaingroup.AlkylSulfone)] = (360.8200, 47.0500),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.Alcohol)] = (803.2000, 25.8200),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.Methanol)] = (603.2500, -44.5000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.Water)] = (5695.0000, 377.6000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.ArylAlcohol)] = (884.9000, 244.2000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.AlkylKetone)] = (-52.1000, 365.8000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.Aldehyde)] = (586.8000, 106.0000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.AlkylEster)] = (5688.0000, -170.0000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.Formate)] = (197.8000, 428.0000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.AlkylEther)] = (213.1000, 65.6900),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.AlkylPriAmine)] = (19.0200, 296.4000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.AlkylSecAmine)] = (-49.2900, 223.0000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.AlkylTerAmine)] = (23.5000, 109.9000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.ArylPriAmine)] = (664.2000, 762.8000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.Pyridine)] = (52.8000, 49.8000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.AlkylCyanide)] = (6096.0000, -138.4000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.CarboxylicAcid)] = (872.3000, 89.8600),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.AlkylChloride)] = (-114.1400, 122.9100),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.AlkylDichloride)] = (-111.0000, 140.7800),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.AlkylTrichloride)] = (-80.2500, 69.9000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.AlkylTetrachloride)] = (-141.3000, 134.7000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.ArylChloride)] = (-211.0000, 402.5000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.NitroAlkane)] = (3629.0000, -97.0500),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.NitroArene)] = (4448.0000, -127.8000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.CarbonDisulfide)] = (-9.4510, 40.6750),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.AlkylThiol)] = (393.6000, 19.5600),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.Furfural)] = (48.4900, 128.8000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.EthyleneGlycol)] = (4975.0000, 150.6400),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.IodoAlkane)] = (259.0000, 26.4100),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.BromoAlkane)] = (210.0000, 1112.0000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.AlkyneC)] = (-152.5500, 614.5200),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.Dimethylsulfoxide)] = (4284.0000, -143.2000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.Acrylonitrile)] = (-119.2000, 397.4000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.ChloroAlkene)] = (-282.5000, 419.1000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.ArylFluoride)] = (101.4000, -157.3000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.DialkylFormamide)] = (5629.0000, -240.2000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.PerfluoroAlkane)] = (-245.3900, 839.8300),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.GenericEster)] = (69.2600, 615.8000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.nMethylpyrrolidone)] = (46.3800, 6.2140),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.AlkoxyAlkanol)] = (421.9000, -19.4500),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.Morpholine)] = (-203.6000, 274.1000),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.Thiophene)] = (184.9000, 2.8450),
		[(FunctionalMaingroup.ArylAlkane, FunctionalMaingroup.AlkylSulfone)] = (233.5100, 347.1300),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.Methanol)] = (-137.1000, 249.1000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.Water)] = (353.5000, -229.1000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.ArylAlcohol)] = (-259.7000, -451.6000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.AlkylKetone)] = (84.0000, 164.5000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.Aldehyde)] = (-203.6000, 529.0000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.AlkylEster)] = (101.1000, 245.4000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.Formate)] = (267.8000, 139.4000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.AlkylEther)] = (28.0600, 237.7000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.AlkylPriAmine)] = (8.6420, -242.8000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.AlkylSecAmine)] = (42.7000, -150.0000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.AlkylTerAmine)] = (-323.0000, 28.6000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.ArylPriAmine)] = (-52.3900, -17.4000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.Pyridine)] = (170.0290, -132.3000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.AlkylCyanide)] = (6.7120, 185.4000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.CarboxylicAcid)] = (199.0000, -151.0000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.AlkylChloride)] = (75.6200, 562.2000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.AlkylDichloride)] = (65.2800, 527.6000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.AlkylTrichloride)] = (-98.1200, 742.1000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.AlkylTetrachloride)] = (143.1000, 856.3000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.ArylChloride)] = (123.5000, 325.7000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.NitroAlkane)] = (256.5000, 261.6000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.NitroArene)] = (157.1000, 561.6000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.CarbonDisulfide)] = (488.9000, 609.8000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.AlkylThiol)] = (147.5000, 461.6000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.Furfural)] = (-120.4600, 521.6300),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.EthyleneGlycol)] = (-318.9300, 267.6000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.IodoAlkane)] = (313.5000, 501.3000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.BromoAlkane)] = (202.1000, 524.9000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.AlkyneC)] = (727.8000, 68.9500),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.Dimethylsulfoxide)] = (-202.1000, -25.8700),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.Acrylonitrile)] = (74.2700, 389.3000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.ChloroAlkene)] = (225.8000, 738.9000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.ArylFluoride)] = (44.7800, 649.7000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.DialkylFormamide)] = (-143.9000, 64.1600),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.GenericEster)] = (190.3000, 88.6300),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.Silane)] = (-817.7000, 1913.0000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.Siloxane)] = (-1712.8000, 430.0600),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.nMethylpyrrolidone)] = (-504.2000, 796.9000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.ChloroFluoroAlkane)] = (72.9600, 794.4000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.AlkylAmide)] = (-382.7000, 394.8000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.AlkoxyAlkanol)] = (-248.3000, 517.5000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.Morpholine)] = (104.7000, -61.2000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.Thiophene)] = (57.6500, 682.5000),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.AlkylSulfone)] = (215.8100, 72.1900),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.Imidazole)] = (-1840.8000, 111.6500),
		[(FunctionalMaingroup.Alcohol, FunctionalMaingroup.Bistriflimide)] = (56.2980, 122.1900),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.Water)] = (-180.9500, 289.6000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.ArylAlcohol)] = (-101.7000, -265.2000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.AlkylKetone)] = (23.3900, 108.6500),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.Aldehyde)] = (306.4200, -340.1800),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.AlkylEster)] = (-10.7200, 249.6300),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.Formate)] = (179.7000, 227.8000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.AlkylEther)] = (-128.6000, 238.4000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.AlkylPriAmine)] = (359.3000, -481.6500),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.AlkylSecAmine)] = (-20.9800, -370.3000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.AlkylTerAmine)] = (53.9000, -406.8000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.ArylPriAmine)] = (489.7000, -118.1000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.Pyridine)] = (580.4800, -378.2400),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.AlkylCyanide)] = (53.2800, 162.6000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.CarboxylicAcid)] = (-202.0000, 339.8000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.AlkylChloride)] = (-38.3200, 529.0000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.AlkylDichloride)] = (-102.5400, 669.9000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.AlkylTrichloride)] = (-139.3500, 649.1000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.AlkylTetrachloride)] = (-44.7600, 709.6000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.ArylChloride)] = (-28.2500, 612.8000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.NitroAlkane)] = (75.1400, 252.5600),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.NitroArene)] = (457.8800, 511.2900),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.CarbonDisulfide)] = (-31.0900, 914.2000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.AlkylThiol)] = (17.5000, 448.6000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.Furfural)] = (-61.7600, 287.0000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.EthyleneGlycol)] = (-119.2000, 240.8000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.IodoAlkane)] = (212.1000, 431.3000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.BromoAlkane)] = (106.3000, 494.7000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.AlkyneC)] = (-119.1000, 967.7100),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.Dimethylsulfoxide)] = (-399.3000, 695.0000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.Acrylonitrile)] = (-5.2240, 218.8000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.ChloroAlkene)] = (33.4700, 528.0000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.ArylFluoride)] = (-48.2500, 645.9000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.DialkylFormamide)] = (-172.4000, 172.2000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.GenericEster)] = (165.7000, 171.0000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.ChloroFluoroAlkane)] = (-52.1000, 762.7000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.AlkylSulfide)] = (37.6300, 420.0000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.Morpholine)] = (-59.4000, -89.2400),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.Thiophene)] = (-46.0100, 597.8000),
		[(FunctionalMaingroup.Methanol, FunctionalMaingroup.AlkylSulfone)] = (150.0200, 265.7500),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.ArylAlcohol)] = (324.5000, -601.8000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.AlkylKetone)] = (-195.4000, 472.5000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.Aldehyde)] = (-116.0000, 480.8000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.AlkylEster)] = (72.8700, 200.8000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.Formate)] = (233.8700, 124.6300),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.AlkylEther)] = (540.5000, -314.7000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.AlkylPriAmine)] = (48.8900, -330.4000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.AlkylSecAmine)] = (168.0000, -448.2000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.AlkylTerAmine)] = (304.0000, -598.8000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.ArylPriAmine)] = (243.2000, -341.6000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.Pyridine)] = (459.0000, -332.9000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.AlkylCyanide)] = (112.6000, 242.8000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.CarboxylicAcid)] = (-14.0900, -66.1700),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.AlkylChloride)] = (325.4400, 698.2400),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.AlkylDichloride)] = (370.4000, 708.6900),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.AlkylTrichloride)] = (353.6800, 826.7600),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.AlkylTetrachloride)] = (497.5400, 1201.0000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.ArylChloride)] = (133.9000, -274.5000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.NitroAlkane)] = (220.6000, 417.9000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.NitroArene)] = (399.5000, 360.7000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.CarbonDisulfide)] = (887.1000, 1081.0000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.Furfural)] = (188.0260, 23.4840),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.EthyleneGlycol)] = (12.7200, -137.4000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.BromoAlkane)] = (777.1000, 79.1800),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.Dimethylsulfoxide)] = (-139.0000, -240.0000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.Acrylonitrile)] = (160.8000, 386.6000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.DialkylFormamide)] = (319.0000, -287.1000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.GenericEster)] = (-197.5000, 284.4000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.Silane)] = (-363.8000, 180.2000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.nMethylpyrrolidone)] = (-452.2000, 832.2000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.AlkylAmide)] = (835.6000, -509.3000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.AlkoxyAlkanol)] = (139.6000, -205.7000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.Morpholine)] = (407.9000, -384.3000),
		[(FunctionalMaingroup.Water, FunctionalMaingroup.AlkylSulfone)] = (-255.6300, 627.3900),
		[(FunctionalMaingroup.ArylAlcohol, FunctionalMaingroup.AlkylKetone)] = (-356.1000, -133.1000),
		[(FunctionalMaingroup.ArylAlcohol, FunctionalMaingroup.Aldehyde)] = (-271.1000, -155.6000),
		[(FunctionalMaingroup.ArylAlcohol, FunctionalMaingroup.AlkylEster)] = (-449.4000, -36.7200),
		[(FunctionalMaingroup.ArylAlcohol, FunctionalMaingroup.Formate)] = (-32.5200, -234.2500),
		[(FunctionalMaingroup.ArylAlcohol, FunctionalMaingroup.AlkylEther)] = (-162.8742, -178.5461),
		[(FunctionalMaingroup.ArylAlcohol, FunctionalMaingroup.AlkylPriAmine)] = (-832.9700, -870.8000),
		[(FunctionalMaingroup.ArylAlcohol, FunctionalMaingroup.ArylPriAmine)] = (119.9000, -253.1000),
		[(FunctionalMaingroup.ArylAlcohol, FunctionalMaingroup.Pyridine)] = (-305.5000, -341.6000),
		[(FunctionalMaingroup.ArylAlcohol, FunctionalMaingroup.CarboxylicAcid)] = (408.9000, -11.0000),
		[(FunctionalMaingroup.ArylAlcohol, FunctionalMaingroup.AlkylDichloride)] = (517.2700, 1633.5000),
		[(FunctionalMaingroup.ArylAlcohol, FunctionalMaingroup.AlkylTetrachloride)] = (1827.0000, 10000.0000),
		[(FunctionalMaingroup.ArylAlcohol, FunctionalMaingroup.ArylChloride)] = (6915.0000, 622.3000),
		[(FunctionalMaingroup.ArylAlcohol, FunctionalMaingroup.NitroArene)] = (-413.4800, 815.1200),
		[(FunctionalMaingroup.ArylAlcohol, FunctionalMaingroup.CarbonDisulfide)] = (8483.5000, 1421.3000),
		[(FunctionalMaingroup.ArylAlcohol, FunctionalMaingroup.EthyleneGlycol)] = (-687.1000, 838.4000),
		[(FunctionalMaingroup.ArylAlcohol, FunctionalMaingroup.GenericEster)] = (-494.2000, -167.3000),
		[(FunctionalMaingroup.ArylAlcohol, FunctionalMaingroup.nMethylpyrrolidone)] = (-659.0000, -234.7000),
		[(FunctionalMaingroup.ArylAlcohol, FunctionalMaingroup.Thiophene)] = (1005.0000, 810.5000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.Aldehyde)] = (-37.3600, 128.0000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.AlkylEster)] = (-213.7000, 372.2000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.Formate)] = (-190.4000, 385.4000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.AlkylEther)] = (-103.6000, 191.1000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.AlkylSecAmine)] = (-174.2000, 394.6000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.AlkylTerAmine)] = (-169.0000, 225.3000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.ArylPriAmine)] = (6201.0000, -450.3000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.Pyridine)] = (7.3410, 29.1000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.AlkylCyanide)] = (481.7000, -287.5000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.CarboxylicAcid)] = (669.4000, -297.8000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.AlkylChloride)] = (-191.6900, 286.2800),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.AlkylDichloride)] = (-130.3000, 82.8600),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.AlkylTrichloride)] = (-354.5500, 552.1000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.AlkylTetrachloride)] = (-39.2000, 372.0000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.ArylChloride)] = (-119.8000, 518.4000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.NitroAlkane)] = (137.5000, -142.6100),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.NitroArene)] = (548.5000, -101.5000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.CarbonDisulfide)] = (216.1380, 303.6570),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.AlkylThiol)] = (-46.2800, 160.6000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.Furfural)] = (-163.7000, 317.5000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.EthyleneGlycol)] = (71.4600, 135.4000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.IodoAlkane)] = (53.5900, 138.0000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.BromoAlkane)] = (245.2000, -142.6000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.AlkyneC)] = (-246.6000, 443.6150),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.Dimethylsulfoxide)] = (-44.5800, 110.4000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.Acrylonitrile)] = (-63.5000, 114.5500),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.ChloroAlkene)] = (-34.5700, -40.9000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.DialkylFormamide)] = (-61.7000, 97.0400),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.GenericEster)] = (-18.8000, 123.4000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.Silane)] = (-588.9000, 992.4000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.AlkoxyAlkanol)] = (37.5400, 156.4000),
		[(FunctionalMaingroup.AlkylKetone, FunctionalMaingroup.Thiophene)] = (-162.6000, 278.8000),
		[(FunctionalMaingroup.Aldehyde, FunctionalMaingroup.AlkylEster)] = (-110.3000, 185.1000),
		[(FunctionalMaingroup.Aldehyde, FunctionalMaingroup.Formate)] = (766.0000, -236.5000),
		[(FunctionalMaingroup.Aldehyde, FunctionalMaingroup.AlkylEther)] = (304.1000, -7.8380),
		[(FunctionalMaingroup.Aldehyde, FunctionalMaingroup.AlkylCyanide)] = (-106.4000, 224.6600),
		[(FunctionalMaingroup.Aldehyde, FunctionalMaingroup.CarboxylicAcid)] = (497.5000, -165.5000),
		[(FunctionalMaingroup.Aldehyde, FunctionalMaingroup.AlkylChloride)] = (751.9000, -47.5100),
		[(FunctionalMaingroup.Aldehyde, FunctionalMaingroup.AlkylDichloride)] = (67.5200, 190.6000),
		[(FunctionalMaingroup.Aldehyde, FunctionalMaingroup.AlkylTrichloride)] = (-483.7000, 242.8000),
		[(FunctionalMaingroup.Aldehyde, FunctionalMaingroup.IodoAlkane)] = (117.0000, 245.9000),
		[(FunctionalMaingroup.Aldehyde, FunctionalMaingroup.AlkyneC)] = (2.2100, -55.8700),
		[(FunctionalMaingroup.Aldehyde, FunctionalMaingroup.Acrylonitrile)] = (-339.2000, 354.0000),
		[(FunctionalMaingroup.Aldehyde, FunctionalMaingroup.ChloroAlkene)] = (172.4000, 183.8000),
		[(FunctionalMaingroup.Aldehyde, FunctionalMaingroup.DialkylFormamide)] = (-268.8000, 13.8900),
		[(FunctionalMaingroup.Aldehyde, FunctionalMaingroup.GenericEster)] = (-275.5000, 577.5000),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.Formate)] = (-241.8000, 1167.0000),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.AlkylEther)] = (-235.7000, 461.3000),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.AlkylSecAmine)] = (-73.5000, 136.0000),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.AlkylTerAmine)] = (-196.7000, 2888.6001),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.ArylPriAmine)] = (475.5000, -294.8000),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.Pyridine)] = (-0.1300, 8.8700),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.AlkylCyanide)] = (494.6000, -266.6000),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.CarboxylicAcid)] = (660.2000, -256.3000),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.AlkylChloride)] = (-34.7400, 35.3800),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.AlkylDichloride)] = (108.8500, -132.9500),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.AlkylTrichloride)] = (-209.6600, 176.4500),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.AlkylTetrachloride)] = (54.5700, 129.4900),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.ArylChloride)] = (442.4000, -171.1000),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.NitroAlkane)] = (-81.1300, 129.3000),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.CarbonDisulfide)] = (183.0460, 243.7750),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.Furfural)] = (202.2500, -146.3100),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.EthyleneGlycol)] = (-101.7000, 152.0000),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.IodoAlkane)] = (148.3000, 21.9200),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.BromoAlkane)] = (18.8800, 24.3700),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.AlkyneC)] = (71.4800, -111.4500),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.Dimethylsulfoxide)] = (52.0800, 41.5700),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.Acrylonitrile)] = (-28.6100, 175.5300),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.ChloroAlkene)] = (-275.2000, 611.3000),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.DialkylFormamide)] = (85.3300, -82.1200),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.GenericEster)] = (560.2000, -234.9000),
		[(FunctionalMaingroup.AlkylEster, FunctionalMaingroup.AlkoxyAlkanol)] = (151.8000, -3.4440),
		[(FunctionalMaingroup.Formate, FunctionalMaingroup.AlkylEther)] = (-234.0000, 457.3000),
		[(FunctionalMaingroup.Formate, FunctionalMaingroup.Pyridine)] = (-233.4000, 554.4000),
		[(FunctionalMaingroup.Formate, FunctionalMaingroup.AlkylCyanide)] = (-47.2500, 99.3700),
		[(FunctionalMaingroup.Formate, FunctionalMaingroup.CarboxylicAcid)] = (-268.1000, 193.9000),
		[(FunctionalMaingroup.Formate, FunctionalMaingroup.AlkylDichloride)] = (31.0000, 80.9900),
		[(FunctionalMaingroup.Formate, FunctionalMaingroup.AlkylTrichloride)] = (-126.2000, 235.6000),
		[(FunctionalMaingroup.Formate, FunctionalMaingroup.AlkylTetrachloride)] = (179.7000, 351.9000),
		[(FunctionalMaingroup.Formate, FunctionalMaingroup.ArylChloride)] = (24.2800, 383.3000),
		[(FunctionalMaingroup.Formate, FunctionalMaingroup.AlkylThiol)] = (103.9000, 201.5000),
		[(FunctionalMaingroup.Formate, FunctionalMaingroup.BromoAlkane)] = (298.1300, -92.2600),
		[(FunctionalMaingroup.Formate, FunctionalMaingroup.ChloroAlkene)] = (-11.4000, 134.5000),
		[(FunctionalMaingroup.Formate, FunctionalMaingroup.DialkylFormamide)] = (308.9000, -116.7000),
		[(FunctionalMaingroup.Formate, FunctionalMaingroup.GenericEster)] = (-122.3000, 145.4000),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.AlkylPriAmine)] = (-78.3600, 222.1000),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.AlkylSecAmine)] = (251.5000, -56.0800),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.AlkylTerAmine)] = (5422.2998, -194.1000),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.ArylPriAmine)] = (-46.3900, 285.3600),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.Pyridine)] = (213.2000, -156.1000),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.AlkylCyanide)] = (-18.5100, 38.8100),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.CarboxylicAcid)] = (664.6000, -338.5000),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.AlkylChloride)] = (301.1400, 225.3900),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.AlkylDichloride)] = (137.7700, -197.7100),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.AlkylTrichloride)] = (-154.3000, -20.9300),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.AlkylTetrachloride)] = (47.6700, 113.9000),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.ArylChloride)] = (134.8000, -25.1500),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.NitroAlkane)] = (95.1800, -94.4900),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.NitroArene)] = (155.1100, 220.6600),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.CarbonDisulfide)] = (140.8960, 112.3820),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.AlkylThiol)] = (-8.5380, 63.7100),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.Furfural)] = (170.1000, -87.3100),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.EthyleneGlycol)] = (-20.1100, 9.2070),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.IodoAlkane)] = (-149.5000, 476.6000),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.BromoAlkane)] = (-202.3000, 736.4000),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.AlkyneC)] = (-156.5700, 173.7700),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.Dimethylsulfoxide)] = (128.8000, -93.5100),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.ChloroAlkene)] = (240.2000, -217.9000),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.ArylFluoride)] = (-273.9500, 167.3000),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.DialkylFormamide)] = (254.8000, -158.2000),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.PerfluoroAlkane)] = (-172.5100, 278.1500),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.GenericEster)] = (417.0000, -247.8000),
		[(FunctionalMaingroup.AlkylEther, FunctionalMaingroup.Silane)] = (1338.0000, 448.5000),
		[(FunctionalMaingroup.AlkylPriAmine, FunctionalMaingroup.AlkylSecAmine)] = (-107.2000, 127.4000),
		[(FunctionalMaingroup.AlkylPriAmine, FunctionalMaingroup.AlkylTerAmine)] = (-41.1100, 38.8900),
		[(FunctionalMaingroup.AlkylPriAmine, FunctionalMaingroup.ArylPriAmine)] = (-200.7000, -15.0700),
		[(FunctionalMaingroup.AlkylPriAmine, FunctionalMaingroup.AlkylCyanide)] = (358.9000, -157.3000),
		[(FunctionalMaingroup.AlkylPriAmine, FunctionalMaingroup.AlkylChloride)] = (-82.9200, 131.2000),
		[(FunctionalMaingroup.AlkylPriAmine, FunctionalMaingroup.AlkylTetrachloride)] = (-99.8100, 261.1000),
		[(FunctionalMaingroup.AlkylPriAmine, FunctionalMaingroup.ArylChloride)] = (30.0500, 108.5000),
		[(FunctionalMaingroup.AlkylPriAmine, FunctionalMaingroup.AlkylThiol)] = (-70.1400, 106.7000),
		[(FunctionalMaingroup.AlkylPriAmine, FunctionalMaingroup.Dimethylsulfoxide)] = (874.1900, -366.5100),
		[(FunctionalMaingroup.AlkylPriAmine, FunctionalMaingroup.DialkylFormamide)] = (-164.0000, 49.7000),
		[(FunctionalMaingroup.AlkylPriAmine, FunctionalMaingroup.Silane)] = (-664.4000, 961.8000),
		[(FunctionalMaingroup.AlkylPriAmine, FunctionalMaingroup.Siloxane)] = (275.9000, -125.2000),
		[(FunctionalMaingroup.AlkylSecAmine, FunctionalMaingroup.AlkylTerAmine)] = (-189.2000, 865.9000),
		[(FunctionalMaingroup.AlkylSecAmine, FunctionalMaingroup.ArylPriAmine)] = (138.5400, 64.3000),
		[(FunctionalMaingroup.AlkylSecAmine, FunctionalMaingroup.Pyridine)] = (431.4900, -207.6600),
		[(FunctionalMaingroup.AlkylSecAmine, FunctionalMaingroup.AlkylCyanide)] = (147.1000, -108.5000),
		[(FunctionalMaingroup.AlkylSecAmine, FunctionalMaingroup.AlkylTetrachloride)] = (71.2300, 91.1300),
		[(FunctionalMaingroup.AlkylSecAmine, FunctionalMaingroup.ArylChloride)] = (-18.9300, 102.2000),
		[(FunctionalMaingroup.AlkylSecAmine, FunctionalMaingroup.EthyleneGlycol)] = (939.0700, -213.7400),
		[(FunctionalMaingroup.AlkylSecAmine, FunctionalMaingroup.ArylFluoride)] = (570.9000, -198.8000),
		[(FunctionalMaingroup.AlkylSecAmine, FunctionalMaingroup.DialkylFormamide)] = (-255.2200, 10.0300),
		[(FunctionalMaingroup.AlkylSecAmine, FunctionalMaingroup.GenericEster)] = (-38.7700, 284.5000),
		[(FunctionalMaingroup.AlkylSecAmine, FunctionalMaingroup.Silane)] = (448.1000, 1464.2000),
		[(FunctionalMaingroup.AlkylSecAmine, FunctionalMaingroup.Siloxane)] = (-1327.0000, 1603.8000),
		[(FunctionalMaingroup.AlkylTerAmine, FunctionalMaingroup.ArylPriAmine)] = (287.4300, -24.4600),
		[(FunctionalMaingroup.AlkylTerAmine, FunctionalMaingroup.AlkylCyanide)] = (1255.1000, -446.8600),
		[(FunctionalMaingroup.AlkylTerAmine, FunctionalMaingroup.AlkylChloride)] = (-182.9100, 151.3800),
		[(FunctionalMaingroup.AlkylTerAmine, FunctionalMaingroup.AlkylDichloride)] = (-73.8500, -141.4000),
		[(FunctionalMaingroup.AlkylTerAmine, FunctionalMaingroup.AlkylTrichloride)] = (-352.9000, -293.7000),
		[(FunctionalMaingroup.AlkylTerAmine, FunctionalMaingroup.AlkylTetrachloride)] = (-262.0000, 316.9000),
		[(FunctionalMaingroup.AlkylTerAmine, FunctionalMaingroup.ArylChloride)] = (-181.9000, 2951.0000),
		[(FunctionalMaingroup.AlkylTerAmine, FunctionalMaingroup.Dimethylsulfoxide)] = (243.1000, -257.2000),
		[(FunctionalMaingroup.AlkylTerAmine, FunctionalMaingroup.ArylFluoride)] = (-196.3120, 116.4780),
		[(FunctionalMaingroup.AlkylTerAmine, FunctionalMaingroup.DialkylFormamide)] = (22.0500, -185.2000),
		[(FunctionalMaingroup.ArylPriAmine, FunctionalMaingroup.Pyridine)] = (89.7000, 117.4000),
		[(FunctionalMaingroup.ArylPriAmine, FunctionalMaingroup.AlkylCyanide)] = (-281.6000, 777.4000),
		[(FunctionalMaingroup.ArylPriAmine, FunctionalMaingroup.CarboxylicAcid)] = (-396.0000, 493.8000),
		[(FunctionalMaingroup.ArylPriAmine, FunctionalMaingroup.AlkylChloride)] = (287.0000, 429.7000),
		[(FunctionalMaingroup.ArylPriAmine, FunctionalMaingroup.AlkylDichloride)] = (-111.0000, 140.8000),
		[(FunctionalMaingroup.ArylPriAmine, FunctionalMaingroup.AlkylTetrachloride)] = (882.0000, 898.2000),
		[(FunctionalMaingroup.ArylPriAmine, FunctionalMaingroup.ArylChloride)] = (617.5000, 334.9000),
		[(FunctionalMaingroup.ArylPriAmine, FunctionalMaingroup.NitroArene)] = (-139.3000, 134.9000),
		[(FunctionalMaingroup.ArylPriAmine, FunctionalMaingroup.EthyleneGlycol)] = (0.1004, 192.3000),
		[(FunctionalMaingroup.ArylPriAmine, FunctionalMaingroup.DialkylFormamide)] = (-334.4000, 343.7000),
		[(FunctionalMaingroup.ArylPriAmine, FunctionalMaingroup.GenericEster)] = (-89.4200, -22.1000),
		[(FunctionalMaingroup.Pyridine, FunctionalMaingroup.AlkylCyanide)] = (-169.6700, 134.2800),
		[(FunctionalMaingroup.Pyridine, FunctionalMaingroup.CarboxylicAcid)] = (-153.7000, -313.5000),
		[(FunctionalMaingroup.Pyridine, FunctionalMaingroup.AlkylDichloride)] = (-351.6000, 587.3000),
		[(FunctionalMaingroup.Pyridine, FunctionalMaingroup.AlkylTrichloride)] = (-114.7300, 18.9800),
		[(FunctionalMaingroup.Pyridine, FunctionalMaingroup.AlkylTetrachloride)] = (-205.3000, 368.5000),
		[(FunctionalMaingroup.Pyridine, FunctionalMaingroup.ArylChloride)] = (-2.1700, 20.1800),
		[(FunctionalMaingroup.Pyridine, FunctionalMaingroup.NitroArene)] = (2845.0000, 2475.0000),
		[(FunctionalMaingroup.Pyridine, FunctionalMaingroup.BromoAlkane)] = (-60.7800, -42.7100),
		[(FunctionalMaingroup.Pyridine, FunctionalMaingroup.ChloroAlkene)] = (160.7000, 281.6000),
		[(FunctionalMaingroup.Pyridine, FunctionalMaingroup.ArylFluoride)] = (-158.8000, 159.8000),
		[(FunctionalMaingroup.Pyridine, FunctionalMaingroup.Thiophene)] = (-136.6000, 221.4000),
		[(FunctionalMaingroup.AlkylCyanide, FunctionalMaingroup.CarboxylicAcid)] = (205.2700, 92.0700),
		[(FunctionalMaingroup.AlkylCyanide, FunctionalMaingroup.AlkylChloride)] = (4.9330, 54.3200),
		[(FunctionalMaingroup.AlkylCyanide, FunctionalMaingroup.AlkylDichloride)] = (-152.7000, 258.6000),
		[(FunctionalMaingroup.AlkylCyanide, FunctionalMaingroup.AlkylTrichloride)] = (-15.6200, 74.0400),
		[(FunctionalMaingroup.AlkylCyanide, FunctionalMaingroup.AlkylTetrachloride)] = (-54.8600, 491.9500),
		[(FunctionalMaingroup.AlkylCyanide, FunctionalMaingroup.ArylChloride)] = (-4.6240, 363.5000),
		[(FunctionalMaingroup.AlkylCyanide, FunctionalMaingroup.NitroAlkane)] = (-0.5150, 0.2830),
		[(FunctionalMaingroup.AlkylCyanide, FunctionalMaingroup.CarbonDisulfide)] = (230.8520, 335.7430),
		[(FunctionalMaingroup.AlkylCyanide, FunctionalMaingroup.AlkylThiol)] = (0.4604, 161.0000),
		[(FunctionalMaingroup.AlkylCyanide, FunctionalMaingroup.EthyleneGlycol)] = (177.5000, 169.6000),
		[(FunctionalMaingroup.AlkylCyanide, FunctionalMaingroup.BromoAlkane)] = (-62.1700, 136.9000),
		[(FunctionalMaingroup.AlkylCyanide, FunctionalMaingroup.AlkyneC)] = (-203.0200, 329.1200),
		[(FunctionalMaingroup.AlkylCyanide, FunctionalMaingroup.Acrylonitrile)] = (81.5700, -42.3100),
		[(FunctionalMaingroup.AlkylCyanide, FunctionalMaingroup.ChloroAlkene)] = (-55.7700, 335.2000),
		[(FunctionalMaingroup.AlkylCyanide, FunctionalMaingroup.DialkylFormamide)] = (-151.5000, 150.6000),
		[(FunctionalMaingroup.AlkylCyanide, FunctionalMaingroup.GenericEster)] = (120.3000, -61.6000),
		[(FunctionalMaingroup.AlkylCyanide, FunctionalMaingroup.AlkoxyAlkanol)] = (16.2300, 119.2000),
		[(FunctionalMaingroup.CarboxylicAcid, FunctionalMaingroup.AlkylChloride)] = (13.4100, 519.1000),
		[(FunctionalMaingroup.CarboxylicAcid, FunctionalMaingroup.AlkylDichloride)] = (-44.7000, 543.3000),
		[(FunctionalMaingroup.CarboxylicAcid, FunctionalMaingroup.AlkylTrichloride)] = (39.6300, 504.2000),
		[(FunctionalMaingroup.CarboxylicAcid, FunctionalMaingroup.AlkylTetrachloride)] = (183.4000, 631.0000),
		[(FunctionalMaingroup.CarboxylicAcid, FunctionalMaingroup.ArylChloride)] = (-79.0800, 993.4000),
		[(FunctionalMaingroup.CarboxylicAcid, FunctionalMaingroup.Furfural)] = (-208.9000, 570.6000),
		[(FunctionalMaingroup.CarboxylicAcid, FunctionalMaingroup.IodoAlkane)] = (228.4000, 616.6000),
		[(FunctionalMaingroup.CarboxylicAcid, FunctionalMaingroup.BromoAlkane)] = (-95.0000, 5256.0000),
		[(FunctionalMaingroup.CarboxylicAcid, FunctionalMaingroup.Dimethylsulfoxide)] = (-463.6000, -180.2000),
		[(FunctionalMaingroup.CarboxylicAcid, FunctionalMaingroup.ChloroAlkene)] = (-11.1600, 898.2000),
		[(FunctionalMaingroup.CarboxylicAcid, FunctionalMaingroup.DialkylFormamide)] = (-228.0000, -97.7700),
		[(FunctionalMaingroup.CarboxylicAcid, FunctionalMaingroup.GenericEster)] = (-337.0000, 1179.0000),
		[(FunctionalMaingroup.CarboxylicAcid, FunctionalMaingroup.AlkylAmide)] = (-322.3000, -70.2500),
		[(FunctionalMaingroup.AlkylChloride, FunctionalMaingroup.AlkylDichloride)] = (108.3100, -84.5300),
		[(FunctionalMaingroup.AlkylChloride, FunctionalMaingroup.AlkylTrichloride)] = (249.1500, -157.1000),
		[(FunctionalMaingroup.AlkylChloride, FunctionalMaingroup.AlkylTetrachloride)] = (62.4200, 11.8000),
		[(FunctionalMaingroup.AlkylChloride, FunctionalMaingroup.ArylChloride)] = (153.0000, -129.7000),
		[(FunctionalMaingroup.AlkylChloride, FunctionalMaingroup.NitroAlkane)] = (32.7300, 113.0000),
		[(FunctionalMaingroup.AlkylChloride, FunctionalMaingroup.NitroArene)] = (86.2000, 1971.0000),
		[(FunctionalMaingroup.AlkylChloride, FunctionalMaingroup.CarbonDisulfide)] = (450.0880, -73.0920),
		[(FunctionalMaingroup.AlkylChloride, FunctionalMaingroup.AlkylThiol)] = (59.0200, -27.9400),
		[(FunctionalMaingroup.AlkylChloride, FunctionalMaingroup.Furfural)] = (65.5600, -39.4600),
		[(FunctionalMaingroup.AlkylChloride, FunctionalMaingroup.IodoAlkane)] = (2.2200, 179.2500),
		[(FunctionalMaingroup.AlkylChloride, FunctionalMaingroup.BromoAlkane)] = (344.4000, -262.3000),
		[(FunctionalMaingroup.AlkylChloride, FunctionalMaingroup.ChloroAlkene)] = (-168.2000, 383.2000),
		[(FunctionalMaingroup.AlkylChloride, FunctionalMaingroup.DialkylFormamide)] = (6.5700, -55.2100),
		[(FunctionalMaingroup.AlkylChloride, FunctionalMaingroup.GenericEster)] = (63.6700, 182.2000),
		[(FunctionalMaingroup.AlkylDichloride, FunctionalMaingroup.AlkylTrichloride)] = (0.0000, 0.0000),
		[(FunctionalMaingroup.AlkylDichloride, FunctionalMaingroup.AlkylTetrachloride)] = (56.3300, 17.9700),
		[(FunctionalMaingroup.AlkylDichloride, FunctionalMaingroup.ArylChloride)] = (223.1000, -8.3090),
		[(FunctionalMaingroup.AlkylDichloride, FunctionalMaingroup.NitroAlkane)] = (108.9000, -9.6390),
		[(FunctionalMaingroup.AlkylDichloride, FunctionalMaingroup.Furfural)] = (149.5600, -116.2100),
		[(FunctionalMaingroup.AlkylDichloride, FunctionalMaingroup.IodoAlkane)] = (177.6000, -40.8200),
		[(FunctionalMaingroup.AlkylDichloride, FunctionalMaingroup.BromoAlkane)] = (315.9000, -174.5000),
		[(FunctionalMaingroup.AlkylDichloride, FunctionalMaingroup.Dimethylsulfoxide)] = (215.0000, -215.0000),
		[(FunctionalMaingroup.AlkylDichloride, FunctionalMaingroup.ChloroAlkene)] = (-91.8000, 301.9000),
		[(FunctionalMaingroup.AlkylDichloride, FunctionalMaingroup.DialkylFormamide)] = (-160.2800, 397.2400),
		[(FunctionalMaingroup.AlkylDichloride, FunctionalMaingroup.GenericEster)] = (-96.8700, 305.4000),
		[(FunctionalMaingroup.AlkylDichloride, FunctionalMaingroup.AlkoxyAlkanol)] = (361.1000, -194.7000),
		[(FunctionalMaingroup.AlkylTrichloride, FunctionalMaingroup.AlkylTetrachloride)] = (-30.1000, 51.9000),
		[(FunctionalMaingroup.AlkylTrichloride, FunctionalMaingroup.ArylChloride)] = (192.1000, -0.2266),
		[(FunctionalMaingroup.AlkylTrichloride, FunctionalMaingroup.CarbonDisulfide)] = (116.6120, -26.0580),
		[(FunctionalMaingroup.AlkylTrichloride, FunctionalMaingroup.Furfural)] = (-64.3800, 48.4840),
		[(FunctionalMaingroup.AlkylTrichloride, FunctionalMaingroup.IodoAlkane)] = (86.4000, 21.7600),
		[(FunctionalMaingroup.AlkylTrichloride, FunctionalMaingroup.BromoAlkane)] = (168.8000, -46.8000),
		[(FunctionalMaingroup.AlkylTrichloride, FunctionalMaingroup.Dimethylsulfoxide)] = (363.7000, -343.6000),
		[(FunctionalMaingroup.AlkylTrichloride, FunctionalMaingroup.ChloroAlkene)] = (111.2000, -149.8000),
		[(FunctionalMaingroup.AlkylTrichloride, FunctionalMaingroup.GenericEster)] = (255.8000, -193.0000),
		[(FunctionalMaingroup.AlkylTrichloride, FunctionalMaingroup.nMethylpyrrolidone)] = (-35.6800, -196.2000),
		[(FunctionalMaingroup.AlkylTrichloride, FunctionalMaingroup.AlkylSulfide)] = (565.9000, -363.1000),
		[(FunctionalMaingroup.AlkylTetrachloride, FunctionalMaingroup.ArylChloride)] = (-75.9700, 248.4000),
		[(FunctionalMaingroup.AlkylTetrachloride, FunctionalMaingroup.NitroAlkane)] = (490.8800, -34.6800),
		[(FunctionalMaingroup.AlkylTetrachloride, FunctionalMaingroup.NitroArene)] = (534.7000, 514.6000),
		[(FunctionalMaingroup.AlkylTetrachloride, FunctionalMaingroup.CarbonDisulfide)] = (132.2000, -60.7100),
		[(FunctionalMaingroup.AlkylTetrachloride, FunctionalMaingroup.Furfural)] = (546.6800, -133.1600),
		[(FunctionalMaingroup.AlkylTetrachloride, FunctionalMaingroup.IodoAlkane)] = (247.8000, 48.4900),
		[(FunctionalMaingroup.AlkylTetrachloride, FunctionalMaingroup.BromoAlkane)] = (146.6000, 77.5500),
		[(FunctionalMaingroup.AlkylTetrachloride, FunctionalMaingroup.Dimethylsulfoxide)] = (337.7000, -58.4300),
		[(FunctionalMaingroup.AlkylTetrachloride, FunctionalMaingroup.Acrylonitrile)] = (369.4900, -85.1480),
		[(FunctionalMaingroup.AlkylTetrachloride, FunctionalMaingroup.ChloroAlkene)] = (187.1000, -134.2000),
		[(FunctionalMaingroup.AlkylTetrachloride, FunctionalMaingroup.ArylFluoride)] = (215.2000, -124.6000),
		[(FunctionalMaingroup.AlkylTetrachloride, FunctionalMaingroup.DialkylFormamide)] = (498.6000, -186.7000),
		[(FunctionalMaingroup.AlkylTetrachloride, FunctionalMaingroup.GenericEster)] = (256.5000, 335.7000),
		[(FunctionalMaingroup.AlkylTetrachloride, FunctionalMaingroup.Siloxane)] = (233.1000, 70.8100),
		[(FunctionalMaingroup.AlkylTetrachloride, FunctionalMaingroup.AlkoxyAlkanol)] = (423.1000, 3.1630),
		[(FunctionalMaingroup.AlkylTetrachloride, FunctionalMaingroup.AlkylSulfide)] = (63.9500, -11.3000),
		[(FunctionalMaingroup.AlkylTetrachloride, FunctionalMaingroup.Thiophene)] = (108.5000, -79.3400),
		[(FunctionalMaingroup.AlkylTetrachloride, FunctionalMaingroup.AlkylSulfone)] = (585.1900, 75.0400),
		[(FunctionalMaingroup.ArylChloride, FunctionalMaingroup.NitroAlkane)] = (132.7000, 132.9000),
		[(FunctionalMaingroup.ArylChloride, FunctionalMaingroup.NitroArene)] = (2213.0000, -123.1000),
		[(FunctionalMaingroup.ArylChloride, FunctionalMaingroup.BromoAlkane)] = (593.4000, -185.3000),
		[(FunctionalMaingroup.ArylChloride, FunctionalMaingroup.Dimethylsulfoxide)] = (1337.3700, -334.1200),
		[(FunctionalMaingroup.ArylChloride, FunctionalMaingroup.DialkylFormamide)] = (5143.1401, -374.1600),
		[(FunctionalMaingroup.ArylChloride, FunctionalMaingroup.PerfluoroAlkane)] = (309.5800, 33.9500),
		[(FunctionalMaingroup.ArylChloride, FunctionalMaingroup.GenericEster)] = (-71.1800, 956.1000),
		[(FunctionalMaingroup.ArylChloride, FunctionalMaingroup.nMethylpyrrolidone)] = (-209.7000, 161.5000),
		[(FunctionalMaingroup.ArylChloride, FunctionalMaingroup.AlkoxyAlkanol)] = (434.1000, 7.0820),
		[(FunctionalMaingroup.NitroAlkane, FunctionalMaingroup.NitroArene)] = (533.2000, -85.1200),
		[(FunctionalMaingroup.NitroAlkane, FunctionalMaingroup.CarbonDisulfide)] = (320.2000, 277.8000),
		[(FunctionalMaingroup.NitroAlkane, FunctionalMaingroup.EthyleneGlycol)] = (139.8220, 481.3480),
		[(FunctionalMaingroup.NitroAlkane, FunctionalMaingroup.IodoAlkane)] = (304.3000, 64.2800),
		[(FunctionalMaingroup.NitroAlkane, FunctionalMaingroup.BromoAlkane)] = (10.1700, 125.3000),
		[(FunctionalMaingroup.NitroAlkane, FunctionalMaingroup.AlkyneC)] = (-27.7010, 174.4330),
		[(FunctionalMaingroup.NitroAlkane, FunctionalMaingroup.ChloroAlkene)] = (10.7600, 379.4000),
		[(FunctionalMaingroup.NitroAlkane, FunctionalMaingroup.DialkylFormamide)] = (-223.1000, 223.6000),
		[(FunctionalMaingroup.NitroAlkane, FunctionalMaingroup.GenericEster)] = (248.4000, -124.7000),
		[(FunctionalMaingroup.NitroAlkane, FunctionalMaingroup.ChloroFluoroAlkane)] = (-218.9000, 844.0000),
		[(FunctionalMaingroup.NitroAlkane, FunctionalMaingroup.Thiophene)] = (-4.5650, 176.3000),
		[(FunctionalMaingroup.NitroArene, FunctionalMaingroup.IodoAlkane)] = (2990.0000, 2448.0000),
		[(FunctionalMaingroup.NitroArene, FunctionalMaingroup.BromoAlkane)] = (-124.0000, 4288.0000),
		[(FunctionalMaingroup.CarbonDisulfide, FunctionalMaingroup.IodoAlkane)] = (292.7000, -27.4500),
		[(FunctionalMaingroup.CarbonDisulfide, FunctionalMaingroup.ChloroAlkene)] = (-47.3700, 167.9000),
		[(FunctionalMaingroup.CarbonDisulfide, FunctionalMaingroup.GenericEster)] = (469.8000, 885.5000),
		[(FunctionalMaingroup.AlkylThiol, FunctionalMaingroup.Dimethylsulfoxide)] = (31.6600, 85.7000),
		[(FunctionalMaingroup.AlkylThiol, FunctionalMaingroup.DialkylFormamide)] = (78.9200, -71.0000),
		[(FunctionalMaingroup.AlkylThiol, FunctionalMaingroup.nMethylpyrrolidone)] = (1004.2000, -274.1000),
		[(FunctionalMaingroup.AlkylThiol, FunctionalMaingroup.AlkylSulfide)] = (-18.2700, 6.9710),
		[(FunctionalMaingroup.Furfural, FunctionalMaingroup.GenericEster)] = (43.3700, -64.2800),
		[(FunctionalMaingroup.EthyleneGlycol, FunctionalMaingroup.Dimethylsulfoxide)] = (-417.2000, 535.8000),
		[(FunctionalMaingroup.EthyleneGlycol, FunctionalMaingroup.DialkylFormamide)] = (302.2000, -191.7000),
		[(FunctionalMaingroup.EthyleneGlycol, FunctionalMaingroup.GenericEster)] = (347.8000, -264.3000),
		[(FunctionalMaingroup.EthyleneGlycol, FunctionalMaingroup.nMethylpyrrolidone)] = (-262.0000, 262.0000),
		[(FunctionalMaingroup.EthyleneGlycol, FunctionalMaingroup.AlkoxyAlkanol)] = (-353.5000, 515.8000),
		[(FunctionalMaingroup.IodoAlkane, FunctionalMaingroup.BromoAlkane)] = (6.3700, 37.1000),
		[(FunctionalMaingroup.IodoAlkane, FunctionalMaingroup.GenericEster)] = (68.5500, 288.1000),
		[(FunctionalMaingroup.BromoAlkane, FunctionalMaingroup.Dimethylsulfoxide)] = (32.9000, -111.2000),
		[(FunctionalMaingroup.BromoAlkane, FunctionalMaingroup.ChloroAlkene)] = (-48.3300, 322.4200),
		[(FunctionalMaingroup.BromoAlkane, FunctionalMaingroup.DialkylFormamide)] = (336.2500, -176.2600),
		[(FunctionalMaingroup.BromoAlkane, FunctionalMaingroup.GenericEster)] = (-195.1000, 627.7000),
		[(FunctionalMaingroup.AlkyneC, FunctionalMaingroup.ChloroAlkene)] = (2073.2000, 631.5000),
		[(FunctionalMaingroup.AlkyneC, FunctionalMaingroup.DialkylFormamide)] = (-119.8000, 6.6990),
		[(FunctionalMaingroup.Dimethylsulfoxide, FunctionalMaingroup.DialkylFormamide)] = (-97.7100, 136.6000),
		[(FunctionalMaingroup.Dimethylsulfoxide, FunctionalMaingroup.GenericEster)] = (153.7000, -29.3400),
		[(FunctionalMaingroup.Acrylonitrile, FunctionalMaingroup.ChloroAlkene)] = (-208.8000, 837.2000),
		[(FunctionalMaingroup.Acrylonitrile, FunctionalMaingroup.DialkylFormamide)] = (-8.8040, 5.1500),
		[(FunctionalMaingroup.Acrylonitrile, FunctionalMaingroup.GenericEster)] = (423.4000, -53.9100),
		[(FunctionalMaingroup.ChloroAlkene, FunctionalMaingroup.DialkylFormamide)] = (255.0000, -137.7000),
		[(FunctionalMaingroup.ChloroAlkene, FunctionalMaingroup.GenericEster)] = (730.8000, -198.0000),
		[(FunctionalMaingroup.ChloroAlkene, FunctionalMaingroup.nMethylpyrrolidone)] = (26.3500, -66.3100),
		[(FunctionalMaingroup.ChloroAlkene, FunctionalMaingroup.AlkylSulfide)] = (2429.0000, 148.9000),
		[(FunctionalMaingroup.ArylFluoride, FunctionalMaingroup.DialkylFormamide)] = (-110.6500, 50.0600),
		[(FunctionalMaingroup.ArylFluoride, FunctionalMaingroup.PerfluoroAlkane)] = (-117.1700, 185.6000),
		[(FunctionalMaingroup.DialkylFormamide, FunctionalMaingroup.PerfluoroAlkane)] = (-5.5790, 55.8000),
		[(FunctionalMaingroup.DialkylFormamide, FunctionalMaingroup.GenericEster)] = (72.3100, -28.6500),
		[(FunctionalMaingroup.PerfluoroAlkane, FunctionalMaingroup.ChloroFluoroAlkane)] = (111.8000, -32.1700),
		[(FunctionalMaingroup.GenericEster, FunctionalMaingroup.AlkoxyAlkanol)] = (122.4000, 101.2000),
		[(FunctionalMaingroup.Silane, FunctionalMaingroup.Siloxane)] = (-2166.0000, 745.3000),
		[(FunctionalMaingroup.Imidazole, FunctionalMaingroup.Bistriflimide)] = (1517.5000, -1869.9000),
	};

	private double GetFGInteractionParameter(FunctionalMaingroup FG1, FunctionalMaingroup FG2)
	{
		if (FG1 == FG2) return 0;
		if (MaingroupInteractionParameters.ContainsKey((FG1, FG2))) return MaingroupInteractionParameters[(FG1, FG2)].aij;
		if (MaingroupInteractionParameters.ContainsKey((FG2, FG1))) return MaingroupInteractionParameters[(FG2, FG1)].aji;
		
		throw new KeyNotFoundException($"UNIFAC functional maingroup interaction parameter is not available for {FG1}, {FG2}.");
	}

	/// <summary>
	/// Coordination parameter for activity calculations.
	/// Generally set to 10 since this is the coordination of a sphere in hexagonal close packing structure.
	/// </summary>
	private int z = 10;

	#endregion

	#region Precalculations

	private Dictionary<FunctionalSubgroup, double> SubgroupMoleFractions = [];
	private Dictionary<FunctionalSubgroup, double> SubgroupThetas = [];
	private Dictionary<Chemical, double> SpeciesQs = [];
	private Dictionary<Chemical, double> SpeciesRs = [];
	private Dictionary<Chemical, double> SpeciesThetas = [];
	private Dictionary<Chemical, double> SpeciesPhis = [];
	private Dictionary<Chemical, double> SpeciesLs = [];
	private List<FunctionalSubgroup> FunctionalSubgroupsInMixture = [];

	/// <summary>
	/// Validates that all species in the speciesList can be modeled with this activity model.
	/// </summary>
	/// <exception cref="KeyNotFoundException">Thrown when a species in the speciesList is not in the ChemicalSubgroupMap.</exception>
	private void ValidateSpeciesInList()
	{
		if (ChemicalSubgroupMap.Keys.Intersect(speciesList.Select(spec => spec.chemical)).Count() == speciesList.Count) return;

		// This code is only reached when there are fewer elements in the intersection than in the speciesList.
		// That is, some chemicals in the speciesList were not present in the ChemicalSubgroupMap.
		// Find which chemicals those are and thrown an error listing them out.
		string missingSpecies = "";
		foreach(var item in speciesList)
		{
			if (!ChemicalSubgroupMap.ContainsKey(item.chemical))
			{
				var speciesName = Constants.ChemicalNames[item.chemical];
				var addString = missingSpecies == "" ? speciesName : $", {speciesName}";
				missingSpecies += addString;
				
			}
		}
		throw new KeyNotFoundException($"The following chemicals cannot be modeled using the UNIFAC activity model:\n{missingSpecies}.");
	}

	/// <summary>
	/// Lists out all functional subgroups present in a given chemical.
	/// </summary>
	private List<FunctionalSubgroup> GetAllFunctionalSubgroupsInSpecies(Chemical species)
	{
		List<FunctionalSubgroup> subgroups = [];
		foreach (var (subgroup, _) in ChemicalSubgroupMap[species])
		{
			if (subgroups.Contains(subgroup)) continue;
			else subgroups.Add(subgroup);
		}
		return subgroups;
	}

	/// <summary>
	/// Lists out all functional subgroups present in the mixture.
	/// </summary>
	private void GetAllFunctionalSubgroupsInMixture()
	{
		var list = speciesList.SelectMany(item => GetAllFunctionalSubgroupsInSpecies(item.chemical)).Distinct().ToList();
		FunctionalSubgroupsInMixture = list;
	}

	/// <summary>
	/// Gets the index in speciesList which represents the given chemical.
	/// </summary>
	int GetMixtureSpeciesIdx(Chemical species)
	{
		for (int i = 0; i < speciesList.Count; i++)
		{
			if (speciesList[i].chemical == species) return i;
		}

		throw new KeyNotFoundException($"{Constants.ChemicalNames[species]} not found in speciesList.");
	}

	/// <summary>
	/// Precalculates the mole fraction of all subgroups in mixture, X_m.
	/// Essentially breaks down all species in mixture into subgroups and considers only subgroups.
	/// </summary>
	private void CalculateSubgroupMoleFractions()
	{
		double totalSubgroupMoles = 0;
		foreach (var item in speciesList)
		{
			var species_i = item.chemical;
			var x_i = item.speciesMoleFraction;
			foreach (var (_, nu_k) in ChemicalSubgroupMap[species_i])
			{
				totalSubgroupMoles += x_i * nu_k;
			}
		}

		Dictionary<FunctionalSubgroup, double> subgroupMoleFractions = [];
		foreach (var item in speciesList)
		{
			var species_i = item.chemical;
			var x_i = item.speciesMoleFraction;
			foreach (var (subgroup_k, nu_k) in ChemicalSubgroupMap[species_i])
			{
				var value = x_i * nu_k / totalSubgroupMoles;
				if (subgroupMoleFractions.ContainsKey(subgroup_k)) subgroupMoleFractions[subgroup_k] += value;
				else subgroupMoleFractions.Add(subgroup_k, value);
			}
		}
		SubgroupMoleFractions = subgroupMoleFractions;
	}

	/// <summary>
	/// Precalculates the Q-fraction (theta) of all subgroup in mixture, Θ_m.
	/// Essentially breaks down all species in mixture into subgroups and considers only subgroups.
	/// </summary>
	private void CalculateSubgroupThetas()
	{
		double totalSubgroupQ = 0;
		foreach (var item in SubgroupMoleFractions)
		{
			var X_i = item.Value;
			var Q_i = SubgroupRQParameters[item.Key].Q;
			totalSubgroupQ += X_i * Q_i;
		}

		Dictionary<FunctionalSubgroup, double> subgroupThetas = [];
		foreach (var item in SubgroupMoleFractions)
		{
			var X_i = item.Value;
			var Q_i = SubgroupRQParameters[item.Key].Q;
			subgroupThetas[item.Key] = X_i * Q_i / totalSubgroupQ;
		}
		SubgroupThetas = subgroupThetas;
	}

	/// <summary>
	/// Precalculates all derived parameters (q, r, θ, φ, l) for all species in mixture.
	/// </summary>
	private void CalculateSpeciesDerivedParameters()
	{
		// Calculate r and q parameters.
		// Total r and q parameters for use in θ and φ calculations later.
		double qSum = 0;
		double rSum = 0;
		foreach (var item in speciesList)
		{
			var listSG = ChemicalSubgroupMap[item.chemical];
			double r = 0;
			double q = 0;
			foreach (var (subgroup, nu) in listSG)
			{
				r += nu * SubgroupRQParameters[subgroup].R;
				q += nu * SubgroupRQParameters[subgroup].Q;
			}
			SpeciesRs.Add(item.chemical, r);
			SpeciesQs.Add(item.chemical, q);
			rSum += item.speciesMoleFraction * r;
			qSum += item.speciesMoleFraction * q;
		}

		// Calculate θ and φ parameters.
		foreach (var item in speciesList)
		{
			var x = item.speciesMoleFraction;
			var r = SpeciesRs[item.chemical];
			var q = SpeciesQs[item.chemical];
			SpeciesPhis.Add(item.chemical, x * r / rSum);
			SpeciesThetas.Add(item.chemical, x * q / qSum);
		}

		// Calculate l parameters.
		foreach (var item in speciesList)
		{
			var r = SpeciesRs[item.chemical];
			var q = SpeciesQs[item.chemical];
			SpeciesLs.Add(item.chemical, z / 2 * (r - q) - (r - 1));
		}
	}

	/// <summary>
	/// Runs all precalculations. This should be run before any external-facing method
	/// to ensure up-to-date speciesList is used.
	/// </summary>
	private void RunPrecalculations()
	{
		ClearPrecalculations();
		ValidateSpeciesInList();
		GetAllFunctionalSubgroupsInMixture();
		CalculateSubgroupMoleFractions();
		CalculateSubgroupThetas();
		CalculateSpeciesDerivedParameters();
	}

	/// <summary>
	/// Clears all precalculation output fields.
	/// </summary>
	private void ClearPrecalculations()
	{
		SubgroupMoleFractions = [];
		SubgroupThetas = [];
		SpeciesQs = [];
		SpeciesRs = [];
		SpeciesThetas = [];
		SpeciesPhis = [];
		SpeciesLs = [];
	}

	#endregion

	public override double SpeciesActivityCoefficient(Chemical species, Temperature T, Pressure P)
	{
		// Calculates all para-static variables (i.e., precalculations that only rely on the speciesList).
		RunPrecalculations();

		//var taskGammaC = Task.Run(() => LogSpeciesActivityCoefficientCombinatorial(species));
		//var taskGammaR = Task.Run(() => LogSpeciesActivityCoefficientResidual(species, T));
		//Task.WaitAll(taskGammaC, taskGammaR);
		//return Math.Exp(taskGammaC.Result + taskGammaR.Result);

		var gammaC = LogSpeciesActivityCoefficientCombinatorial(species);
		var gammaR = LogSpeciesActivityCoefficientResidual(species, T);
		return Math.Exp(gammaC + gammaR);
	}

	private double LogSpeciesActivityCoefficientCombinatorial(Chemical species)
	{
		var x = speciesList[GetMixtureSpeciesIdx(species)].speciesMoleFraction;
		var q = SpeciesQs[species];
		var theta = SpeciesThetas[species];
		var phi = SpeciesPhis[species];
		var L = SpeciesLs[species];

		var termSpecies = Math.Log(phi / x) + z * q / 2 * Math.Log(theta / phi) + L;

		double termSum = 0;
		foreach (var item in speciesList)
		{
			var xj = item.speciesMoleFraction;
			var Lj = SpeciesLs[item.chemical];
			termSum += xj * Lj;
		}

		return termSpecies - phi / x * termSum;
	}

	private double LogSpeciesActivityCoefficientResidual(Chemical species, Temperature T)
	{
		double sum = 0;
		foreach (var (subgroup_k, nu_ki) in ChemicalSubgroupMap[species])
		{
			//var taskGammaK = Task.Run(() => LogGammaK(subgroup_k));
			//var taskGammaKI = Task.Run(() => LogGammaKI(subgroup_k));
			//sum += nu_ki * (taskGammaK.Result - taskGammaKI.Result);

			var logGamma_k = LogGammaK(subgroup_k);
			var logGamma_ki = LogGammaKI(subgroup_k);
			sum += nu_ki * (logGamma_k - logGamma_ki);
		}
		return sum;


		// subgroup k activity coefficient, Γ_k
		double LogGammaK(FunctionalSubgroup sg_k)
		{
			var Q_k = SubgroupRQParameters[sg_k].Q;

			// Calculate σ_m = Σn[Θ_n * Ψ_mn] for each m-subgroup in mixture
			Dictionary<FunctionalSubgroup, double> listDenoms = [];
			foreach (var sg_m in FunctionalSubgroupsInMixture)
			{
				foreach (var sg_n in FunctionalSubgroupsInMixture)
				{
					var value = SubgroupThetas[sg_n] * GroupInteractionEnergy(T, sg_n, sg_m);
					if (listDenoms.ContainsKey(sg_m)) listDenoms[sg_m] += value;
					else listDenoms.Add(sg_m, value);
				}
			}

			// Get all that summation stuff done
			double sumLn = 0;
			double sumFr = 0;
			foreach (var sg_m in FunctionalSubgroupsInMixture)
			{
				sumLn += GroupInteractionEnergy(T, sg_m, sg_k) * SubgroupThetas[sg_m];
				sumFr += GroupInteractionEnergy(T, sg_k, sg_m) * SubgroupThetas[sg_m] / listDenoms[sg_m];
			}

			return Q_k * (1 - Math.Log(sumLn) - sumFr);
		}

		// reference subgroup k activity coefficient, Γ_k(i)
		// Ignores all other species other than species i.
		double LogGammaKI(FunctionalSubgroup sg_k)
		{
			var listSG = GetAllFunctionalSubgroupsInSpecies(species);
			var subgroupThetas = CalculateSubgroupThetas();
			var Q_k = SubgroupRQParameters[sg_k].Q;

			// Calculate σ_m = Σn[Θ_n * Ψ_mn] for each m-subgroup in mixture
			Dictionary<FunctionalSubgroup, double> listDenoms = [];
			foreach (var sg_m in listSG)
			{
				foreach (var sg_n in listSG)
				{
					var psi_nm = GroupInteractionEnergy(T, sg_n, sg_m);
					var theta_n = subgroupThetas[sg_n];
					var value = theta_n * psi_nm;
					if (listDenoms.ContainsKey(sg_m)) listDenoms[sg_m] += value;
					else listDenoms.Add(sg_m, value);
				}
			}

			// Get all that summation stuff done
			double sumLn = 0;
			double sumFr = 0;
			foreach (var sg_m in listSG)
			{
				sumLn += GroupInteractionEnergy(T, sg_m, sg_k) * subgroupThetas[sg_m];
				sumFr += GroupInteractionEnergy(T, sg_k, sg_m) * subgroupThetas[sg_m] / listDenoms[sg_m];
			}

			return Q_k * (1 - Math.Log(sumLn) - sumFr);

			// Overrides standard CalculateSubgroupThetas() to use in reference calculation.
			Dictionary<FunctionalSubgroup, double> CalculateSubgroupThetas()
			{
				var subgroupMoleFractions = CalculateSubgroupMoleFractions();
				double totalSubgroupQ = 0;
				foreach (var item in subgroupMoleFractions)
				{
					var X_i = item.Value;
					var Q_i = SubgroupRQParameters[item.Key].Q;
					totalSubgroupQ += X_i * Q_i;
				}

				Dictionary<FunctionalSubgroup, double> subgroupThetas = [];
				foreach (var item in subgroupMoleFractions)
				{
					var X_i = item.Value;
					var Q_i = SubgroupRQParameters[item.Key].Q;
					subgroupThetas[item.Key] = X_i * Q_i / totalSubgroupQ;
				}
				return subgroupThetas;
			}

			// Overrides standard CalculateSubgroupFractions() to use in reference calculation.
			Dictionary<FunctionalSubgroup, double> CalculateSubgroupMoleFractions()
			{
				double totalSubgroupMoles = 0;
				foreach (var (_, nu_k) in ChemicalSubgroupMap[species])
				{
					totalSubgroupMoles += nu_k;
				}

				Dictionary<FunctionalSubgroup, double> subgroupMoleFractions = [];
				foreach (var (subgroup_k, nu_k) in ChemicalSubgroupMap[species])
				{
					subgroupMoleFractions.Add(subgroup_k, nu_k / totalSubgroupMoles);
				}
				return subgroupMoleFractions;
			}
		}
	}

	/// <summary>
	/// Calculates the interaction energy between two functional maingroups m and n, Ψ_mn.
	/// </summary>
	/// <returns>Technically not an energy as this is a unitless number, hence the use of 'double'.</returns>
	private double GroupInteractionEnergy(Temperature T, FunctionalMaingroup FG1, FunctionalMaingroup FG2)
	{
		return Math.Exp(-1 / T * GetFGInteractionParameter(FG1, FG2));
	}

	/// <inheritdoc cref="GroupInteractionEnergy(Temperature, FunctionalMaingroup, FunctionalMaingroup)"/>
	private double GroupInteractionEnergy(Temperature T, FunctionalSubgroup SG1, FunctionalSubgroup SG2)
	{
		return GroupInteractionEnergy(T, MaingroupSubgroupMap[SG1], MaingroupSubgroupMap[SG2]);
	}
}
