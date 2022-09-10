using System;
using System.Collections.Generic;
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
using Core;
using Core.EquationsOfState;
using Core.VariableTypes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ThermodynamicistUWP
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			this.InitializeComponent();

			// Initializes chemical list in species dropdown
			foreach (Chemical chemical in Enum.GetValues(typeof(Chemical)))
			{
				string item = Constants.ChemicalNames[chemical];
				DropdownSpecies.Items.Add(item);
			}
		}
		/*
		private void UpdateData(CubicEquationOfState EoS, Temperature T, Pressure P)
		{
			var phaseVMols = EoS.PhaseFinder(T, P, true);
			var phaseEquilibriums = EoS.IsStateInPhaseEquilbirum(T, P, phaseVMols.L, phaseVMols.V);

			string phasesString = "";
			if (phaseEquilibriums.L && phaseEquilibriums.V) phasesString = "liquid, vapor";
			if (!phaseEquilibriums.L && phaseEquilibriums.V) phasesString = "vapor";
			if (phaseEquilibriums.L && !phaseEquilibriums.V) phasesString = "liquid";

			var stateData =
				"Reference state: (" + 298.15.ToEngrNotation() + " K, " + 100e3.ToEngrNotation() + " Pa) \n" +
				"Phases at equilibrium: " + phasesString;

			DataLabel.Text = stateData;
			GroupBoxVapor.Text = "Vapor phase data: \n" + Display.GetAllStateVariablesFormatted(EoS, T, P, phaseVMols.V, 5);
			GroupBoxLiquid.Text = "Liquid phase data: \n" + Display.GetAllStateVariablesFormatted(EoS, T, P, phaseVMols.L, 5);
		}

        private void ButtonRecalc_Click(object sender, RoutedEventArgs e)
        {
			var T = new Temperature(NumBoxT.Value);
			var P = new Pressure(NumBoxP.Value);
			Chemical species = Constants.ChemicalNames.FirstOrDefault(
				x => x.Value == DropdownSpecies.SelectedValue.ToString()).Key;
			PengRobinsonEOS PREoS = new PengRobinsonEOS(species);
			UpdateData(PREoS, T, P);
		}

        private void DropdownSpeciesInit(object sender, RoutedEventArgs e)
        {
			
        }
    }
}
