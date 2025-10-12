using Core;
using Core.EquationsOfState;
using Core.Reactions;
using Core.Reactions.Kinetics;
using Core.VariableTypes;
using Core.ViewModels;
using Microsoft.UI.Xaml.Controls;
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

		public ObservableCollection<RxnKinOutputItem> RxnKinOutputDataSource { get; set; }

		private ObservableCollection<RxnKinOutputItem> AllOutputOptions { get; set; }

		public PageRxnKin()
		{
			InitializeComponent();

			// Initializes rate law list in RateLaw dropdown.
			// Note the use of RateLawFactory instead of the RateLaw object directly.
			DropdownRateLaw.Items.Add(new ElementaryRateLawFactory());

			// Sets input values to defaults
			ViewModel.T = 273;
			ViewModel.P = 101325;
			ViewModel.FrequencyFactor = 1;
			ViewModel.ActivationEnergy = 0;

			// Set TreeOutputSelection options.
			RxnKinOutputDataSource = GetOutputSelectionList();
			SetAllOutputOptions();
			ViewModel.AvailableOutputOptions = AllOutputOptions;
			ViewModel.SelectedOutputOptions = new ObservableCollection<RxnKinOutputItem>();
		}

		private void ButtonAddSpecies_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.AddItem(new ControlRxnSpeciesViewModel
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
			if (ValidatePageInputs()) return;

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
			//UpdatePlots(rxn, T, P);
		}

		private bool ValidatePageInputs()
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
				DropdownRateLaw.BorderThickness = new Thickness(1);
				DropdownRateLaw.BorderBrush = new SolidColorBrush(Colors.Red);
				cancelCalc = true;
			}

			return cancelCalc;
		}

		/// <summary>
		/// Runs calculations, updates DataLabel with outputs.
		/// </summary>
		private void UpdateData(Reaction rxn, Temperature T, Pressure P)
		{
			// Get list of all selected output options.
			var selectedOutputs = ViewModel.GetSelectedOutputItems();
			List<string> outputStrings = new List<string>();
			try
			{
				if (selectedOutputs.Contains(RxnKinOutputItem.ItemName.MolarEntropyOfReaction))
				{
					var output = rxn.MolarEntropyOfReaction(T, P).ToEngrNotation(5);
					outputStrings.Add(RxnKinOutputItem.ItemNameToOutputString[RxnKinOutputItem.ItemName.MolarEntropyOfReaction].Invoke(output));
				}
				if (selectedOutputs.Contains(RxnKinOutputItem.ItemName.MolarEnthalpyOfReaction)) {
					var output = rxn.MolarEnthalpyOfReaction(T, P).ToEngrNotation(5);
					outputStrings.Add(RxnKinOutputItem.ItemNameToOutputString[RxnKinOutputItem.ItemName.MolarEnthalpyOfReaction].Invoke(output));
				}
				if (selectedOutputs.Contains(RxnKinOutputItem.ItemName.MolarGibbsEnergyOfReaction))
				{
					var output = rxn.MolarGibbsEnergyOfReaction(T, P).ToEngrNotation(5);
					outputStrings.Add(RxnKinOutputItem.ItemNameToOutputString[RxnKinOutputItem.ItemName.MolarGibbsEnergyOfReaction].Invoke(output));
				}
				if (selectedOutputs.Contains(RxnKinOutputItem.ItemName.PlotMolarityTransience))
				{
					// Fills in the plot view with a view model using the new settings
					PlotViewKin.Model = new RxnKineticsPlotModel(rxn, T, P, 0.01, 10).Model;
					PlotViewKin.Background = new SolidColorBrush(Colors.White);
					PlotViewKin.Visibility = Visibility.Visible;
				} else
				{
					PlotViewKin.Visibility = Visibility.Collapsed;
				}

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
		/// Initialize the data for TreeRxnKinOutputSelection.
		/// </summary>
		private ObservableCollection<RxnKinOutputItem> GetOutputSelectionList()
		{
			return new ObservableCollection<RxnKinOutputItem>
			{
				new RxnKinOutputItem
				{
					Name = "Thermodynamic properties",
					Glyph = "&#xED41;",
					Type = RxnKinOutputItem.ItemType.Folder,
					Children =
					{
						new RxnKinOutputItem
						{
							Name = "Molar enthalpy of reaction [J/mol]",
							Item = RxnKinOutputItem.ItemName.MolarEnthalpyOfReaction,
							Type = RxnKinOutputItem.ItemType.Number
						},
						new RxnKinOutputItem
						{
							Name = "Molar entropy of reaction [J/mol]",
							Item = RxnKinOutputItem.ItemName.MolarEntropyOfReaction,
							Type = RxnKinOutputItem.ItemType.Number
						},
						new RxnKinOutputItem
						{
							Name = "Molar Gibbs energy of reaction [J/mol]",
							Item = RxnKinOutputItem.ItemName.MolarGibbsEnergyOfReaction,
							Type = RxnKinOutputItem.ItemType.Number
						},
					},
				},
				new RxnKinOutputItem
				{
					Name = "Kinetics",
					Type = RxnKinOutputItem.ItemType.Folder,
					Children =
					{
						new RxnKinOutputItem
						{
							Name = "Molarity transience",
							Item = RxnKinOutputItem.ItemName.PlotMolarityTransience,
							Type = RxnKinOutputItem.ItemType.Plot
						},
					},
				},
			};
		}

		private void SetAllOutputOptions()
		{
			var item_rxnS = new RxnKinOutputItem(RxnKinOutputItem.ItemName.MolarEntropyOfReaction, RxnKinOutputItem.ItemType.Number);
			var item_rxnH = new RxnKinOutputItem(RxnKinOutputItem.ItemName.MolarEnthalpyOfReaction, RxnKinOutputItem.ItemType.Number);
			var item_rxnG = new RxnKinOutputItem(RxnKinOutputItem.ItemName.MolarGibbsEnergyOfReaction, RxnKinOutputItem.ItemType.Number);
			var item_transience = new RxnKinOutputItem(RxnKinOutputItem.ItemName.PlotMolarityTransience, RxnKinOutputItem.ItemType.Plot);

			AllOutputOptions = new ObservableCollection<RxnKinOutputItem>()
			{
				item_rxnS, item_rxnH, item_rxnG,
				item_transience
			};
		}

		private void ButtonAddOutputItem_Click(object sender, RoutedEventArgs e)
		{
			// Move all selected items from left to right list.
			var selected = ListOutputItems_Left.SelectedItems.ToList();
			foreach (var item in selected)
			{
				ViewModel.AvailableOutputOptions.Remove((RxnKinOutputItem)item);
				ViewModel.SelectedOutputOptions.Add((RxnKinOutputItem)item);
			}
		}

		private void ButtonRemoveOutputItem_Click(object sender, RoutedEventArgs e)
		{
			// Move all selected items from right to left list.
			var selected = ListOutputItems_Right.SelectedItems.ToList();
			foreach (var item in selected)
			{
				ViewModel.SelectedOutputOptions.Remove((RxnKinOutputItem)item);
				ViewModel.AvailableOutputOptions.Add((RxnKinOutputItem)item);
			}
		}

		private void ButtonSelectOutputItems_Click(object sender, RoutedEventArgs e)
		{
			// Open the output selection popup if not already open.
			if (!PopupSelectOutputItems.IsOpen) { PopupSelectOutputItems.IsOpen = true; }
		}
	}
}