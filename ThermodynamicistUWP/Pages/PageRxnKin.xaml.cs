using Core;
using Core.Reactions;
using Core.Reactions.Kinetics;
using Core.VariableTypes;
using Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ThermodynamicistUWP.Dialogs;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;


namespace ThermodynamicistUWP
{
	public sealed partial class PageRxnKin : Page
	{
		public PageRxnViewModel ViewModel { get; } = new PageRxnViewModel();

		private ObservableCollection<OutputItem> AllOutputOptions { get; set; }

		public PageRxnKin()
		{
			// Initialize all numeric inputs.
			ViewModel.T = 298;
			ViewModel.P = 101325;
			ViewModel.FrequencyFactor = double.NaN;
			ViewModel.ActivationEnergy = double.NaN;

			InitializeComponent();

			// Initializes rate law list.
			// Note the use of RateLawFactory instead of the RateLaw object directly.
			DropdownRateLaw.Items.Add(new ElementaryRateLawFactory());

			// Set output selection options.
			AllOutputOptions = GenerateOutputItems();
			ViewModel.AvailableOutputOptions = AllOutputOptions;
			ViewModel.SelectedOutputOptions = new ObservableCollection<OutputItem>();
		}

		/// <summary>
		/// Checks inputs and packages into Core objects.
		/// </summary>
		private void RunCalc(object sender, RoutedEventArgs e)
		{
			if (CheckInvalidPageInputs()) return;

			// Validate RxnSpecies inputs.
			var chemicals = new List<Chemical>();
			foreach (var viewModel in ViewModel.Items)
			{
				if (!viewModel.CheckValidInput())
				{
					ErrorDialog.ShowErrorDialog("Reaction species contains invalid inputs.");
					return;
				}
				if (chemicals.Contains(viewModel.Chemical))
				{
					ErrorDialog.ShowErrorDialog($"Chemical '{Constants.ChemicalNames[viewModel.Chemical]}' has multiple entries." +
						$"\nBalance the reaction equation and try again.");
					return;
				}
				else
				{
					chemicals.Add(viewModel.Chemical);
				}
			}

			Temperature T = ViewModel.T;
			Pressure P = ViewModel.P;
			double frequencyFactor = ViewModel.FrequencyFactor;
			GibbsEnergy activationEnergy = ViewModel.ActivationEnergy;
			var rateLawFactory = ViewModel.RateLawFactory;

			// Get species list from ControlRxnSpecies
			var rxn = new Reaction(ViewModel.GetRxnSpeciesList(), rateLawFactory, frequencyFactor, activationEnergy);

			UpdateData(rxn, T, P);
		}

		/// <summary>
		/// Validates all user inputs.
		/// </summary>
		/// <returns>true if any inputs are invalid.</returns>
		private bool CheckInvalidPageInputs()
		{
			bool cancelCalc = false;

			// If no outputs are selected, warn user.
			if (ViewModel.SelectedOutputOptions.Count == 0)
			{
				ErrorDialog.ShowErrorDialog("No outputs are selected. Click the 'settings' icon and then 'Run calculation'.");
				return true;
			}

			// If any inputs are not set, do not attempt to run calculations!
			if (
				ViewModel.RateLawFactory == null ||
				double.IsNaN(ViewModel.T) || ViewModel.T == 0 ||
				double.IsNaN(ViewModel.P) || ViewModel.P == 0 ||
				double.IsNaN(ViewModel.FrequencyFactor) ||
				double.IsNaN(ViewModel.ActivationEnergy)
				)
			{
				//ErrorDialog.ShowErrorDialog("Not all required inputs are set.");
				cancelCalc = true;
			}

			if (ViewModel.RateLawFactory == null)
			{
				//ErrorDialog.ShowErrorDialog("Rate law must be selected.");
				cancelCalc = true;
			}

			return cancelCalc;
		}

		/// <summary>
		/// Runs calculations, updates DataLabel with outputs.
		/// </summary>
		private void UpdateData(Reaction rxn, Temperature T, Pressure P)
		{
			// Reset PlotView visibility in case plot output option isn't selected.
			PlotViewKin.Visibility = Visibility.Collapsed;

			// Get list of all selected output options.
			var selectedOutputs = ViewModel.SelectedOutputOptions;
			List<string> outputStrings = new List<string>();
			try
			{
				foreach (var item in selectedOutputs)
				{
					if (item.Type == OutputItem.ItemType.Folder) continue;

					switch (item.OutputName)
					{
						default:
							break;
						case "MolarEntropyOfReaction":
							var S = rxn.MolarEntropyOfReaction(T, P);
							outputStrings.Add(item.DisplayFormat(S.ToEngrNotation(5)));
							break;
						case "MolarEnthalpyOfReaction":
							var H = rxn.MolarEnthalpyOfReaction(T, P);
							outputStrings.Add(item.DisplayFormat(H.ToEngrNotation(5)));
							break;
						case "MolarGibbsEnergyOfReaction":
							var G = rxn.MolarGibbsEnergyOfReaction(T, P);
							outputStrings.Add(item.DisplayFormat(G.ToEngrNotation(5)));
							break;

						case "PlotMolarityTranscience":
							PlotViewKin.Model = new RxnKineticsPlotModel(rxn, T, P, 0.01, 10).Model;
							PlotViewKin.Background = new SolidColorBrush(Colors.White);
							PlotViewKin.Visibility = Visibility.Visible;
							break;
					}
				}

				// Combine all output strings into DataLabel.
				DataLabel.Text = outputStrings.First();
				outputStrings.Remove(outputStrings.First());
				foreach (var item in outputStrings)
				{
					DataLabel.Text += "\n" + item;
				}
			}
			catch (Exception e)
			{
				ErrorDialog.ShowErrorDialog(e);
			}
		}

		/// <summary>
		/// Generates a list of OutputItems for the page.
		/// </summary>
		private ObservableCollection<OutputItem> GenerateOutputItems()
		{
			return new ObservableCollection<OutputItem>()
			{
				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "MolarEntropyOfReaction",
					DisplayName = "Molar entropy of reaction\n[J/K/mol]",
					DisplayFormat = x => $"Molar entropy of reaction: {x}"
				},
				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "MolarEnthalpyOfReaction",
					DisplayName = "Molar enthalpy of reaction\n[J/mol]",
					DisplayFormat = x => $"Molar enthalpy of reaction: {x}"
				},
				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "MolarGibbsEnergyOfReaction",
					DisplayName = "Molar Gibbs energy of reaction\n[J/mol]",
					DisplayFormat = x => $"Molar Gibbs energy of reaction: {x}"
				},
				new OutputItem(OutputItem.ItemType.Plot)
				{
					OutputName = "PlotMolarityTranscience",
					DisplayName = "Species molarity vs reaction time \n[mol/L vs s]"
				}
			};
		}

		private void ButtonAddSpecies_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.AddItem(new ControlRxnSpeciesViewModel
			{
				//Chemical = Chemical.Acetone,
				//EoSFactory = new PengRobinsonEOSFactory(),
				//Stoich = 1,
				//Phase = "",
				IsReactant = true,
				DeleteCommand = ViewModel.DeleteCommand
			});
		}

		private void ButtonAddOutputItem_Click(object sender, RoutedEventArgs e)
		{
			// Move all selected items from left to right list.
			var selected = ListOutputItems_Left.SelectedItems.Cast<OutputItem>().ToList();
			foreach (var item in selected)
			{
				ViewModel.AvailableOutputOptions.Remove(item);
				ViewModel.SelectedOutputOptions.Add(item);
			}
		}

		private void ButtonRemoveOutputItem_Click(object sender, RoutedEventArgs e)
		{
			// Move all selected items from right to left list.
			var selected = ListOutputItems_Right.SelectedItems.Cast<OutputItem>().ToList();
			foreach (var item in selected)
			{
				ViewModel.SelectedOutputOptions.Remove(item);
				ViewModel.AvailableOutputOptions.Add(item);
			}
		}

		private void ButtonSelectOutputItems_Click(object sender, RoutedEventArgs e)
		{
			// Open the output selection popup if not already open.
			if (!PopupSelectOutputItems.IsOpen) { PopupSelectOutputItems.IsOpen = true; }
		}

	}
}