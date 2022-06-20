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
			var VMol = PREoS.PhaseFinder(T, P);
			VMol.VMol_L = 0.026747e-3;
			var Z = new [] { PREoS.CompressibilityFactor(T, P, VMol.VMol_L), PREoS.CompressibilityFactor(T,P,VMol.VMol_V) };
			var H = new [] { PREoS.ReferenceMolarEnthalpy(T, P, VMol.VMol_L), PREoS.ReferenceMolarEnthalpy(T, P, VMol.VMol_V)};
			var S = new [] { PREoS.ReferenceMolarEntropy(T, P, VMol.VMol_L), PREoS.ReferenceMolarEntropy(T, P, VMol.VMol_V) };
			var f = new [] { PREoS.Fugacity(T, P, VMol.VMol_L), PREoS.Fugacity(T, P, VMol.VMol_L) };
			
			var labelTesting = new Label
			{
				Content = 
					"Reference state: (" +298.15+" K, "+100e3+" Pa) \n" + 
					"Interested state: (" +(double)T+" K, "+(double)P+" Pa) \n" +
					"z(l) = " + Z[0] + "\n" +
					"V(l) = " + (double)VMol.VMol_L + " m³/mol \n" +
					"H(l) = " + (double)H[0] + " J/mol \n" +
					"S(l) = " + (double)S[0] + " J/mol/K \n" +
					"f(l) = " + f[1] + " Pa \n" +
					"z(g) = " + Z[1] + "\n" +
					"V(g) = " + (double)VMol.VMol_V + " m³/mol \n" +
					"H(g) = " + (double)H[1] + " J/mol \n" +
					"S(g) = " + (double)S[1] + " J/mol/K \n" +
					"f(g) = " + f[1] + " Pa \n"
			};
			
			var stackPanel = new StackPanel();
			stackPanel.Children.Add(labelTesting);
			Content = stackPanel;
		}
	}
}