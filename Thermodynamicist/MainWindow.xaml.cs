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

			var T = new Temperature(373.15);
			var P = new Pressure(101.325e3);
			CubicEquationOfState PREoS = new PengRobinsonEOS(Chemical.Water);
			var VMols = PREoS.PhaseFinder(T, P);
			var labelL = new Label
			{
				Content = "Liquid phase: \n" +
				          "z = " + PREoS.CompressibilityFactor(T, P, VMols.VMol_L) + "\n" +
				          "f = " + PREoS.FugacityCoeff(T, P, VMols.VMol_L)*P + " Pa \n" +
				          "V = " + (double)VMols.VMol_L + " m³/mol \n" +
				          "P = " + (double)PREoS.Pressure(T, VMols.VMol_L) + " Pa"
			};
			var labelV = new Label
			{
				Content = "Vapor phase: \n" +
				          "z = " + PREoS.CompressibilityFactor(T, P, VMols.VMol_V) + "\n" +
				          "f = " + PREoS.FugacityCoeff(T, P, VMols.VMol_V) * P + " Pa \n" +
				          "V = " + (double)VMols.VMol_V + " m³/mol \n" +
				          "P = " + (double)PREoS.Pressure(T, VMols.VMol_V) + " Pa"
			};
			var stackPanel = new StackPanel();
			stackPanel.Children.Add(labelL);
			stackPanel.Children.Add(labelV);
			Content = stackPanel;

			var enthalpy = new MolarEnthalpy(0.1, ThermoVarRelations.Departure);
		}
	}
}