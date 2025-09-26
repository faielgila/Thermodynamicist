using Core;
using Core.Multicomponent;
using Core.Multicomponent.ActivityModels;
using Core.VariableTypes;
using Spectre.Console;
using System.Threading.Tasks;

namespace Core.Console;

static class TestBinaryPhaseDiagram
{
	public static void Test(string dirConsole)
	{
		var species0 = Chemical.NPropanol;
		var species1 = Chemical.Water;
		Pressure P = 101325;

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

		LinearEnumerable temps = new(360, 372, 0.1);
		string filename = $"P{P}-phases.csv";
		File.Delete(filename);
		StreamWriter file = new(Path.Combine(dirConsole, filename), false);
		file.WriteLine("T,xV,xL");
		foreach (Temperature T in temps)
		{
			var header = new Rule($"[white on darkblue]Testing for multiphase equilibria @ T={T}[/]");
			header.RuleStyle(new(foreground: Color.White, background: Color.DarkBlue));
			header.Justify(Justify.Left);
			AnsiConsole.Write(header);

			var search = system.FindPhaseEquilibria(T, P);
			foreach (var item in search)
			{
				var undict = item.Value.Select(x => (x.Key.phase, Constants.ChemicalNames[x.Key.species], x.Value));
				var first = undict.ElementAt(0);
				var last = undict.ElementAt(2);
				AnsiConsole.MarkupLine($"[white on darkgreen]Equilibrium found:[/]");
				AnsiConsole.MarkupLine($"[white on darkgreen]{first.phase} phase, {first.Value}mol% {first.Item2}[/]");
				AnsiConsole.MarkupLine($"[white on darkgreen]{last.phase} phase, {last.Value}mol% {last.Item2}[/]");

				file.WriteLine($"{T},{first.Value},{last.Value}");
			}
		}
		file.Close();
		AnsiConsole.WriteLine($"Wrote to file: {filename}.");
	}
}
