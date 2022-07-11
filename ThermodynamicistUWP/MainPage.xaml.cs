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

            PengRobinsonEOS PREoS = new PengRobinsonEOS(Chemical.Water);
            var T = new Temperature(500);
            var P = new Pressure(101.325e3);
            var phaseVMols = PREoS.PhaseFinder(T, P, true);
            var phaseEquilibriums = PREoS.IsStateInPhaseEquilbirum(T, P, phaseVMols.L, phaseVMols.V);

            string phasesString = "";
            if ( phaseEquilibriums.L &&  phaseEquilibriums.V) phasesString = "liquid, vapor";
            if (!phaseEquilibriums.L &&  phaseEquilibriums.V) phasesString = "vapor";
            if ( phaseEquilibriums.L && !phaseEquilibriums.V) phasesString = "liquid";

            var stateData =
                "Reference state: (" + 298.15.ToEngrNotation() + " K, " + 100e3.ToEngrNotation() + " Pa) \n" +
                "Interested state: (" + (double)T + " K, " + (double)P + " Pa) \n" +
                "Phases at equilibrium: " + phasesString;

            DataLabel.Text = stateData;
            GroupBoxVapor.Text = "Vapor phase data: \n" + Display.GetAllStateVariablesFormatted(PREoS, T, P, phaseVMols.V, 5);
            GroupBoxLiquid.Text = "Liquid phase data: \n" + Display.GetAllStateVariablesFormatted(PREoS, T, P, phaseVMols.L, 5);
        }
    }
}
