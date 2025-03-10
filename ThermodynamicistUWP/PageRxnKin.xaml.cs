using Core;
using Core.EquationsOfState;
using Core.VariableTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace ThermodynamicistUWP
{
	public sealed partial class PageRxnKin : Page
	{
		public ObservableCollection<RxnSpeciesListItem> RxnSpeciesItems;

		public PageRxnKin()
		{
			InitializeComponent();

			if (RxnSpeciesItems == null)
			{
				RxnSpeciesItems = new ObservableCollection<RxnSpeciesListItem>();
			}
		}

		private void ButtonAddSpecies_Click(object sender, RoutedEventArgs e)
		{
			RxnSpeciesItems.Add(new RxnSpeciesListItem());
		}

		public void RemoveSpecies(RxnSpeciesListItem rxnSpeciesListItem)
		{
			RxnSpeciesItems.Remove(rxnSpeciesListItem);
		}

		/// <summary>
		/// Checks inputs and packages into core objects.
		/// </summary>
		private void RunCalc(object sender, RoutedEventArgs e)
		{
			// If any inputs are not set, do not attempt to run calculations!
			if (
				double.IsNaN(NumBoxT.Value) || NumBoxT.Value == 0 ||
				double.IsNaN(NumBoxP.Value) || NumBoxP.Value == 0
				) return;

			var T = new Temperature(NumBoxT.Value);
			var P = new Pressure(NumBoxP.Value);

			// Get species list from ControlRxnSpecies
			//var rxn = new Reaction(RxnSpeciesItems.ToList());

			// Reactions testing. TEMPORARY TODO
			var rxnSpecies = new List<RxnSpeciesListItem>
			{
				new RxnSpeciesListItem(Chemical.Methane, 1, "vapor", true),
				new RxnSpeciesListItem(Chemical.Oxygen, 2, "vapor", true),
				new RxnSpeciesListItem(Chemical.CarbonDioxide, 1, "vapor", false),
				new RxnSpeciesListItem(Chemical.Water, 2, "liquid", false)
			};
			var reactionA = new Reaction(rxnSpecies);

			UpdateData(reactionA, T, P);
		}

		/// <summary>
		/// Runs calculations, updates DataLabel with outputs.
		/// </summary>
		private void UpdateData(Reaction rxn, Temperature T, Pressure P)
		{
			var dHrxn = rxn.MolarEnthalpyOfReaction(T, P);

			DataLabel.Text = "Molar enthalpy of reaction: " + dHrxn.ToEngrNotation(5);
		}

		private void NumBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
		{
			RunCalc(sender, null);
		}
	}
}
