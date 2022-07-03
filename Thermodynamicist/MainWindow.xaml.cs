using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Core;
using Core.EquationsOfState;
using Core.VariableTypes;

namespace Thermodynamicist
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			Topmost = true;

			PengRobinsonEOS PREoS = new PengRobinsonEOS(Chemical.Water);
			var T = new Temperature(500);
			var P = new Pressure(101.325e3);
			var VMol = new [] { PREoS.PhaseFinder(T, P, true).L, PREoS.PhaseFinder(T, P, true).V };
			var Z = new [] { PREoS.CompressibilityFactor(T, P, VMol[0]), PREoS.CompressibilityFactor(T,P,VMol[1]) };
			var H = new [] { PREoS.ReferenceMolarEnthalpy(T, P, VMol[0]), PREoS.ReferenceMolarEnthalpy(T, P, VMol[1]) };
			var S = new [] { PREoS.ReferenceMolarEntropy(T, P, VMol[0]), PREoS.ReferenceMolarEntropy(T, P, VMol[1]) };
			var f = new [] { PREoS.Fugacity(T, P, VMol[0]), PREoS.Fugacity(T, P, VMol[1]) };

			var stateData =
				"Reference state: (" + 298.15 + " K, " + Display.DoubleToEngrNotation(100e3) + " Pa) \n" + 
				"Interested state: (" + (double)T + " K, " + (double)P + " Pa) \n";
			var phaseDataLiquid =
				"Z = " + Z[0] + "\n" + 
				"V = " + (double)VMol[0] + " m³/mol \n" +
				"H = " + (double)H[0] + " J/mol \n" +
				"S = " + (double)S[0] + " J/mol/K \n" +
				"f = " + f[0] + " Pa \n";
			var phaseDataVapor =
				"Z = " + Z[1] + "\n" + 
				"V = " + (double)VMol[1] + " m³/mol \n" +
				"H = " + (double)H[1] + " J/mol \n" +
				"S = " + (double)S[1] + " J/mol/K \n" +
				"f = " + f[1] + " Pa \n";

			DataLabel.Content = stateData;
			GroupBoxVapor.Content = phaseDataVapor;
			GroupBoxLiquid.Content = phaseDataLiquid;
			MainGrid.ShowGridLines = true;
		}
	}
}