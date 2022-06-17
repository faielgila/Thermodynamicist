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
			var T1 = new Temperature(298.15);
			var P1 = new Pressure(100e3);
			var VMol1 = IdealGasLaw.MolarVolume(T1, P1);
			var T2 = new Temperature(500);
			var P2 = new Pressure(101.325e3);
			var VMol2 = PREoS.PhaseFinder(T2, P2).VMol_V;
			var H = PREoS.DepartureEnthalpy(T2, P2, VMol2) + PREoS.IdealMolarEnthalpyChange(298.15, T2);
			
			var labelTesting = new Label
			{
				Content = "Molar Enthalpy Departure at point 1 = " + 
							(double)PREoS.DepartureEnthalpy(T1, P1, VMol1) + "\n" +
				          "Ideal Molar Enthalpy Change = " + 
							(double)PREoS.IdealMolarEnthalpyChange(T1, T2) + "\n" +
				          "Molar Enthalpy Departure at point 2 = " + 
							(double)PREoS.DepartureEnthalpy(T2, P2, VMol2) + "\n" +
				          "Total Enthalpy Change = " + 
							(double)PREoS.MolarEnthalpyChange(T1, P1, VMol1, T2, P2, VMol2) + "\n" +
				          "Enthalpy w/rt Reference State = " +
							(double)PREoS.ReferenceMolarEnthalpy(T2, P2, VMol2) + "\n" + 
				          "H = " + H + " J/mol"
			};
			
			var stackPanel = new StackPanel();
			stackPanel.Children.Add(labelTesting);
			Content = stackPanel;
		}
	}
}