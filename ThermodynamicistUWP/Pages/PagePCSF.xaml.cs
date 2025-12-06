using CommunityToolkit.WinUI;
using Core;
using Core.EquationsOfState;
using Core.VariableTypes;
using Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using ThermodynamicistUWP.Dialogs;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ThermodynamicistUWP
{
	public sealed partial class PagePCSF : Page
	{
		public PagePCSFViewModel ViewModel { get; } = new PagePCSFViewModel();

		private ObservableCollection<OutputItem> AllOutputOptions { get; set; }

		private ObservableCollection<PhaseOutputData> PhasesOutputList = new ObservableCollection<PhaseOutputData>();

		public PagePCSF()
		{
			// Set output selection options.
			AllOutputOptions = GenerateOutputItems();

			InitializeComponent();

			// Pass down all output options to output selection button.
			ButtonSelectOutputItems.ViewModel = new OutputSelectionPopupViewModel(AllOutputOptions);

			// Register OnChanged event for selected output options.
			ButtonSelectOutputItems.ViewModel.SelectedOutputOptions.CollectionChanged += SelectedOutputOptions_CollectionChanged;

			// Initialize DropdownSpecies to a value (arbitrarily chosen to be 0).
			// Not including this init will cause null reference exceptions since Chemical must
			// be defined in order to instantiate any EoS class.
			DropdownSpecies.SelectedIndex = 0;

			// Initializes equation of state list in EoS dropdown.
			// Note the use of EoSFactory instead of the EoS object directly.
			// Basically like passing around the idea of an EoS instead of passing around a specific EoS instance.
			DropdownEoS.Items.Add(new VanDerWaalsEOSFactory());
			DropdownEoS.Items.Add(new PengRobinsonEOSFactory());
			DropdownEoS.Items.Add(new ModSolidLiquidVaporEOSFactory());

			// Sets input values to defaults
			ViewModel.T = 273;
			ViewModel.P = 101325;
			//ViewModel.EoSFactory = new PengRobinsonEOSFactory();
			//ToggleSCurve.IsOn = false;

			UpdateValidationStyles();
		}

		/// <summary>
		/// Checks inputs and packages into core objects.
		/// </summary>
		private void RunCalc(object sender, RoutedEventArgs e)
		{
			if (CheckInvalidPageInputs())
			{
				ErrorDialog.ShowErrorDialog("An error was detected in the model inputs. Review the errors listed and try again.");
				return;
			}

			UpdateData(ViewModel.ToModel(), ViewModel.T, ViewModel.P);
		}

		/// <summary>
		/// Updates styles on input controls to signal input validation.
		/// </summary>
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

			// Error: Missing model-wide inputs.
			if (T <= 0 || double.IsNaN(T)) missingInputs.Add("Temperature");
			if (P <= 0 || double.IsNaN(P)) missingInputs.Add("Pressure");
			if (DropdownEoS.SelectedItem is null) missingInputs.Add("Equation of state");

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
		/// Runs calculations, updates DataLabel with outputs, and generates plots.
		/// </summary>
		private void UpdateData(EquationOfState EoS, Temperature T, Pressure P)
		{
			// Reset DataLabel in case numeric output options aren't selected.
			DataLabel.Text = "";
			TextNoOutputDataLabel.Visibility = Visibility.Visible;
			// Reset plot segmented control in case plot output option isn't selected.
			PlotView1.Model = null;
			PlotView1.Visibility = Visibility.Collapsed;
			PlotSeg1.Content = "Plot 1 (empty)";
			PlotView2.Model = null;
			PlotView2.Visibility = Visibility.Collapsed;
			PlotSeg2.Content = "Plot 2 (empty)";
			PlotView3.Model = null;
			PlotView3.Visibility = Visibility.Collapsed;
			PlotSeg3.Content = "Plot 3 (empty)";
			PlotView4.Model = null;
			PlotView4.Visibility = Visibility.Collapsed;
			PlotSeg4.Content = "Plot 4 (empty)";

			// Get list of all selected output options.
			var selectedOutputs = ButtonSelectOutputItems.ViewModel.SelectedOutputOptions;
			List<string> outputStrings = new List<string>();
			Dictionary<string, PhaseOutputData> phasesOutputDict = new Dictionary<string, PhaseOutputData>();
			List<(OxyPlot.PlotModel model, string title)> outputPlots = new List<(OxyPlot.PlotModel, string)>();
			try
			{
				// Get molar volumes of each phase, then determine equilibrium states
				var phases = EoS.PhaseFinder(T, P, true);
				// Create empty PhaseOutputData objects for adding data to later.
				foreach (var pair in phases)
				{
					phasesOutputDict.Add(pair.Key, new PhaseOutputData(pair.Key));
				}

				// Calculate outputs.
				foreach (var item in selectedOutputs)
				{
					if (item.Type == OutputItem.ItemType.Folder) continue;

					switch (item.OutputName)
					{
						default:
							break;

						case "VaporPressure":
							var Pvap = EoS.VaporPressure(T);
							if (double.IsNaN(Pvap))
							{
								outputStrings.Add(item.DisplayFormat("not applicable"));
							}
							else
							{
								outputStrings.Add(item.DisplayFormat(Pvap.ToEngrNotation(5)));
							}
							break;

						case "BoilingTemperature":
							var Tboil = EoS.BoilingTemperature(P);
							if (double.IsNaN(Tboil))
							{
								outputStrings.Add(item.DisplayFormat("not applicable"));
							}
							else
							{
								outputStrings.Add(item.DisplayFormat(Tboil.ToEngrNotation(5)));
							}
							break;

						case "EquilibriumPhases":
							string phasesString = "";
							foreach (string phase in EoS.EquilibriumPhases(T, P).Keys) { phasesString += phase + ", "; }
							phasesString = phasesString.Remove(phasesString.Length - 2);
							outputStrings.Add(item.DisplayFormat(phasesString));
							break;

						case "GTPlot":
							outputPlots.Add((new GTPlotModel(EoS, P).Model, "G-T plot"));
							break;

						case "PVPlot":
							outputPlots.Add((new PVPlotModel(EoS, true).Model, "P-V plot"));
							break;

						case "PTPlot":
							outputPlots.Add((new PTPlotModel(EoS).Model, "P-T plot"));
							break;

						case "CompressibilityFactor":
							foreach (var pair in phases)
							{
								var Z = EoS.CompressibilityFactor(T, P, pair.Value).ToEngrNotation(5);
								phasesOutputDict[pair.Key].AddOutputDatum(item.DisplayFormat(Z));
							}
							break;

						case "MolarVolume":
							foreach (var pair in phases)
							{
								var V = pair.Value.ToEngrNotation(5);
								phasesOutputDict[pair.Key].AddOutputDatum(item.DisplayFormat(V));
							}
							break;

						case "MolarInternalEnergy":
							foreach (var pair in phases)
							{
								var U = EoS.ReferenceMolarInternalEnergy(T, P, pair.Value).ToEngrNotation(5);
								phasesOutputDict[pair.Key].AddOutputDatum(item.DisplayFormat(U));
							}
							break;

						case "MolarEntropy":
							foreach (var pair in phases)
							{
								var S = EoS.ReferenceMolarEntropy(T, P, pair.Value).ToEngrNotation(5);
								phasesOutputDict[pair.Key].AddOutputDatum(item.DisplayFormat(S));
							}
							break;

						case "MolarEnthalpy":
							foreach (var pair in phases)
							{
								var H = EoS.ReferenceMolarEnthalpy(T, P, pair.Value).ToEngrNotation(5);
								phasesOutputDict[pair.Key].AddOutputDatum(item.DisplayFormat(H));
							}
							break;

						case "MolarGibbsEnergy":
							foreach (var pair in phases)
							{
								var G = EoS.ReferenceMolarGibbsEnergy(T, P, pair.Value).ToEngrNotation(5);
								phasesOutputDict[pair.Key].AddOutputDatum(item.DisplayFormat(G));
							}
							break;

						case "MolarHelmholtzEnergy":
							foreach (var pair in phases)
							{
								var A = EoS.ReferenceMolarHelmholtzEnergy(T, P, pair.Value).ToEngrNotation(5);
								phasesOutputDict[pair.Key].AddOutputDatum(item.DisplayFormat(A));
							}
							break;

						case "FugacityCoefficient":
							foreach (var pair in phases)
							{
								var phi = EoS.FugacityCoeff(T, P, pair.Value).ToEngrNotation(5);
								phasesOutputDict[pair.Key].AddOutputDatum(item.DisplayFormat(phi));
							}
							break;

						case "Fugacity":
							foreach (var pair in phases)
							{
								var f = EoS.Fugacity(T, P, pair.Value).ToEngrNotation(5);
								phasesOutputDict[pair.Key].AddOutputDatum(item.DisplayFormat(f));
							}
							break;
					}
				}

				// Publicize phase output data.
				PhasesOutputList.Clear();
				foreach (var phase in phases.Keys)
				{
					if (phasesOutputDict[phase].outputData.Equals("")) continue;
					PhasesOutputList.Add(phasesOutputDict[phase]);
				}

				// Set plots.
				try
				{
					PlotView1.Visibility = Visibility.Visible;
					PlotView1.Model = outputPlots[0].model;
					PlotSeg1.Content = outputPlots[0].title;
				}
				catch
				{
					TextNoPlot1.Visibility = Visibility.Visible;
					PlotView1.Visibility = Visibility.Collapsed;
				}
				try
				{
					PlotView2.Model = outputPlots[1].model;
					PlotView2.Visibility = Visibility.Visible;
					PlotSeg2.Content = outputPlots[1].title;
				}
				catch
				{
					TextNoPlot2.Visibility = Visibility.Visible;
					PlotView2.Visibility = Visibility.Collapsed;
				}
				try
				{
					PlotView3.Model = outputPlots[2].model;
					PlotView3.Visibility = Visibility.Visible;
					PlotSeg3.Content = outputPlots[2].title;
				}
				catch
				{
					TextNoPlot3.Visibility = Visibility.Visible;
					PlotView3.Visibility = Visibility.Collapsed;
				}
				try
				{
					PlotView4.Model = outputPlots[3].model;
					PlotView4.Visibility = Visibility.Visible;
					PlotSeg4.Content = outputPlots[3].title;
				}
				catch
				{
					TextNoPlot4.Visibility = Visibility.Visible;
					PlotView4.Visibility = Visibility.Collapsed;
				}


				// Display reference state used for calculations.
				//DataLabel.Text =
				//	$"Reference state: {EoS.ReferenceState.refT.ToEngrNotation()}, {EoS.ReferenceState.refP.ToEngrNotation()}";

				// Combine all output strings into DataLabel.
				if (outputStrings.Count == 0 && PhasesOutputList.Count == 0)
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
				return;
			}
		}

		private void UpdateValidationStyles()
		{
			Temperature T = NumBoxT.GetValue();
			Pressure P = NumBoxP.GetValue();

			var SelectedOutputs = ButtonSelectOutputItems.GetSelectedOutputs();
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

			if (DropdownEoS.SelectedItem is null)
			{
				DropdownEoS.Style = this.FindResource("ComboBoxErrorStyle") as Style;
				InfoBadgeDropdownEoS.Style = this.FindResource("ControlErrorInfoBadgeStyle") as Style;
				InfoBadgeDropdownEoS.Visibility = Visibility.Visible;
			}
			else
			{
				DropdownEoS.Style = this.FindResource("ComboBoxDefaultStyle") as Style;
				InfoBadgeDropdownEoS.Visibility = Visibility.Collapsed;
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
					OutputName = "VaporPressure",
					DisplayName = "Vapor pressure\n[Pa]",
					DisplayFormat = x => $"Vapor pressure, Pvap: {x}"
				},
				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "BoilingTemperature",
					DisplayName = "Boiling temperature\n[K]",
					DisplayFormat = x => $"Boiling temperature, Tboil: {x}"
				},
				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "EquilibriumPhases",
					DisplayName = "Phases at equilibrium",
					DisplayFormat = x => $"Equilibrium phases: {x}"
				},
				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "CompressibilityFactor",
					DisplayName = "Compressibility factor\n[1]",
					DisplayFormat = x => $"Compressibility factor, Z: {x}"
				},
				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "MolarVolume",
					DisplayName = "Molar volume\n[m³/mol]",
					DisplayFormat = x => $"Molar volume, V: {x}"
				},
				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "MolarInternalEnergy",
					DisplayName = "Molar internal energy\n[m³/mol]",
					DisplayFormat = x => $"Molar internal energy, U: {x}"
				},
				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "MolarEntropy",
					DisplayName = "Molar entropy\n[J/K/mol]",
					DisplayFormat = x => $"Molar entropy, S: {x}"
				},
				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "MolarEnthalpy",
					DisplayName = "Molar enthalpy\n[J/mol]",
					DisplayFormat = x => $"Molar enthalpy, H: {x}"
				},
				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "MolarGibbsEnergy",
					DisplayName = "Molar Gibbs energy\n[J/mol]",
					DisplayFormat = x => $"Molar Gibbs energy, G: {x}"
				},
				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "MolarHelmholtzEnergy",
					DisplayName = "Molar Helmholtz energy\n[J/mol]",
					DisplayFormat = x => $"Molar Helmholtz energy, A: {x}"
				},
				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "FugacityCoefficient",
					DisplayName = "Fugacity coefficient\n[1]",
					DisplayFormat = x => $"Fugacity coefficient, φ: {x}"
				},
				new OutputItem(OutputItem.ItemType.Number)
				{
					OutputName = "Fugacity",
					DisplayName = "Fugacity\n[Pa]",
					DisplayFormat = x => $"Fugacity, f: {x}"
				},
				new OutputItem(OutputItem.ItemType.Plot)
				{
					OutputName = "GTPlot",
					DisplayName = "Molar Gibbs energy vs temperature \n[J/mol vs K]"
				},
				new OutputItem(OutputItem.ItemType.Plot)
				{
					OutputName = "GPPlot",
					DisplayName = "Molar Gibbs energy vs pressure \n[J/mol vs Pa]"
				},
				new OutputItem(OutputItem.ItemType.Plot)
				{
					OutputName = "PVPlot",
					DisplayName = "Pressure vs molar volume \n[Pa vs m³/mol]"
				},
				new OutputItem(OutputItem.ItemType.Plot)
				{
					OutputName = "PTPlot",
					DisplayName = "Pressure vs temperature \n[Pa vs K]"
				}
			};
		}

		private void SelectedOutputOptions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			UpdateValidationStyles();
		}

		private void Dropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdateValidationStyles();
		}

		private void NumBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
		{
			UpdateValidationStyles();
		}
	}

	/// <summary>
	/// Stores output data for a single phase (in a pre-formatted string) for use in PhaseResultsTemplate
	/// </summary>
	public class PhaseOutputData
	{
		public string phase;
		public string phaseText;
		public string outputData;

		public PhaseOutputData(string phase)
		{
			this.phase = char.ToUpper(phase[0]).ToString() + phase.TrimStart(phase[0]);
			phaseText = this.phase + " phase";
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
