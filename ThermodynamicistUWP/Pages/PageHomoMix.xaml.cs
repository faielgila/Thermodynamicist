using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using Core;
using Core.EquationsOfState;
using Core.Multicomponent;
using Core.Multicomponent.ActivityModels;
using Core.Reactions;
using Core.VariableTypes;
using Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ThermodynamicistUWP.Converters;
using ThermodynamicistUWP.Dialogs;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;


namespace ThermodynamicistUWP
{
	public sealed partial class PageHomoMix : Page
	{
		public PageHomoMixViewModel ViewModel { get; } = new PageHomoMixViewModel();

		private ObservableCollection<OutputItem> AllOutputOptions { get; set; }

		private ObservableCollection<SpeciesOutputData> SpeciesOutputList = new ObservableCollection<SpeciesOutputData>();
		public PageHomoMix()
		{
			// Set output selection options.
			AllOutputOptions = GenerateOutputItems();

			InitializeComponent();

			// Pass down all output options to output selection button and register a listener for when selected outputs change.
			ButtonSelectOutputItems.ViewModel = new OutputSelectionPopupViewModel(AllOutputOptions);

			// Register OnChanged event for selected output options.
			ButtonSelectOutputItems.ViewModel.SelectedOutputOptions.CollectionChanged += SelectedOutputOptions_CollectionChanged;

			// Initializes activity model list in ActivityModel dropdown.
			// Note the use of ActivityModelFactory instead of the ActivityModel object directly.
			DropdownModel.Items.Add(new UNIFACActivityModelFactory());
			DropdownModel.Items.Add(new IdealMixtureModelFactory());

			// Initialize numeric inputs.
			ViewModel.T = 298;
			ViewModel.P = 101325;


			UpdateValidationStyles();
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

			/* Normally, the input data would be packaged into a Core object (in this case, HomogeneousMixture)
			 * before being passed to UpdateData().
			 * However, calculations for some activity models are done during initialization to prevent
			 * duplicate calculations. Thus, the UpdateData() method will take care of the packaging and will directly
			 * access the ViewModel property on its own.
			 */
			UpdateData(ViewModel.T, ViewModel.P);
		}

		/// <summary>
		/// Validates all user inputs.
		/// </summary>
		/// <returns>true if any inputs are invalid.</returns>
		private bool CheckInvalidPageInputs()
		{
			Temperature T = ViewModel.T;
			Pressure P = ViewModel.P;

			ViewModel.Errors.Clear();
			bool cancelCalc = false;
			List<string> missingInputs = new List<string>();

			// Warning: No outputs are selected.
			if (ButtonSelectOutputItems.ViewModel.SelectedOutputOptions.Count == 0)
			{
				ViewModel.Errors.Add(new ErrorInfoViewModel(false,
					"No outputs were selected. Click the 'settings' icon to pick which outputs to calculate."
				));
			}

			// Warning: No rxnSpecies have been defined.
			if (ViewModel.Items.Count == 0)
			{
				ViewModel.Errors.Add(new ErrorInfoViewModel(false,
					"No reaction species were defined. Click the '+' icon in the Reaction panel to add one."
				));
			}

			// Error: Missing model-wide inputs.
			if (T <= 0 || double.IsNaN(T)) missingInputs.Add("Temperature");
			if (P <= 0 || double.IsNaN(P)) missingInputs.Add("Pressure");

			// Error: Missing reaction inputs.
			if (DropdownModel.SelectedItem == null) missingInputs.Add("Activity model");

			// Validate MixtureSpecies inputs.
			var chemicals = new List<Chemical>();
			foreach (var viewModel in ViewModel.Items)
			{
				var chem = viewModel.Chemical;
				var text = viewModel.CheckValidInput();
				if (!(text is null))
				{
					ViewModel.Errors.Add(new ErrorInfoViewModel(true,
						$"Mixture species {Constants.ChemicalNames[chem]} has the following invalid inputs: {text}." +
						"\nSet them to valid inputs and try again."));
					cancelCalc = true;
				}
				if (chemicals.Contains(chem))
				{
					ViewModel.Errors.Add(new ErrorInfoViewModel(true,
						$"Chemical '{Constants.ChemicalNames[chem]}' has multiple entries." +
						"\nRemove or combine any duplicates in the mixture definition and try again."));
					cancelCalc = true;
					break;
				}
				else
				{
					chemicals.Add(chem);
				}
			}

			// Calculate mole fraction total to ensure sum is 1.
			double sum = 0;
			foreach (var item in ViewModel.Items)
			{
				sum += item.SpeciesMoleFraction;
			}
			if (Math.Abs(sum - 1) >= 1e-2)
			{
				ViewModel.Errors.Add(new ErrorInfoViewModel(true, "Mixture species mole fractions must sum to 1."));
				cancelCalc = true;
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
		private void UpdateData(Temperature T, Pressure P)
		{
			// Reset DataLabel in case numeric output options aren't selected.
			DataLabel.Text = "";
			TextNoOutputDataLabel.Visibility = Visibility.Visible;
			// Reset PlotView visibility in case plot output option isn't selected.
			//PlotViewKin.Visibility = Visibility.Collapsed;
			//TextNoPlots.Visibility = Visibility.Visible;

			// Get list of all selected output options.
			var selectedOutputs = ButtonSelectOutputItems.ViewModel.SelectedOutputOptions;
			Dictionary<Chemical, SpeciesOutputData> speciesOutputDict = new Dictionary<Chemical, SpeciesOutputData>();
			List<string> outputStrings = new List<string>();
			try
			{
				// Get species list from ControlMixtureSpecies.
				var speciesList = ViewModel.GetMixtureSpeciesList();
				// Instatiate activity model using the provided ActivityModelFactory.
				var model = ViewModel.ActivityModelFactory.Create(ViewModel.GetMixtureSpeciesList());
				// Instatiate HomogeneousMixture Core object. This will begin some preliminary calculations.
				var mixture = new HomogeneousMixture(speciesList, "liquid", model, null);

				
				// Create empty SpeciesOutputData objects for adding data to later.
				foreach (var species in speciesList)
				{
					speciesOutputDict.Add(species.chemical, new SpeciesOutputData(species.chemical));
				}

				foreach (var item in selectedOutputs)
				{
					if (item.Type == OutputItem.ItemType.Folder) continue;

					switch (item.OutputName)
					{
						default:
							break;
						//case "TotalMolarEntropy":
						//	//var S = mixture.TotalMolarEntropy(T, P);
						//	//outputStrings.Add(item.DisplayFormat(S.ToEngrNotation(5)));
						//	break;
						case "TotalMolarEnthalpy":
							var H = mixture.TotalMolarEnthalpy(T, P);
							outputStrings.Add(item.DisplayFormat(H.ToEngrNotation(5)));
							break;
						case "TotalMolarGibbsEnergy":
							var G = mixture.TotalMolarGibbsEnergy(T, P);
							outputStrings.Add(item.DisplayFormat(G.ToEngrNotation(5)));
							break;
						case "MolarEnthalpyOfMixing":
							var Hmix = mixture.MolarEnthalpyOfMixing(T, P);
							outputStrings.Add(item.DisplayFormat(Hmix.ToEngrNotation(5)));
							break;
						//case "MolarGibbsEnergyOfMixing":
						//	var Gmix = mixture.MolarGibbsEnergyOfMixing(T, P);
						//	outputStrings.Add(item.DisplayFormat(Gmix.ToEngrNotation(5)));
						//	break;

						case "SpeciesPartialMolarEnthalpy":
							foreach (var species in speciesList)
							{
								var xH = mixture.SpeciesPartialMolarEnthalpy(T, P, species.chemical).ToEngrNotation(5);
								speciesOutputDict[species.chemical].AddOutputDatum(item.DisplayFormat(xH));
							}
							break;
						case "SpeciesChemicalPotential":
							foreach (var species in speciesList)
							{
								var xMu = mixture.SpeciesChemicalPotential(T, P, species.chemical).ToEngrNotation(5);
								speciesOutputDict[species.chemical].AddOutputDatum(item.DisplayFormat(xMu));
							}
							break;
						case "SpeciesFugacity":
							foreach (var species in speciesList)
							{
								var xf = mixture.SpeciesFugacity(T, P, species.chemical).ToEngrNotation(5);
								speciesOutputDict[species.chemical].AddOutputDatum(item.DisplayFormat(xf));
							}
							break;
						case "SpeciesActivityCoefficient":
							foreach (var species in speciesList)
							{
								var xa = mixture.activityModel.SpeciesActivityCoefficient(species.chemical, T).ToEngrNotation(5);
								speciesOutputDict[species.chemical].AddOutputDatum(item.DisplayFormat(xa));
							}
							break;
					}
				}

				// Publicize phase output data.
				SpeciesOutputList.Clear();
				foreach (var species in speciesList)
				{
					if (speciesOutputDict[species.chemical].outputData.Equals("")) continue;
					SpeciesOutputList.Add(speciesOutputDict[species.chemical]);
				}

				// Combine all output strings into DataLabel.
				if (outputStrings.Count == 0 && SpeciesOutputList.Count == 0)
				{
					TextNoOutputDataLabel.Visibility = Visibility.Visible;
					DataLabel.Visibility = Visibility.Collapsed;
				}
				else
				{
					TextNoOutputDataLabel.Visibility = Visibility.Collapsed;
				}
				if (outputStrings.Count != 0)
				{
					DataLabel.Text = outputStrings.First();
					outputStrings.Remove(outputStrings.First());
					foreach (var item in outputStrings)
					{
						DataLabel.Text += "\n" + item;
					}
					DataLabel.Visibility = Visibility.Visible;
				}
				else
				{
					DataLabel.Visibility = Visibility.Collapsed;
				}
			}
			catch (Exception e)
			{
				ErrorDialog.ShowErrorDialog(e);
			}
		}

		/// <summary>
		/// Updates styles on input controls to signal input validation.
		/// </summary>
		private void UpdateValidationStyles()
		{
			var SelectedOutputs = ButtonSelectOutputItems.GetSelectedOutputs();
			Temperature T = NumBoxT.GetValue();
			Pressure P = NumBoxP.GetValue();

			if (SelectedOutputs is null || SelectedOutputs.Count == 0)
			{
				ButtonSelectOutputItems.MarkWithWarning();
			}
			else
			{
				ButtonSelectOutputItems.ClearMarks();
			}

			if (double.IsNaN(T) || T <= 0)
			{
				NumBoxT.MarkWithError();
			}
			else
			{
				NumBoxT.ClearMarks();
			}

			if (double.IsNaN(P) || P <= 0)
			{
				NumBoxP.MarkWithError();
			}
			else
			{
				NumBoxP.ClearMarks();
			}

			if (DropdownModel.SelectedValue is null)
			{
				DropdownModel.Style = this.FindResource("ComboBoxErrorStyle") as Style;
				InfoBadgeDropdownModel.Style = this.FindResource("ControlErrorInfoBadgeStyle") as Style;
				InfoBadgeDropdownModel.Visibility = Visibility.Visible;
			}
			else
			{
				DropdownModel.Style = this.FindResource("ComboBoxDefaultStyle") as Style;
				InfoBadgeDropdownModel.Visibility = Visibility.Collapsed;
			}
		}

		/// <summary>
		/// Generates a list of OutputItems for the page.
		/// </summary>
		private ObservableCollection<OutputItem> GenerateOutputItems()
		{
			return new ObservableCollection<OutputItem>()
			{
				//new OutputItem(OutputItem.ItemType.Number)
				//{
				//	OutputName = "TotalMolarEntropy",
				//	DisplayName = "Total molar entropy\n[J/K/mol]",
				//	DisplayFormat = x => $"Total molar entropy: {x}"
				//},
				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "TotalMolarEnthalpy",
					DisplayName = "Total molar enthalpy\n[J/mol]",
					DisplayFormat = x => $"Total molar enthalpy, H: {x}"
				},
				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "TotalMolarGibbsEnergy",
					DisplayName = "Total molar Gibbs energy\n[J/mol]",
					DisplayFormat = x => $"Total molar Gibbs energy, G: {x}"
				},
				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "MolarEnthalpyOfMixing",
					DisplayName = "Molar enthalpy of mixing\n[J/mol]",
					DisplayFormat = x => $"Molar enthalpy of mixing, Hmix: {x}"
				},
				//new OutputItem(OutputItem.ItemType.Number)
				//{
				//	OutputName = "MolarGibbsEnergyOfMixing",
				//	DisplayName = "Molar Gibbs energy of mixing\n[J/mol]",
				//	DisplayFormat = x => $"Molar Gibbs energy of mixing: {x}"
				//},

				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "SpeciesPartialMolarEnthalpy",
					DisplayName = "Species partial molar enthalpy\n[J/mol]",
					DisplayFormat = x => $"Partial molar enthalpy, s: {x}"
				},
				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "SpeciesChemicalPotential",
					DisplayName = "Species chemical potential\n[J/mol]",
					DisplayFormat = x => $"Chemical potential, μ: {x}"
				},
				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "SpeciesFugacity",
					DisplayName = "Species fugacity\n[Pa]",
					DisplayFormat = x => $"Fugacity, f: {x}"
				},
				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "SpeciesActivityCoefficient",
					DisplayName = "Species activity coefficient\n[1]",
					DisplayFormat = x => $"Activity coefficient, γ: {x}"
				},
			};
		}

		private void ButtonAddSpecies_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.AddItem(new ControlMixtureSpeciesViewModel()
			{
				//Chemical = Chemical.Acetone,
				//EoSFactory = new PengRobinsonEOSFactory(),
				SpeciesMoleFraction = 0,
				//ModeledPhase = "",
				DeleteCommand = ViewModel.DeleteCommand
			});
		}

		private void SelectedOutputOptions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			UpdateValidationStyles();
		}
		private void ValNumBox_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			UpdateValidationStyles();
		}

		private void NumBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
		{
			UpdateValidationStyles();
		}

		private void Dropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdateValidationStyles();
		}
	}

	/// <summary>
	/// Stores chemical and output data (in a pre-formatted string) for use in SpeciesResultsTemplate
	/// </summary>
	public class SpeciesOutputData
	{
		public string chemical;
		public string outputData;

		public SpeciesOutputData(Chemical species)
		{
			chemical = Constants.ChemicalNames[species];
			this.outputData = "";
		}

		public void AddOutputDatum(string datum)
		{
			if (outputData.Equals(""))
			{
				outputData = datum;
			}
			else
			{
				outputData += "\n" + datum;
			}
		}
	}
}
