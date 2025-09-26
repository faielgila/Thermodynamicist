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
		Temperature T = 361.8;
		Pressure P = 101325;
		var species0 = Chemical.NPropanol;
		var species1 = Chemical.Water;

		var systemSpeciesList = new Dictionary<Chemical, MoleFraction>()
		{
			[species0] = 0.5,
			[species1] = 0.5
		};
		var systemCompVec = new CompositionVector(systemSpeciesList);
		var vaporSpeciesList = new List<MixtureSpecies>() {
			new(species0, 0.5, "vapor"),
			new(species1, 0.5, "vapor")
		};
		var liquidSpeciesList = new List<MixtureSpecies>() {
			new(species0, 0.5, "liquid"),
			new(species1, 0.5, "liquid")
		};
		var mixtureList = new List<HomogeneousMixture>() {
			new(vaporSpeciesList, "vapor", new IdealMixture("vapor", vaporSpeciesList), null),
			new(liquidSpeciesList, "liquid", new UNIFACActivityModel(liquidSpeciesList), null)
		};
		var system = new MultiphaseSystem(systemCompVec, mixtureList, Chemical.Water);

		var equilibria = system.FindPhaseEquilibria(T, P);

		foreach (var entry in system.ConvertChemicalPotentialCurvesToCSV())
		{
			string filename = $"T365-P{P}-{entry.Key.phase}-{entry.Key.species}.csv";
			File.Delete(filename);
			StreamWriter file = new(Path.Combine(dirConsole, filename), false);
			file.WriteLine(entry.Value);
			file.Close();
			AnsiConsole.WriteLine($"Wrote to file: {filename}.");
		}

		foreach (var entry in system.ConvertTotalGibbsEnergyCurvesToCSV())
		{
			string filename = $"T365-P{P}-{entry.Key.phase}.csv";
			File.Delete(filename);
			StreamWriter file = new(Path.Combine(dirConsole, filename), false);
			file.WriteLine(entry.Value);
			file.Close();
			AnsiConsole.WriteLine($"Wrote to file: {filename}.");
		}

		string filenameErr = $"T365-P{P}-errors.csv";
		File.Delete(filenameErr);
		StreamWriter fileErr = new(Path.Combine(dirConsole, filenameErr), false);
		fileErr.WriteLine("xV,error");
		var errors = system.LastPhaseEquilibriaErrors.ToList();
		foreach (var entry in errors.OrderBy(entry => entry.Key))
		{
			fileErr.WriteLine($"{entry.Key},{entry.Value}");
		}
		fileErr.Close();
		AnsiConsole.WriteLine($"Wrote to file: {filenameErr}.");

		var search = system.FindPhaseEquilibria(T, P);
		foreach (var item in search)
		{
			var undict = item.Value.Select(x => (x.Key.phase, Constants.ChemicalNames[x.Key.species], x.Value.RoundToSigfigs(2)));
			var first = undict.First();
			AnsiConsole.MarkupLine($"[green]Equilibrium found: {first.phase} phase, {first.Item3}mol% {first.Item2}[/]");
		}
		AnsiConsole.MarkupLine($"[bold][springgreen1]Found {search.Count} equilibria @ T={T}[/][/]");
	}
}
