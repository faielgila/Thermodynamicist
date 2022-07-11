using System.Windows;
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
			var phaseVMols = PREoS.PhaseFinder(T, P);

			var stateData =
				"Reference state: (" + 298.15.ToEngrNotation() + " K, " + 100e3.ToEngrNotation() + " Pa) \n" + 
				"Interested state: (" + (double)T + " K, " + (double)P + " Pa)";

			DataLabel.Content = stateData;
			GroupBoxVapor.Content = Display.GetAllStateVariablesFormatted(PREoS, T, P, phaseVMols.V, 5);
			GroupBoxLiquid.Content = Display.GetAllStateVariablesFormatted(PREoS, T, P, phaseVMols.L, 5);
		}
	}
}