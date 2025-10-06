using Core.Reactions;
using Core.Reactions.Kinetics;
using Core.VariableTypes;
using Spectre.Console;

namespace Core.Console;

static class TestElementaryRateLaw
{
	public static void Test(string dirConsole)
	{
		Temperature T = 500;
		Pressure P = 101325;

		List<RxnSpecies> speciesListA =
		[
			new(Chemical.Oxygen, 1, "vapor", true),
			new(Chemical.Hydrogen, 2, "vapor", true),
			new(Chemical.Water, 2, "vapor", false)
		];
		var rxnA = new Reaction(speciesListA, new ElementaryRateLawFactory(10, 8e3));

		List<RxnSpecies> speciesListB =
		[
			new(Chemical.Water, 2, "vapor", true),
			new(Chemical.Chlorine, 2, "vapor", true),
			new(Chemical.HydrogenChloride, 4, "vapor", false),
			new(Chemical.Oxygen, 2, "vapor", false)
		];
		var rxnB = new Reaction(speciesListB, new ElementaryRateLawFactory(10, 8e3));

		double dt = 0.01;
		double maxtime = 10;
		MolarityVector cVec = new()
		{
			[Chemical.Oxygen] = 1,
			[Chemical.Hydrogen] = 2,
			[Chemical.Water] = 0,
			[Chemical.Chlorine] = 1,
			[Chemical.HydrogenChloride] = 0
		};

		string filename = "ElementaryRateLaw.csv";
		File.Delete(filename);
		StreamWriter file = new(Path.Combine(dirConsole, filename), false);
		file.WriteLine("t,O2,H2,H2O,Cl2,HCl");
		file.WriteLine($"0,{cVec[Chemical.Oxygen]},{cVec[Chemical.Hydrogen]}," +
			$"{cVec[Chemical.Water]},{cVec[Chemical.Chlorine]},{cVec[Chemical.HydrogenChloride]}");
		AnsiConsole.WriteLine($"Initial concentrations:\n" +
			$"Oxygen... {cVec[Chemical.Oxygen]} mol/L\n" +
			$"Hydrogen... {cVec[Chemical.Hydrogen]} mol/L\n" +
			$"Water... {cVec[Chemical.Water]} mol/L\n" +
			$"Chlorine... {cVec[Chemical.Chlorine]} mol/L\n" +
			$"Hydrogen chloride... {cVec[Chemical.HydrogenChloride]} mol/L");
		for (int i = 1; i * dt < maxtime; i++)
		{
			var changesA = rxnA.rateLaw.SpeciesReactionRates(T, P, cVec);
			var changesB = rxnB.rateLaw.SpeciesReactionRates(T, P, cVec);
			cVec[Chemical.Oxygen] += (changesA[Chemical.Oxygen] + changesB[Chemical.Oxygen]) * dt;
			cVec[Chemical.Hydrogen] += (changesA[Chemical.Hydrogen] + changesB[Chemical.Hydrogen]) * dt;
			cVec[Chemical.Water] += (changesA[Chemical.Water] + changesB[Chemical.Water]) * dt;
			cVec[Chemical.Chlorine] += (changesA[Chemical.Chlorine] + changesB[Chemical.Chlorine]) * dt;
			cVec[Chemical.HydrogenChloride] += (changesA[Chemical.HydrogenChloride] + changesB[Chemical.HydrogenChloride]) * dt;
			file.WriteLine($"{i * dt},{cVec[Chemical.Oxygen]},{cVec[Chemical.Hydrogen]}," +
				$"{cVec[Chemical.Water]},{cVec[Chemical.Chlorine]},{cVec[Chemical.HydrogenChloride]}");
			AnsiConsole.WriteLine($"{i * dt},{cVec[Chemical.Oxygen]},{cVec[Chemical.Hydrogen]}," +
				$"{cVec[Chemical.Water]},{cVec[Chemical.Chlorine]},{cVec[Chemical.HydrogenChloride]}");
		}
		file.Close();
		AnsiConsole.WriteLine($"Wrote to file: {filename}.");
	}
}
