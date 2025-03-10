using Core.EquationsOfState;
using Core.VariableTypes;
using Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ThermodynamicistUWP
{
	public sealed partial class PagePCSF : Page
	{
		public PagePCSF()
		{
			InitializeComponent();

			// Initializes chemical list in species dropdown
			foreach (Chemical chemical in Enum.GetValues(typeof(Chemical)))
			{
				string item = Constants.ChemicalNames[chemical];
				DropdownSpecies.Items.Add(item);
			}

			// Initializes equation of state list in EoS dropdown
			DropdownEoS.Items.Add("van der Waals");
			DropdownEoS.Items.Add("Peng-Robinson");
			DropdownEoS.Items.Add("modified Solid-Liquid-Vapor");

			// Sets input values to defaults
			NumBoxT.Value = 273;
			NumBoxP.Value = 101325;
			DropdownSpecies.SelectedValue = "Carbon dioxide";
			DropdownEoS.SelectedValue = "Peng-Robinson";
			ToggleSCurve.IsOn = false;

			// Update all outputs
			UpdateData(new PengRobinsonEOS(Chemical.CarbonDioxide), NumBoxT.Value, NumBoxP.Value);
		}

		/// <summary>
		/// Runs calculations, updates DataLabel with outputs, and generates plots.
		/// </summary>
		private void UpdateData(EquationOfState EoS, Temperature T, Pressure P)
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

			// Fills in the plot view with a view model using the new settings
			PlotViewLeft.Model = new PVViewModel(EoS, ToggleSCurve.IsOn).Model;
			//PlotViewLeft.Model = new GenericViewModel().Model;
			PlotViewRight.Model = new GTViewModel(EoS, P).Model;
		}

		/// <summary>
		/// Checks inputs and packages into core objects.
		/// </summary>
		private void RefreshCalculations(object sender, RoutedEventArgs e)
		{
			// If any inputs are not set, do not attempt to run calculations!
			if (
				double.IsNaN(NumBoxT.Value) || NumBoxT.Value == 0 ||
				double.IsNaN(NumBoxP.Value) || NumBoxP.Value == 0 ||
				DropdownSpecies.SelectedItem == null ||
				DropdownEoS.SelectedItem == null
				) return;

			var T = new Temperature(NumBoxT.Value);
			var P = new Pressure(NumBoxP.Value);
			Chemical species = Constants.ChemicalNames.FirstOrDefault(
				x => x.Value == DropdownSpecies.SelectedValue.ToString()).Key;
			EquationOfState EoS;
			switch (DropdownEoS.SelectedValue.ToString())
			{
				case "van der Waals":
					EoS = new VanDerWaalsEOS(species);
					break;
				case "Peng-Robinson":
					EoS = new PengRobinsonEOS(species);
					break;
				case "modified Solid-Liquid-Vapor":
					EoS = new ModSolidLiquidVaporEOS(species);
					break;
				default:
					EoS = new VanDerWaalsEOS(species);
					break;
			}
			UpdateData(EoS, T, P);
		}

		// TODO: This seems like a terrible hack for this problem...
		private void NumBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
		{
			RefreshCalculations(sender, null);
		}
	}
}
