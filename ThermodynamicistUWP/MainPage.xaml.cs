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
			// Get molar volumes of each phase, then determine equilibrium states
			var phaseVMols = EoS.PhaseFinder(T, P, true);
			var phaseEquilibriums = EoS.IsStateInPhaseEquilbirum(T, P, phaseVMols.L, phaseVMols.V);

			string phasesString = "\nPhases at equilibrium: ";
			if (phaseEquilibriums.L == 1) phasesString += "liquid";
			if (phaseEquilibriums.L == 1 && phaseEquilibriums.V == 1) phasesString += ", ";
			if (phaseEquilibriums.V == 1) phasesString += "vapor";

			var Pvap = EoS.VaporPressure(T);
			string PvapString;
			if (!Double.IsNaN(Pvap.Value))
			{
				PvapString = "\nVapor pressure: " + EoS.VaporPressure(T).Value.ToEngrNotation(5) + " Pa";
			} else PvapString = "";

			var stateData =
				"Reference state: (" + EoS.ReferenceState.refT.Value.ToEngrNotation() + " K, " + EoS.ReferenceState.refP.Value.ToEngrNotation() + " Pa)" +
				phasesString + PvapString;

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
