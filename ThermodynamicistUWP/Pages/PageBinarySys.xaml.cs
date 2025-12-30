using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using Core;
using Core.EquationsOfState;
using Core.Multicomponent;
using Core.Multicomponent.ActivityModels;
using Core.Reactions;
using Core.VariableTypes;
using Core.ViewModels;
using OxyPlot;
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
	public sealed partial class PageBinarySys: Page
	{
		public PageBinarySysViewModel ViewModel { get; } = new PageBinarySysViewModel();

		private ObservableCollection<OutputItem> AllOutputOptions { get; set; }

		private ObservableCollection<EquilibriumOutputData> EquilibriumOutputList = new ObservableCollection<EquilibriumOutputData>();

		public PageBinarySys()
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
			//DropdownModel.Items.Add(new UNIFACActivityModelFactory());
			//DropdownModel.Items.Add(new IdealMixtureModelFactory());

			// Initialize numeric inputs.
			ViewModel.T = 298;
			ViewModel.P = 101325;


			// Initialize phase options.
			ViewModel.AvailablePhases = new List<string>()
			{
				"liquid", "vapor"
			};

			// Initialize activity model options.
			// Note the use of ActivityModelFactory instead of the ActivityModel object directly.
			ViewModel.AvailableActivityModels.Add(new IdealMixtureModelFactory());
			ViewModel.AvailableActivityModels.Add(new UNIFACActivityModelFactory());

			// Initialize equation of state options.
			ViewModel.AvailableEoS.Add(new VanDerWaalsEOSFactory());
			ViewModel.AvailableEoS.Add(new PengRobinsonEOSFactory());

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

			/* Normally, the input data would be packaged into a Core object (in this case, MultiphaseSystem)
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
			MoleFraction moleFraction_p1s1 = NumBoxMoleFractionPhase1Species1.Value;
			MoleFraction moleFraction_p1s2 = NumBoxMoleFractionPhase1Species2.Value;
			MoleFraction moleFraction_p2s1 = NumBoxMoleFractionPhase2Species1.Value;
			MoleFraction moleFraction_p2s2 = NumBoxMoleFractionPhase2Species2.Value;

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

			// Error: Missing model-wide inputs.
			if (T <= 0 || double.IsNaN(T)) missingInputs.Add("Temperature");
			if (P <= 0 || double.IsNaN(P)) missingInputs.Add("Pressure");

			// Error: Missing phase and activity model inputs.
			if (DropdownPhase1.SelectedItem == null) missingInputs.Add("Phase 1");
			if (DropdownModelPhase1.SelectedItem == null) missingInputs.Add("Activity model 1");
			if (DropdownPhase2.SelectedItem == null) missingInputs.Add("Phase 2");
			if (DropdownModelPhase2.SelectedItem == null) missingInputs.Add("Activity model 2");

			// Error: Missing chemical inputs.
			if (DropdownSpecies1.SelectedItem == null) missingInputs.Add("Chemical 1");
			if (DropdownSpecies2.SelectedItem == null) missingInputs.Add("Chemical 2");

			// Error: Missing mole fraction inputs.
			if (double.IsNaN(NumBoxMoleFractionPhase1Species1.Value) ||
				NumBoxMoleFractionPhase1Species1.Value <= 0) missingInputs.Add("Mole fraction (phase 1, chemical 1)");
			if (double.IsNaN(NumBoxMoleFractionPhase1Species2.Value) ||
				NumBoxMoleFractionPhase1Species2.Value <= 0) missingInputs.Add("Mole fraction (phase 1, chemical 2)");
			if (double.IsNaN(NumBoxMoleFractionPhase2Species1.Value) ||
				NumBoxMoleFractionPhase2Species1.Value <= 0) missingInputs.Add("Mole fraction (phase 2, chemical 1)");
			if (double.IsNaN(NumBoxMoleFractionPhase2Species2.Value) ||
				NumBoxMoleFractionPhase2Species2.Value <= 0) missingInputs.Add("Mole fraction (phase 2, chemical 2)");

			// Error: Missing EoS inputs.
			if (DropdownEoSPhase1Species1 == null) missingInputs.Add("Equation of state (phase 1, chemical 1)");
			if (DropdownEoSPhase1Species2 == null) missingInputs.Add("Equation of state (phase 1, chemical 2)");
			if (DropdownEoSPhase2Species1 == null) missingInputs.Add("Equation of state (phase 2, chemical 1)");
			if (DropdownEoSPhase2Species2 == null) missingInputs.Add("Equation of state (phase 2, chemical 2)");

			// Error: Sum of mole fraction for phase is not equal to 1.
			var totalMoleFraction_p1 = moleFraction_p1s1 + moleFraction_p1s2;
			if (Math.Abs(totalMoleFraction_p1 - 1) >= 1e-2)
			{
				ViewModel.Errors.Add(new ErrorInfoViewModel(true, "Mole fractions for phase 1 must sum to 1."));
			}
			var totalMoleFraction_p2 = moleFraction_p2s1 + moleFraction_p2s2;
			if (Math.Abs(totalMoleFraction_p2 - 1) >= 1e-2)
			{
				ViewModel.Errors.Add(new ErrorInfoViewModel(true, "Mole fractions for phase 2 must sum to 1."));
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
			Dictionary<MoleFraction , EquilibriumOutputData> equilibriumOutputDict = new Dictionary<MoleFraction, EquilibriumOutputData>();
			List<string> outputStrings = new List<string>();
			try
			{
				// Instatiate activity model using the provided ActivityModelFactory.
				var modelPhase1 = ViewModel.CreateActivityModel(0);
				var modelPhase2 = ViewModel.CreateActivityModel(1);
				// Instatiate HomogeneousMixture Core object. This will begin some preliminary calculations.
				var mixturePhases = new List<HomogeneousMixture>()
				{
					new HomogeneousMixture(ViewModel.GetMixtureSpeciesList(0), "vapor", modelPhase1, null),
					new HomogeneousMixture(ViewModel.GetMixtureSpeciesList(1), "liquid", modelPhase2, null)
				};
				
				// Define temporary composition vector.
				var compVec = new CompositionVector()
				{
					[ViewModel.GetMixtureSpeciesList(0)[0].chemical] = 0.5,
					[ViewModel.GetMixtureSpeciesList(0)[1].chemical] = 0.5
				};
				// Define new MultiphaseSystem. Assume first chemical is the species basis.
				var binSys = new MultiphaseSystem(compVec, mixturePhases, ViewModel.GetMixtureSpeciesList(0)[0].chemical);

				// Find equilibrium states for the system.
				var equilibriumResults = binSys.FindPhaseEquilibria(T, P);
				bool noEquilbriaFound = equilibriumResults.Count == 0;

				// Create empty EquilibriumOutputData objects for adding data to later.
				foreach (var result in equilibriumResults)
				{
					equilibriumOutputDict.Add(result.Value.First().Value, new EquilibriumOutputData(result));
				}

				foreach (var item in selectedOutputs)
				{
					if (item.Type == OutputItem.ItemType.Folder) continue;

					switch (item.OutputName)
					{
						default:
							break;

						case "SpeciesMoleFraction":
							if (equilibriumResults.Count == 0)
							{
								break;
							}
							foreach (var result in equilibriumResults)
							{
								foreach (var kvp in result.Value)
								{
									var x = kvp.Value.RoundToSigfigs(3).ToString();
									equilibriumOutputDict[result.Value.First().Value].AddOutputDatum(kvp.Key, item.DisplayFormat(x));
								}
							}
							break;

						case "SpeciesChemicalPotential":
							if (equilibriumResults.Count == 0)
							{
								break;
							}
							foreach (var result in equilibriumResults)
							{
								foreach (var kvp in result.Value)
								{
									var point = new MultiphaseStatePoint(kvp.Key.species, kvp.Key.phase, result.T, result.P);
									var x = binSys.chemicalPotentialCurves[point].GetValue(kvp.Value).ToEngrNotation(5);
									equilibriumOutputDict[result.Value.First().Value].AddOutputDatum(kvp.Key, item.DisplayFormat(x));
								}
							}
							break;
					}
				}

				// Publicize phase output data.
				EquilibriumOutputList.Clear();
				if (equilibriumOutputDict.Count > 0)
				{
					TextNoOutputDataLabel.Visibility = Visibility.Collapsed;
					foreach (var item in equilibriumOutputDict.Values)
					{
						EquilibriumOutputList.Add(item);
					}
				}

				// Show warning if there are no equilibria found.

				// Combine all output strings into DataLabel.
				if (outputStrings.Count == 0 && EquilibriumOutputList.Count == 0)
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


			if (DropdownPhase1.SelectedValue is null)
			{
				DropdownPhase1.Style = this.FindResource("ComboBoxErrorStyle") as Style;
				InfoBadgeDropdownPhase1.Style = this.FindResource("ControlErrorInfoBadgeStyle") as Style;
				InfoBadgeDropdownPhase1.Visibility = Visibility.Visible;
			}
			else
			{
				DropdownPhase1.Style = this.FindResource("ComboBoxDefaultStyle") as Style;
				InfoBadgeDropdownPhase1.Visibility = Visibility.Collapsed;
			}

			if (DropdownPhase2.SelectedValue is null)
			{
				DropdownPhase2.Style = this.FindResource("ComboBoxErrorStyle") as Style;
				InfoBadgeDropdownPhase2.Style = this.FindResource("ControlErrorInfoBadgeStyle") as Style;
				InfoBadgeDropdownPhase2.Visibility = Visibility.Visible;
			}
			else
			{
				DropdownPhase2.Style = this.FindResource("ComboBoxDefaultStyle") as Style;
				InfoBadgeDropdownPhase2.Visibility = Visibility.Collapsed;
			}


			if (DropdownModelPhase1.SelectedValue is null)
			{
				DropdownModelPhase1.Style = this.FindResource("ComboBoxErrorStyle") as Style;
				InfoBadgeDropdownModelPhase1.Style = this.FindResource("ControlErrorInfoBadgeStyle") as Style;
				InfoBadgeDropdownModelPhase1.Visibility = Visibility.Visible;
			}
			else
			{
				DropdownModelPhase1.Style = this.FindResource("ComboBoxDefaultStyle") as Style;
				InfoBadgeDropdownModelPhase1.Visibility = Visibility.Collapsed;
			}

			if (DropdownModelPhase2.SelectedValue is null)
			{
				DropdownModelPhase2.Style = this.FindResource("ComboBoxErrorStyle") as Style;
				InfoBadgeDropdownModelPhase2.Style = this.FindResource("ControlErrorInfoBadgeStyle") as Style;
				InfoBadgeDropdownModelPhase2.Visibility = Visibility.Visible;
			}
			else
			{
				DropdownModelPhase2.Style = this.FindResource("ComboBoxDefaultStyle") as Style;
				InfoBadgeDropdownModelPhase2.Visibility = Visibility.Collapsed;
			}

			if (DropdownSpecies1.SelectedValue is null)
			{
				DropdownSpecies1.Style = this.FindResource("ComboBoxErrorStyle") as Style;
				InfoBadgeDropdownSpecies1.Style = this.FindResource("ControlErrorInfoBadgeStyle") as Style;
				InfoBadgeDropdownSpecies1.Visibility = Visibility.Visible;
			}
			else
			{
				DropdownSpecies1.Style = this.FindResource("ComboBoxDefaultStyle") as Style;
				InfoBadgeDropdownSpecies1.Visibility = Visibility.Collapsed;
			}

			if (DropdownSpecies2.SelectedValue is null)
			{
				DropdownSpecies2.Style = this.FindResource("ComboBoxErrorStyle") as Style;
				InfoBadgeDropdownSpecies2.Style = this.FindResource("ControlErrorInfoBadgeStyle") as Style;
				InfoBadgeDropdownSpecies2.Visibility = Visibility.Visible;
			}
			else
			{
				DropdownSpecies2.Style = this.FindResource("ComboBoxDefaultStyle") as Style;
				InfoBadgeDropdownSpecies2.Visibility = Visibility.Collapsed;
			}


			var moleFraction_p1s1 = NumBoxMoleFractionPhase1Species1.Value;
			var moleFraction_p1s2 = NumBoxMoleFractionPhase1Species2.Value;
			var moleFraction_p2s1 = NumBoxMoleFractionPhase2Species1.Value;
			var moleFraction_p2s2 = NumBoxMoleFractionPhase2Species2.Value;
			if (double.IsNaN(moleFraction_p1s1) || moleFraction_p1s1 <= 0)
			{
				NumBoxMoleFractionPhase1Species1.MarkWithError();
			}
			else
			{
				NumBoxMoleFractionPhase1Species1.ClearMarks();
			}
			
			if (double.IsNaN(moleFraction_p1s2) || moleFraction_p1s2 <= 0)
			{
				NumBoxMoleFractionPhase1Species2.MarkWithError();
			}
			else
			{
				NumBoxMoleFractionPhase1Species2.ClearMarks();
			}

			if (double.IsNaN(moleFraction_p2s1) || moleFraction_p2s1 <= 0)
			{
				NumBoxMoleFractionPhase2Species1.MarkWithError();
			}
			else
			{
				NumBoxMoleFractionPhase2Species1.ClearMarks();
			}

			if (double.IsNaN(moleFraction_p2s2) || moleFraction_p2s2 <= 0)
			{
				NumBoxMoleFractionPhase2Species2.MarkWithError();
			}
			else
			{
				NumBoxMoleFractionPhase2Species2.ClearMarks();
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
				//new OutputItem(OutputItem.ItemType.Number)
				//{
				//	OutputName = "TotalMolarEnthalpy",
				//	DisplayName = "Total molar enthalpy\n[J/mol]",
				//	DisplayFormat = x => $"Total molar enthalpy, H: {x}"
				//},
				//new OutputItem(OutputItem.ItemType.Number)
				//{
				//	OutputName = "TotalMolarGibbsEnergy",
				//	DisplayName = "Total molar Gibbs energy\n[J/mol]",
				//	DisplayFormat = x => $"Total molar Gibbs energy, G: {x}"
				//},
				//new OutputItem(OutputItem.ItemType.Number)
				//{
				//	OutputName = "MolarEnthalpyOfMixing",
				//	DisplayName = "Molar enthalpy of mixing\n[J/mol]",
				//	DisplayFormat = x => $"Molar enthalpy of mixing, Hmix: {x}"
				//},
				//new OutputItem(OutputItem.ItemType.Number)
				//{
				//	OutputName = "MolarGibbsEnergyOfMixing",
				//	DisplayName = "Molar Gibbs energy of mixing\n[J/mol]",
				//	DisplayFormat = x => $"Molar Gibbs energy of mixing: {x}"
				//},

				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "SpeciesMoleFraction",
					DisplayName = "Mole fraction of species in phase\n[mol/L]",
					DisplayFormat = x => $"Mole fraction in phase: {x}"
				},
				//new OutputItem(OutputItem.ItemType.Number)
				//{
				//	OutputName = "SpeciesPartialMolarEnthalpy",
				//	DisplayName = "Species partial molar enthalpy\n[J/mol]",
				//	DisplayFormat = x => $"Partial molar enthalpy, s: {x}"
				//},
				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "SpeciesChemicalPotential",
					DisplayName = "Species chemical potential\n[J/mol]",
					DisplayFormat = x => $"Chemical potential, μ: {x}"
				},
				//new OutputItem(OutputItem.ItemType.Number)
				//{
				//	OutputName = "SpeciesFugacity",
				//	DisplayName = "Species fugacity\n[Pa]",
				//	DisplayFormat = x => $"Fugacity, f: {x}"
				//},
				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "SpeciesActivityCoefficient",
					DisplayName = "Species activity coefficient\n[1]",
					DisplayFormat = x => $"Activity coefficient, γ: {x}"
				},
			};
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
	/// Stores multiphase equilibrium output data (in a pre-formatted string) for use in EquilibriumResultsTemplate
	/// </summary>
	public class EquilibriumOutputData
	{
		public string phase1;
		public string phase2;
		public string chemical1;
		public string chemical2;
		public string outputData_p1s1 = "";
		public string outputData_p1s2 = "";
		public string outputData_p2s1 = "";
		public string outputData_p2s2 = "";

		public readonly List<string> phasesList;
		public readonly List<Chemical> speciesList;

		public EquilibriumOutputData(MultiphaseEquilibriumResult result)
		{
			if (result.Value.Count > 4) throw new ArgumentOutOfRangeException("Too many species and/or phases present in equilibrium result.");
			phasesList = result.Value.Select(kvp => kvp.Key.phase).Distinct().ToList();
			speciesList = result.Value.Select(kvp => kvp.Key.species).Distinct().ToList();

			phase1 = phasesList[0];
			phase2 = phasesList[1];
			chemical1 = Constants.ChemicalNames[speciesList[0]];
			chemical2 = Constants.ChemicalNames[speciesList[1]];
		}

		public void AddOutputDatum(string phase, Chemical species, string datum)
		{
			if (phase == phase1 && Constants.ChemicalNames[species] == chemical1)
			{
				if (outputData_p1s1 == "")
				{
					outputData_p1s1 = datum;
				}
				else
				{
					outputData_p1s1 += "\n" + datum;
				}
			}
			if (phase == phase1 && Constants.ChemicalNames[species] == chemical2)
			{
				if (outputData_p1s2 == "")
				{
					outputData_p1s2 = datum;
				}
				else
				{
					outputData_p1s2 += "\n" + datum;
				}
			}
			if (phase == phase2 && Constants.ChemicalNames[species] == chemical1)
			{
				if (outputData_p2s1 == "")
				{
					outputData_p2s1 = datum;
				}
				else
				{
					outputData_p2s1 += "\n" + datum;
				}
			}
			if (phase == phase2 && Constants.ChemicalNames[species] == chemical2)
			{
				if (outputData_p2s2 == "")
				{
					outputData_p2s2 = datum;
				}
				else
				{
					outputData_p2s2 += "\n" + datum;
				}
			}
		}

		public void AddOutputDatum((string, Chemical) key, string datum)
		{
			AddOutputDatum(key.Item1, key.Item2, datum);
		}
	}
}
