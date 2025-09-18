using Core;
using Core.Multicomponent;
using Core.Multicomponent.ActivityModels;
using Core.VariableTypes;
using Spectre.Console;
using System.Threading.Tasks;

static class TestBinaryMultiphaseSystem
{
	public static void Test(string dirConsole)
	{
		Temperature T = 365;
		Pressure P = 101325;

		var systemSpeciesList = new Dictionary<Chemical, MoleFraction>()
		{
			[Chemical.NPropanol] = 0.5,
			[Chemical.Water] = 0.5
		};
		var vaporSpeciesList = new List<MixtureSpecies>() {
			new(Chemical.NPropanol, 0.5, "vapor"),
			new(Chemical.Water, 0.5, "vapor")
		};
		var liquidSpeciesList = new List<MixtureSpecies>() {
			new(Chemical.NPropanol, 0.5, "liquid"),
			new(Chemical.Water, 0.5, "liquid")
		};
		var mixtureList = new List<HomogeneousMixture>() {
			new(vaporSpeciesList, "vapor", new IdealMixture("vapor", vaporSpeciesList), null),
			new(liquidSpeciesList, "liquid", new UNIFACActivityModel(liquidSpeciesList), null)
		};
		var system = new MultiphaseSystem(systemSpeciesList, mixtureList);

		var equilibria = system.FindPhaseEquilibria(T, P);

		foreach (var entry in system.ConvertChemicalPotentialCurvesToCSV())
		{
			string filename = $"T{T}-P{P}-{entry.Key.phase}-{entry.Key.species}.csv";
			File.Delete(filename);
			StreamWriter file = new(Path.Combine(dirConsole, filename), false);
			file.WriteLine(entry.Value);
			file.Close();
			AnsiConsole.WriteLine($"Wrote to file: {filename}.");
		}

		foreach (var entry in system.ConvertTotalGibbsEnergyCurvesToCSV())
		{
			string filename = $"T{T}-P{P}-{entry.Key.phase}.csv";
			File.Delete(filename);
			StreamWriter file = new(Path.Combine(dirConsole, filename), false);
			file.WriteLine(entry.Value);
			file.Close();
			AnsiConsole.WriteLine($"Wrote to file: {filename}.");
		}
	}
}
