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

			Temperature T = 15 + 273.15;
			Pressure P = 101.325e3;
			CubicEquationOfState vdWSF = new VanDerWaalsEOS(Chemical.Water);
			MolarVolume[] VMols = vdWSF.PhaseFinder(T, P);
			/*
			Label label = new Label();
			label.Content = "Molar volume at (T,P): " + (double) vdWSF.Pressure(T, VMols[1]) + " m³/mol";

			StackPanel stackPanel = new StackPanel();
			stackPanel.Children.Add(label);
			Content = stackPanel;
			*/
		}
	}
}