using Core;
using Core.EquationsOfState;
using Microsoft.UI.Xaml.Controls;
using OxyPlot.Series;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace ThermodynamicistUWP
{
	public sealed partial class ControlRxnSpecies : UserControl
	{
		RxnSpeciesListItem rxnSpecies;

		public ControlRxnSpecies()
		{
			InitializeComponent();
			rxnSpecies = new RxnSpeciesListItem(Chemical.Methane, new PengRobinsonEOS(Chemical.Methane), 2, "vapor", false);

			// Initializes chemical list in species dropdown
			foreach (Chemical chemical in Enum.GetValues(typeof(Chemical)))
			{
				DropdownSpecies.Items.Add(Constants.ChemicalNames[chemical]);
			}
			// Set default species
			DropdownSpecies.SelectedItem = "Methane";

			// Initializes equation of state list in EoS dropdown
			DropdownEoS.Items.Add("van der Waals");
			DropdownEoS.Items.Add("Peng-Robinson");
			DropdownEoS.Items.Add("modified Solid-Liquid-Vapor");

			// Set default EoS
			DropdownEoS.SelectedValue = "Peng-Robinson";

			if (DataContext == null) return;
			(DataContext as RxnSpeciesListItem).chemical = rxnSpecies.chemical;
			(DataContext as RxnSpeciesListItem).EoS = rxnSpecies.EoS;
			(DataContext as RxnSpeciesListItem).stoich = rxnSpecies.stoich;
			(DataContext as RxnSpeciesListItem).phase = rxnSpecies.phase;
			(DataContext as RxnSpeciesListItem).IsReactant = rxnSpecies.IsReactant;
		}

		private void RefreshCalculations(object sender, RoutedEventArgs e)
		{
			// If any inputs are not set, do not attempt to run calculations!
			if (
				double.IsNaN(NumBoxStoich.Value) ||
				DropdownSpecies.SelectedItem == null ||
				DropdownEoS.SelectedItem == null ||
				DropdownPhase.SelectedItem == null
				) return;

			var stoich = (int) NumBoxStoich.Value;

			Chemical species = Constants.ChemicalNames.FirstOrDefault(
				x => x.Value == DropdownSpecies.SelectedValue.ToString()).Key;

			string phase = DropdownPhase.SelectedValue.ToString();

			(DataContext as RxnSpeciesListItem).chemical = species;
			(DataContext as RxnSpeciesListItem).stoich = stoich;
			(DataContext as RxnSpeciesListItem).phase = phase;
		}

		private void RadioButton_Checked(object sender, RoutedEventArgs e)
		{
			if (sender.Equals(RadioProduct)) (DataContext as RxnSpeciesListItem).IsReactant = false;
			else (DataContext as RxnSpeciesListItem).IsReactant = true;
		}

		private void NumBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
		{
			RefreshCalculations(sender, null);
		}

		private void DropdownEoS_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (DropdownEoS.SelectedValue == null) return;
			switch (DropdownEoS.SelectedValue.ToString())
			{
				case "van der Waals":
					rxnSpecies.EoS = new VanDerWaalsEOS(rxnSpecies.chemical);
					break;
				case "Peng-Robinson":
					rxnSpecies.EoS = new PengRobinsonEOS(rxnSpecies.chemical);
					break;
				case "modified Solid-Liquid-Vapor":
					rxnSpecies.EoS = new ModSolidLiquidVaporEOS(rxnSpecies.chemical);
					break;
				default:
					rxnSpecies.EoS = new PengRobinsonEOS(rxnSpecies.chemical);
					break;
			}
			if (DataContext != null) (DataContext as RxnSpeciesListItem).EoS = rxnSpecies.EoS;

			DropdownPhase.Items.Clear();
			foreach (var phase in rxnSpecies.EoS.ModeledPhases) DropdownPhase.Items.Add(phase);
		}

		private void ButtonRemoveSpecies_Click(object sender, RoutedEventArgs e)
		{
			// This seems like awful code, too dependent on the specific structure of the PageRxnKin UI design.
			// TODO: come up with a robust way of finding the PageRxnKin instance.

			var repeater = this.Parent as ItemsRepeater;
			var grid = repeater.Parent as Grid;
			var page = grid.Parent as PageRxnKin;
			page.RemoveSpecies((DataContext as RxnSpeciesListItem));
		}
	}
}
