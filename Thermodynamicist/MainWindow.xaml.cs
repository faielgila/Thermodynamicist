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

			Temperature T = 373.15;
			Pressure P = 101.325e3;
			CubicEquationOfState PREoS = new PengRobinsonEOS(Chemical.Water);
			var VMol = PREoS.PhaseFinder(T, P);
			var labelL = new Label
			{
				Content = "Liquid phase: \n" +
				          "z = " + PREoS.CompressibilityFactor(T, P, VMol.VMol_L) + "\n" +
				          "f = " + PREoS.FugacityCoeff(T, P, VMol.VMol_L)*P + " Pa \n" +
				          "V = " + (double)VMol.VMol_L + " m³/mol \n" +
				          "P = " + (double)PREoS.Pressure(T, VMol.VMol_L) + " Pa"
			};
			var labelV = new Label
			{
				Content = "Vapor phase: \n" +
				          "z = " + PREoS.CompressibilityFactor(T, P, VMol.VMol_V) + "\n" +
				          "f = " + PREoS.FugacityCoeff(T, P, VMol.VMol_V) * P + " Pa \n" +
				          "V = " + (double)VMol.VMol_V + " m³/mol \n" +
				          "P = " + (double)PREoS.Pressure(T, VMol.VMol_V) + " Pa"
			};
			var stackPanel = new StackPanel();
			stackPanel.Children.Add(labelL);
			stackPanel.Children.Add(labelV);
			Content = stackPanel;
		}
	}
}