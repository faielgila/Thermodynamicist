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
			ViewModel.Time = double.NaN;
			ViewModel.FrequencyFactor = double.NaN;
			ViewModel.ActivationEnergy = double.NaN;

			// Set output selection options.
			AllOutputOptions = GenerateOutputItems();

			InitializeComponent();

			// Initializes rate law list.
			// Note the use of RateLawFactory instead of the RateLaw object directly.
			DropdownRateLaw.Items.Add(new ElementaryRateLawFactory());

			UpdateValidationStyles(null, null);
		}

		/// <summary>
		/// Checks inputs and packages into Core objects.
		/// </summary>
		private void RunCalc(object sender, RoutedEventArgs e)
		{
			if (CheckInvalidPageInputs())
			{
				ErrorDialog.ShowErrorDialog("An error was detected in the model inputs. Review the errors listed and try again.");
				return;
			}

			Temperature T = ViewModel.T;
			Pressure P = ViewModel.P;
			Time time = ViewModel.Time;
			double frequencyFactor = ViewModel.FrequencyFactor;
			GibbsEnergy activationEnergy = ViewModel.ActivationEnergy;
			var rateLawFactory = ViewModel.RateLawFactory;

			// Get species list from ControlRxnSpecies
			var rxn = new Reaction(ViewModel.GetRxnSpeciesList(), rateLawFactory, frequencyFactor, activationEnergy);

			UpdateData(rxn, T, P, time);
		}

		/// <summary>
		/// Validates all user inputs.
		/// </summary>
		/// <returns>true if any inputs are invalid.</returns>
		private bool CheckInvalidPageInputs()
		{
			ViewModel.Errors.Clear();
			bool cancelCalc = false;
			List<string> missingInputs = new List<string>();

			// Warning: No outputs are selected.
			if (ButtonSelectOutputItems.ViewModel.SelectedOutputOptions.Count == 0)
			{
				ViewModel.Errors.Add(new ErrorInfoViewModel( false,
					"No outputs were selected. Click the 'settings' icon to pick which outputs to calculate."
				));
			}

			// Warning: No rxnSpecies have been defined.
			if (ViewModel.Items.Count == 0)
			{
				ViewModel.Errors.Add(new ErrorInfoViewModel( false,
					"No reaction species were defined. Click the '+' icon in the Reaction panel to add one."
				));
			}

			// Error: Missing model-wide inputs.
			if (ViewModel.T == null || ViewModel.T == 0 || double.IsNaN(ViewModel.T)) missingInputs.Add("Temperature");
			if (ViewModel.P == null || ViewModel.P == 0 || double.IsNaN(ViewModel.T)) missingInputs.Add("Pressure");
			if (ButtonSelectOutputItems.ViewModel.SelectedOutputOptions.Select(item => item.OutputName).Contains("PlotMolarityTranscience"))
			{
				if (ViewModel.Time == null || ViewModel.Time == 0 || double.IsNaN(ViewModel.Time)) missingInputs.Add("Time");
			}

			// Error: Missing reaction inputs.
			if (ViewModel.RateLawFactory == null) missingInputs.Add("Rate law");
			if (double.IsNaN(ViewModel.FrequencyFactor)) missingInputs.Add("Frequency factor");
			if (double.IsNaN(ViewModel.ActivationEnergy)) missingInputs.Add("Activation energy");

			// Validate RxnSpecies inputs.
			var chemicals = new List<Chemical>();
			foreach (var viewModel in ViewModel.Items)
			{
				var chem = viewModel.Chemical;
				var text = viewModel.CheckValidInput();
				if (!(text is null))
				{
					ViewModel.Errors.Add(new ErrorInfoViewModel(true,
						$"Reaction species {Constants.ChemicalNames[chem]} has the following invalid inputs: {text}." +
						"\nSet them to valid inputs and try again."));
					cancelCalc = true;
				}
				if (chemicals.Contains(chem))
				{
					ViewModel.Errors.Add(new ErrorInfoViewModel(true,
						$"Chemical '{Constants.ChemicalNames[chem]}' has multiple entries." +
						"\nRemove or combine any duplicates in the reaction equation and try again."));
					cancelCalc = true;
					break;
				}
				else
				{
					chemicals.Add(chem);
				}
			}

			// Combine missingInputs string.
			if (missingInputs.Count != 0)
			{
				string text = missingInputs.First();
				missingInputs.Remove(missingInputs.First());
				foreach (var item in missingInputs)
				{
					text += "; " + item;
				}
				text = $"The following model-wide inptus are invalid: {text}.\nSet them to valid inputs and try again.";
				ViewModel.Errors.Add(new ErrorInfoViewModel(true, text));
				cancelCalc = true;
			}

			return cancelCalc;
		}

		/// <summary>
		/// Runs calculations, updates DataLabel with outputs.
		/// </summary>
		private void UpdateData(Reaction rxn, Temperature T, Pressure P, Time time)
		{
			// Reset DataLabel in case numeric output options aren't selected.
			DataLabel.Text = "";
			// Reset PlotView visibility in case plot output option isn't selected.
			PlotViewKin.Visibility = Visibility.Collapsed;
			TextNoPlots.Visibility = Visibility.Visible;

			// Get list of all selected output options.
			var selectedOutputs = ButtonSelectOutputItems.ViewModel.SelectedOutputOptions;
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
							var compVec = new MolarityVector();
							foreach (var rxnSpecies in ViewModel.Items)
							{
								compVec.Add(rxnSpecies.Chemical, rxnSpecies.Concentration);
							}
							PlotViewKin.Model = new RxnKineticsPlotModel(rxn, T, P, 0.01, time, compVec).Model;
							PlotViewKin.Background = new SolidColorBrush(Colors.White);
							PlotViewKin.Visibility = Visibility.Visible;
							TextNoPlots.Visibility = Visibility.Collapsed;
							break;
					}
				}

				// Combine all output strings into DataLabel.
				if (outputStrings.Count == 0) { return; }
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
				Chemical = Chemical.Water,
				//EoSFactory = new PengRobinsonEOSFactory(),
				Stoich = 1,
				//Phase = "",
				Concentration = double.NaN,
				IsReactant = true,
				DeleteCommand = ViewModel.DeleteCommand
			});
		}

		private void UpdateValidationStyles(object sender, RoutedEventArgs e)
		{
			if (ButtonSelectOutputItems.ViewModel.SelectedOutputOptions is null || ButtonSelectOutputItems.ViewModel.SelectedOutputOptions.Count == 0)
			{
				ButtonSelectOutputItems.MarkWithWarning();
			} else
			{
				ButtonSelectOutputItems.ClearMarks();
			}
		}

		private void NumBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
		{
			UpdateValidationStyles(sender, null);
		}
	}
}