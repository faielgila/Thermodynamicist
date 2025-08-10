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
		}

		private void ButtonAddSpecies_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.AddItem( new ControlMixtureSpeciesViewModel()
			{
				Chemical = Chemical.Water,
				EoSFactory = new PengRobinsonEOSFactory(),
				SpeciesMoleFraction = 1,
				ModeledPhase = "liquid",
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
				double.IsNaN(NumBoxP.Value) || NumBoxP.Value == 0
				)
			{
				ErrorDialog.ShowErrorDialog("Not all required inputs are set.");
				return;
			}

			var T = new Temperature(NumBoxT.Value);
			var P = new Pressure(NumBoxP.Value);

			// Get species list from ControlMixtureSpecies
			//var speciesList = ViewModel.GetMixtureSpeciesList();
			//var homogeneousMixture = new HomogeneousMixture(speciesList, "liquid", ViewModel.ActivityModelFactory.Create(speciesList));

			// Mixture testing. TEMPORARY TODO
			var mixtureSpecies = new List<MixtureSpecies>
			{
				new MixtureSpecies(Chemical.Acetone, 0.047, "liquid"),
				new MixtureSpecies(Chemical.NPentane, 1-0.047, "liquid")
			};
			var mixtureA = new HomogeneousMixture(mixtureSpecies, "liquid", new UNIFACActivityModel(mixtureSpecies));

			UpdateData(mixtureA, T, P);
		}

		/// <summary>
		/// Runs calculations, updates DataLabel with outputs.
		/// </summary>
		private void UpdateData(HomogeneousMixture mixture, Temperature T, Pressure P)
		{
			try
			{
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
