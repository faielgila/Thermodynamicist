using Core.Multicomponent;
using Core.Multicomponent.ActivityModels;
using Core;
using Spectre.Console;
using Core.VariableTypes;

static class TestHomogeneousMixture
{
	public static void Test(string dirConsole)
	{

		Temperature T = 307;
		Pressure P = 101325;
		LinearEnumerable compositions = new(0.05, 1, 0.05);
		Chemical species0 = Chemical.Acetone;
		Chemical species1 = Chemical.NPentane;

		var table = new Table();
		var spec0Name = Constants.ChemicalNames[species0];
		var spec1Name = Constants.ChemicalNames[species1];
		table.Title = new TableTitle($"Homogeneous mixture: {spec0Name} + {spec1Name} @ {T}K, {P}Pa\nliquid phase (UNIFAC)");
		table.AddColumn($"mol% {spec0Name}");
		table.AddColumn($"activity coefficient\n{spec0Name}");
		table.AddColumn($"activity coefficient\n{spec1Name}");
		table.AddColumn($"chemical potential\n{spec0Name}");
		table.AddColumn($"chemical potential\n{spec1Name}");
		table.AddColumn("total Gibbs energy");

		string filename_L = "testUNIFAC";
		StreamWriter generateCSV_L = new(Path.Combine(dirConsole, $"{filename_L}.csv"), false);
		generateCSV_L.WriteLine($"composition,activity {spec0Name},activity {spec1Name},potential {spec0Name},potential {spec1Name},total Gibbs energy");
		generateCSV_L.Dispose();
		StreamWriter outputCSV_L = new(Path.Combine(dirConsole, $"{filename_L}.csv"), true);

		string filename_V = "testIdeal";
		StreamWriter generateCSV_V = new(Path.Combine(dirConsole, $"{filename_V}.csv"), false);
		generateCSV_V.WriteLine($"composition,activity {spec0Name},activity {spec1Name},potential {spec0Name},potential {spec1Name},total Gibbs energy");
		generateCSV_V.Dispose();
		StreamWriter outputCSV_V = new(Path.Combine(dirConsole, $"{filename_V}.csv"), true);

		AnsiConsole.Progress().Start(ctx =>
		{
			var taskL = ctx.AddTask("[green]Calculating liquid phase...[/]", maxValue: (1-.05)/.05);
			var taskV = ctx.AddTask("[green]Calculating vapor phase...[/]", maxValue: (1-.05)/.05);


			List<string> csvRecord_L = [];
			List<string> csvRecord_V = [];
			foreach (var x in compositions)
			{
				// Calculate liquid (UNIFAC)
				var mixtureSpecies = new List<MixtureSpecies>() {
					new(species0, x, "liquid"),
					new(species1, 1-x, "liquid")
				};
				var modelL = new UNIFACActivityModel(mixtureSpecies);
				var homomix = new HomogeneousMixture(mixtureSpecies, "liquid", modelL, null);
				var activity0 = homomix.activityModel.SpeciesActivityCoefficient(species0, T, P).ToString();
				var activity1 = homomix.activityModel.SpeciesActivityCoefficient(species1, T, P).ToString();
				var mu0 = homomix.SpeciesChemicalPotential(T, P, species0).ToString();
				var mu1 = homomix.SpeciesChemicalPotential(T, P, species1).ToString();
				var totalG = homomix.TotalMolarGibbsEnergy(T, P).ToString();
				csvRecord_L.Add(x.ToString());
				csvRecord_L.Add(activity0);
				csvRecord_L.Add(activity1);
				csvRecord_L.Add(mu0);
				csvRecord_L.Add(mu1);
				csvRecord_L.Add(totalG);

				table.AddRow([x.ToString(), activity0, activity1, mu0, mu1, totalG]);
				outputCSV_L.WriteLine($"{x},{activity0},{activity1},{mu0},{mu1},{totalG}");
				taskL.Increment(1); ctx.Refresh();

				// Calculate vapor (ideal)
				mixtureSpecies = new List<MixtureSpecies>() {
					new(species0, x, "vapor"),
					new(species1, 1-x, "vapor")
				};
				var modelV = new IdealMixture("vapor", mixtureSpecies);
				homomix = new HomogeneousMixture(mixtureSpecies, "vapor", modelV, null);
				activity0 = homomix.activityModel.SpeciesActivityCoefficient(species0, T, P).ToString();
				activity1 = homomix.activityModel.SpeciesActivityCoefficient(species1, T, P).ToString();
				mu0 = homomix.SpeciesChemicalPotential(T, P, species0).ToString();
				mu1 = homomix.SpeciesChemicalPotential(T, P, species1).ToString();
				totalG = homomix.TotalMolarGibbsEnergy(T, P).ToString();
				csvRecord_V.Add(x.ToString());
				csvRecord_V.Add(activity0);
				csvRecord_V.Add(activity1);
				csvRecord_V.Add(mu0);
				csvRecord_V.Add(mu1);
				csvRecord_V.Add(totalG);

				outputCSV_V.WriteLine($"{x},{activity0},{activity1},{mu0},{mu1},{totalG}");
				taskV.Increment(1); ctx.Refresh();
			}
		});
		AnsiConsole.Write(table);
		outputCSV_L.Dispose();
		outputCSV_V.Dispose();

	}
}