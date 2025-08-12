using Core;
using Core.EquationsOfState;
using Core.VariableTypes;
using Core.ViewModels;
using System;
using System.Collections.Generic;
using ThermodynamicistUWP.Dialogs;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace ThermodynamicistUWP
{
	public sealed partial class PageRxnKin : Page
	{
		public PageRxnViewModel ViewModel { get; } = new PageRxnViewModel();

		public PageRxnKin()
		{
			InitializeComponent();
		}

		private void ButtonAddSpecies_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.AddItem( new ControlRxnSpeciesViewModel
			{
				Chemical = Chemical.Acetone,
				EoSFactory = new PengRobinsonEOSFactory(),
				Stoich = 1,
				Phase = "",
				IsReactant = true,
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

			// Get species list from ControlRxnSpecies
			var rxn = new Reaction(ViewModel.GetRxnSpeciesList());

			// Reactions testing. TEMPORARY TODO
			var rxnSpecies = new List<RxnSpecies>
			{
				new RxnSpecies(Chemical.Benzene, 1, "vapor", true),
				new RxnSpecies(Chemical.Chlorine, 1, "vapor", true),
				new RxnSpecies(Chemical.Chlorobenzene, 1, "liquid", false),
				new RxnSpecies(Chemical.HydrogenChloride, 1, "vapor", false)
			};
			var reactionA = new Reaction(rxnSpecies);

			UpdateData(reactionA, T, P);
		}

		/// <summary>
		/// Runs calculations, updates DataLabel with outputs.
		/// </summary>
		private void UpdateData(Reaction rxn, Temperature T, Pressure P)
		{
			try
			{
				var dHrxn = rxn.MolarEnthalpyOfReaction(T, P);

				DataLabel.Text = "Molar enthalpy of reaction: " + dHrxn.ToEngrNotation(5);
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
