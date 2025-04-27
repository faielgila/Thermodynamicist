using Core.EquationsOfState;
using Core.VariableTypes;
using Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Core.ViewModels;
using ThermodynamicistUWP.Dialogs;

namespace ThermodynamicistUWP
{
	public sealed partial class PagePCSF : Page
	{
		public PCSFViewModel ViewModel { get; } = new PCSFViewModel();

		public PagePCSF()
		{
			InitializeComponent();

			// Set "show plots" to false as default behavior.
			TogglePlots.IsOn = false;
			ToggleSCurve.IsEnabled = false;
			PlotViewLeft.Visibility = Visibility.Collapsed;
			PlotViewRight.Visibility = Visibility.Collapsed;

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
			//ViewModel.Chemical = Chemical.CarbonDioxide;
			//ViewModel.EoSFactory = new PengRobinsonEOSFactory();
			//ToggleSCurve.IsOn = false;
		}

		/// <summary>
		/// Runs calculations, updates DataLabel with outputs, and generates plots.
		/// </summary>
		private void UpdateData(EquationOfState EoS, Temperature T, Pressure P)
		{
			try
			{
				// Get molar volumes of each phase, then determine equilibrium states
				var phases = EoS.PhaseFinder(T, P, true);

				string phasesString = "\nPhases at equilibrium: ";
				foreach (string phase in EoS.EquilibriumPhases(T, P).Keys) { phasesString += phase + ", "; }
				phasesString = phasesString.Remove(phasesString.Length - 2);

				// Calculate and display vapor pressure, if applicable
				var Pvap = EoS.VaporPressure(T);
				string PvapString;
				if (!double.IsNaN(Pvap.Value))
				{
					PvapString = "\nVapor pressure: " + Pvap.ToEngrNotation(5);
					PvapString += "\nBoiling temperature: " + EoS.BoilingTemperature(P).ToEngrNotation(5);
				}
				else PvapString = "";

				// Display reference state used for calculations
				var stateData =
					"Reference state: (" + EoS.ReferenceState.refT.ToEngrNotation() + ", " + EoS.ReferenceState.refP.ToEngrNotation() + ")" +
					phasesString + PvapString;

				// Calculate and display state variables for each phase
				DataLabel.Text = stateData;
				if (phases.ContainsKey("vapor"))
				{
					GroupBoxVapor.Text = "Vapor phase data: \n" + Display.GetAllStateVariablesFormatted(EoS, T, P, phases["vapor"], 5);
				}
				else
				{
					GroupBoxVapor.Text = "Vapor phase data: \n indeterminate";
				}
				if (phases.ContainsKey("liquid"))
				{
					GroupBoxLiquid.Text = "Liquid phase data: \n" + Display.GetAllStateVariablesFormatted(EoS, T, P, phases["liquid"], 5);
				}
				else
				{
					GroupBoxLiquid.Text = "Liquid phase data: \n indeterminate";
				}

				if (phases.ContainsKey("solid"))
				{
					GroupBoxSolid.Text = "Solid phase data: \n" + Display.GetAllStateVariablesFormatted(EoS, T, P, phases["solid"], 5);
				}
				else
				{
					GroupBoxSolid.Text = "Solid phase data: \n not calculated";
				}

				if (TogglePlots.IsOn) UpdatePlots();
			}
			catch (Exception e)
			{
				ErrorDialog.ShowErrorDialog(e.Message);
				return;
			}
		}

		/// <summary>
		/// Checks inputs and packages into core objects.
		/// </summary>
		private void RunCalc(object sender, RoutedEventArgs e)
		{
			// If any inputs are not set, do not attempt to run calculations!
			if (
				double.IsNaN(NumBoxT.Value) || NumBoxT.Value == 0 ||
				double.IsNaN(NumBoxP.Value) || NumBoxP.Value == 0 ||
				DropdownSpecies.SelectedItem == null ||
				DropdownEoS.SelectedItem == null
				)
			{
				ErrorDialog.ShowErrorDialog("Not all required inputs are set.");
				return;
			}

			try
			{
				UpdateData(ViewModel.ToModel(), ViewModel.T, ViewModel.P);
			}
			catch (Exception exception)
			{
				ErrorDialog.ShowErrorDialog(exception.Message);
			}
		}

		private void UpdatePlots()
		{
			try
			{
				var EoS = ViewModel.ToModel();
				// Fills in the plot view with a view model using the new settings 
				PlotViewLeft.Model = new PVPlotModel(EoS, ToggleSCurve.IsOn).Model;
				//PlotViewLeft.Model = new GenericViewModel().Model;
				PlotViewRight.Model = new GTPlotModel(EoS, ViewModel.P).Model;
			}
			catch (Exception exception)
			{
				ErrorDialog.ShowErrorDialog(exception.Message);
				return;
			}
		}

		private void ToggleSCurve_Toggled(object sender, RoutedEventArgs e)
		{
			//
		}

		private void TogglePlots_Toggled(object sender, RoutedEventArgs e)
		{
			if (TogglePlots.IsOn)
			{
				ToggleSCurve.IsEnabled = true;
				PlotViewLeft.Visibility = Visibility.Visible;
				PlotViewRight.Visibility = Visibility.Visible;
			} else
			{
				ToggleSCurve.IsEnabled = false;
				PlotViewLeft.Visibility = Visibility.Collapsed;
				PlotViewRight.Visibility = Visibility.Collapsed;
			}
		}
	}
}
