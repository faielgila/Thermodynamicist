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

namespace ThermodynamicistUWP
{
	public sealed partial class MainPage : Page
	{
		public MainViewModel ViewModel
		{
			get { return (MainViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ViewModel. This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ViewModelProperty =
			DependencyProperty.Register(nameof(ViewModel), typeof(MainViewModel), typeof(MainPage), new PropertyMetadata(null));

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

		private void UpdateData(CubicEquationOfState EoS, Temperature T, Pressure P)
		{
			// Get molar volumes of each phase, then determines equilibrium
			var phaseVMols = EoS.PhaseFinder(T, P, true);
			var phaseEquilibriums = EoS.IsStateInPhaseEquilbirum(T, P, phaseVMols.L, phaseVMols.V);

			string phasesString = "";
			if (phaseEquilibriums.L == 1 && phaseEquilibriums.V == 1) phasesString = "liquid, vapor";
			if ((phaseEquilibriums.L == 0 || phaseEquilibriums.L == 2)	&& phaseEquilibriums.V == 1) phasesString = "vapor";
			if (phaseEquilibriums.L == 1 && (phaseEquilibriums.V == 0 || phaseEquilibriums.V == 2)) phasesString = "liquid";

			var stateData =
				"Reference state: (" + 298.15.ToEngrNotation() + " K, " + 100e3.ToEngrNotation() + " Pa) \n" +
				"Phases at equilibrium: " + phasesString;

			DataLabel.Text = stateData;
            if (phaseEquilibriums.V != 2)
            {
                GroupBoxVapor.Text = "Vapor phase data: \n" + Display.GetAllStateVariablesFormatted(EoS, T, P, phaseVMols.V, 5);
            }
            else
            {
                GroupBoxVapor.Text = "Vapor phase data: \n indeterminate";
            }
            if (phaseEquilibriums.L != 2)
            {
                GroupBoxLiquid.Text = "Liquid phase data: \n" + Display.GetAllStateVariablesFormatted(EoS, T, P, phaseVMols.L, 5);
            }
            else
            {
                GroupBoxLiquid.Text = "Liquid phase data: \n indeterminate";
            }

            // Creates a new view model with the new chemical and equation of state, then updates the plot view
            ViewModel = new MainViewModel(EoS);
			MainPlotView.InvalidatePlot();
		}

		private void ButtonRecalc_Click(object sender, RoutedEventArgs e)
		{
			var T = new Temperature(NumBoxT.Value);
			var P = new Pressure(NumBoxP.Value);
			Chemical species = Constants.ChemicalNames.FirstOrDefault(
				x => x.Value == DropdownSpecies.SelectedValue.ToString()).Key;
			var EoS = new PengRobinsonEOS(species);
			UpdateData(EoS, T, P);
		}
	}
}
