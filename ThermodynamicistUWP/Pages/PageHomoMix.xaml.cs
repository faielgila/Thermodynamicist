using Core;
using Core.EquationsOfState;
using Core.Multicomponent;
using Core.Multicomponent.ActivityModels;
using Core.VariableTypes;
using Core.ViewModels;
using System;
using System.Collections.Generic;
using ThermodynamicistUWP.Converters;
using ThermodynamicistUWP.Dialogs;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace ThermodynamicistUWP
{
	public sealed partial class PageHomoMix : Page
	{
		public PageHomoMixViewModel ViewModel { get; } = new PageHomoMixViewModel();

		public PageHomoMix()
		{
			InitializeComponent();

			// Initializes activity model list in ActivityModel dropdown.
			// Note the use of ActivityModelFactory instead of the ActivityModel object directly.
			DropdownModel.Items.Add(new UNIFACActivityModelFactory());
		}

		private void ButtonAddSpecies_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.AddItem(new ControlMixtureSpeciesViewModel()
			{
				Chemical = Chemical.Acetone,
				EoSFactory = new PengRobinsonEOSFactory(),
				SpeciesMoleFraction = 0,
				ModeledPhase = "",
				DeleteCommand = ViewModel.DeleteCommand
			});
		}

		/// <summary>
		/// Checks inputs and packages into Core objects.
		/// </summary>
		private void RunCalc(object sender, RoutedEventArgs e)
		{
			// If any inputs are not set, do not attempt to run calculations!
			if (
				double.IsNaN(NumBoxT.Value) || NumBoxT.Value == 0 ||
				double.IsNaN(NumBoxP.Value) || NumBoxP.Value == 0 ||
				ViewModel.Items == null || ViewModel.Items.Count == 0 ||
				DropdownModel.SelectedItem == null
				)
			{
				ErrorDialog.ShowErrorDialog("Not all required inputs are set.");
				return;
			}
			var T = new Temperature(NumBoxT.Value);
			var P = new Pressure(NumBoxP.Value);

			foreach (var viewModel in ViewModel.Items)
			{
				if (!viewModel.CheckValidInput())
				{
					ErrorDialog.ShowErrorDialog("Mixture species contains invalid inputs.");
					return;
				}
			}

			// Calculate mole fraction total to ensure sum is 1.
			double sum = 0;
			foreach ( var item in ViewModel.Items)
			{
				sum += item.SpeciesMoleFraction;
			}
			if ( sum != 1)
			{
				ErrorDialog.ShowErrorDialog("Species mole fractions does not sum to 1.");
				return;
			}

			// If any chemical are added multiple times in the speciesList, do not run calculations.
			List<Chemical> chemicals = new List<Chemical>();
			foreach ( var item in ViewModel.Items)
			{
				if (chemicals.Contains(item.Chemical))
				{
					ErrorDialog.ShowErrorDialog("Duplicate chemical(s) found in species list. Remove duplicates and try again.");
					return;
				} else
				{
					chemicals.Add(item.Chemical);
				}
			}

			// Get species list from ControlMixtureSpecies.
			var speciesList = ViewModel.GetMixtureSpeciesList();
			var model = ViewModel.ActivityModelFactory.Create(ViewModel.GetMixtureSpeciesList());
			var homogeneousMixture = new HomogeneousMixture(speciesList, "liquid", model);

			// Mixture testing. TEMPORARY TODO
			//var mixtureSpecies = new List<MixtureSpecies>
			//{
			//	new MixtureSpecies(Chemical.Acetone, 0.047, "liquid"),
			//	new MixtureSpecies(Chemical.NPentane, 1-0.047, "liquid")
			//};
			//var homogeneousMixture = new HomogeneousMixture(mixtureSpecies, "liquid", new UNIFACActivityModel(mixtureSpecies));

			UpdateData(homogeneousMixture, T, P);
		}

		/// <summary>
		/// Runs calculations, updates DataLabel with outputs.
		/// </summary>
		private void UpdateData(HomogeneousMixture mixture, Temperature T, Pressure P)
		{
			try
			{
				DataLabel.Text = "";
				foreach (var ms in mixture.speciesList)
				{
					var name = Constants.ChemicalNames[ms.chemical];
					var activity = mixture.activityModel.SpeciesActivityCoefficient(ms.chemical, T);
					DataLabel.Text += "\nActivity coefficient, " + name + ": " + activity.ToEngrNotation(5);
				}
			} catch (Exception e)
			{
				ErrorDialog.ShowErrorDialog(e);
			}
		}

		private void NumBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
		{
			//RunCalc(sender, null);
		}
	}
}
