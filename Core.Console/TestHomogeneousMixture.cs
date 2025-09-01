using Core.Multicomponent;
using Core.Multicomponent.ActivityModels;
using Core;
using CsvHelper;
using System.Globalization;
using Spectre.Console;


Console.WriteLine("Starting Thermodynamicist.Core testing console...\n\n");
var dirConsole = Environment.CurrentDirectory;
Path.GetDirectoryName(dirConsole);

var table = new Table();
table.Title = new TableTitle("Homogeneous mixture: acetone + n-pentane @ 298K");
table.AddColumn("mol% acetone");
table.AddColumn("activity coefficient\nacetone");
table.AddColumn("activity coefficient\npentane");

double[] compositions = [.021, .061, .134, .2105, .292, .405, .503, .611, .728, .869, .953];

var records = new List<HomogeneousMixtureActivityCoefficientRecord>();
foreach (var x in compositions)
{
	var mixtureSpecies = new List<MixtureSpecies>() {
		new(Chemical.Acetone, x, "liquid"),
		new(Chemical.NPentane, 1-x, "liquid")
	};
	var model = new UNIFACActivityModel(mixtureSpecies);
	var homomix = new HomogeneousMixture(mixtureSpecies, "liquid", model, null);
	var record = new HomogeneousMixtureActivityCoefficientRecord
	{
		composition = x,
		species1 = homomix.activityModel.SpeciesActivityCoefficient(Chemical.Acetone, 298),
		species2 = homomix.activityModel.SpeciesActivityCoefficient(Chemical.NPentane, 298)
	};

	records.Add(record);

	var activityAcetone = record.species1.ToString().Remove(4);
	var activityPentane = record.species2.ToString().Remove(4);

	table.AddRow([x.ToString(), activityAcetone, activityPentane]);
}
AnsiConsole.Write(table);

using (var writer = new StreamWriter("Y:\\Repos\\faielgila\\Thermodynamicist\\Core.Console\\csv\\test.csv"))
using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
{
	csv.WriteRecords(records);
}


public class HomogeneousMixtureActivityCoefficientRecord
{
	public double composition { get; set; }
	public double species1 { get; set; }
	public double species2 { get; set; }
}