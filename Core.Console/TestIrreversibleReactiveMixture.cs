using Core.Multicomponent;
using Core.Multicomponent.ActivityModels;
using Core.Reactions;
using Core.Reactions.Kinetics;
using Core.VariableTypes;
using Spectre.Console;

namespace Core.Console;

static class TestIrreversibleReactiveMixture
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
		List<RxnSpecies> speciesListB =
		[
			new(Chemical.Water, 2, "vapor", true),
			new(Chemical.Chlorine, 2, "vapor", true),
			new(Chemical.HydrogenChloride, 4, "vapor", false),
			new(Chemical.Oxygen, 1, "vapor", false)
		];
		var rxnA = new Reaction(speciesListA, new ElementaryRateLawFactory(), 10, 8e3);
		var rxnB = new Reaction(speciesListB, new ElementaryRateLawFactory(), 10, 8e3);
		var rxnSet = new List<Reaction>() { rxnA, rxnB };

		List<ReactiveMixtureSpecies> rxnMixtureSpecies = [
			new(Chemical.Oxygen, 1, "vapor"),
			new(Chemical.Hydrogen, 2, "vapor"),
			new(Chemical.Water, 0, "vapor"),
			new(Chemical.Chlorine, 1, "vapor"),
			new(Chemical.HydrogenChloride, 0, "vapor")
		];
		var mixSpecies = ReactiveMixtureSpecies.ConvertToMixtureSpeciesList(rxnMixtureSpecies);
		var activityModel = new IdealMixture("vapor", mixSpecies);

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
		var sys = new SimpleIrreversibleReactiveMixture(rxnSet, rxnMixtureSpecies, "vapor", activityModel);
		sys.SetConcentrations(cVec);

		string filename = "SimpleIrreversibleReactiveMixture.csv";
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
			sys.ForwardTick(T, P, dt);
			cVec = sys.GetConcentrations();
			var str = $"{i * dt},{cVec[Chemical.Oxygen]},{cVec[Chemical.Hydrogen]}," +
				$"{cVec[Chemical.Water]},{cVec[Chemical.Chlorine]},{cVec[Chemical.HydrogenChloride]}";
			file.WriteLine(str);
			AnsiConsole.WriteLine(str);

			var rates = sys.ReactionRates(T, P);
			AnsiConsole.WriteLine($"{i * dt}, rxn A rate {rates[0]}, rxn B rate {rates[1]}");
		}

		file.Close();
		AnsiConsole.WriteLine($"Wrote to file: {filename}.");
	}
}
