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

		LinearEnumerable temps = new(360, 372, 0.2);
		string filenameDiag = $"P{P}-phases.csv";
		File.Delete(filenameDiag);
		StreamWriter fileDiag = new(Path.Combine(dirConsole, filenameDiag), false);
		fileDiag.WriteLine("T,xV,xL");

		string filenameErr = $"P{P}-error-E.csv";
		File.Delete(filenameErr);
		StreamWriter fileErr = new(Path.Combine(dirConsole, filenameErr), false);

		string filenameT = $"P{P}-error-T.csv";
		File.Delete(filenameT);
		StreamWriter fileT = new(Path.Combine(dirConsole, filenameT), false);

		string filenamexV = $"P{P}-error-xV.csv";
		File.Delete(filenamexV);
		StreamWriter filexV = new(Path.Combine(dirConsole, filenamexV), false);

		foreach (Temperature T in temps)
		{
			var header = new Rule($"[white on darkblue]Testing for multiphase equilibria @ T={T}[/]");
			header.RuleStyle(new(foreground: Color.White, background: Color.DarkBlue));
			header.Justify(Justify.Left);
			AnsiConsole.Write(header);

			var search = system.FindPhaseEquilibria(T, P);

			var errors = system.LastPhaseEquilibriaErrors.ToList();
			var orderedErrors = errors.OrderBy(entry => entry.Key);
			foreach (var entry in orderedErrors)
			{
				fileT.Write($"{T},");
				filexV.Write($"{entry.Key},");
				fileErr.Write($"{entry.Value},");
			}
			fileT.WriteLine();
			filexV.WriteLine();
			fileErr.WriteLine();

			foreach (var item in search)
			{
				var undict = item.Value.Select(x => (x.Key.phase, Constants.ChemicalNames[x.Key.species], x.Value));
				var first = undict.ElementAt(0);
				var last = undict.ElementAt(2);
				AnsiConsole.MarkupLine($"[white on darkgreen]Equilibrium found:[/]");
				AnsiConsole.MarkupLine($"[white on darkgreen]{first.phase} phase, {first.Value}mol% {first.Item2}[/]");
				AnsiConsole.MarkupLine($"[white on darkgreen]{last.phase} phase, {last.Value}mol% {last.Item2}[/]");

				fileDiag.WriteLine($"{T},{first.Value},{last.Value}");
			}
		}
		fileDiag.Close();
		fileErr.Close();
		fileT.Close();
		filexV.Close();
		AnsiConsole.WriteLine($"Wrote to file: {filenameDiag}.");
	}
}
