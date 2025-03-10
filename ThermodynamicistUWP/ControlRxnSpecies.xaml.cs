using CommunityToolkit.Mvvm.Input;
using Core;
using Core.EquationsOfState;
using Core.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace ThermodynamicistUWP
{
	public sealed partial class ControlRxnSpecies : UserControl
	{
		public ControlRxnSpeciesViewModel ViewModel
		{
			get => (ControlRxnSpeciesViewModel)GetValue(ViewModelProperty);
			set => SetValue(ViewModelProperty, value);
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
			nameof(ViewModel), typeof(ControlRxnSpeciesViewModel), typeof(ControlRxnSpecies), new PropertyMetadata(null));

		public ControlRxnSpecies()
		{
			InitializeComponent();

			DropdownSpecies.SelectedIndex = 0;

			// Initializes equation of state list in EoS dropdown
			DropdownEoS.Items.Add(new VanDerWaalsEOSFactory());
			DropdownEoS.Items.Add(new PengRobinsonEOSFactory());
			DropdownEoS.Items.Add(new ModSolidLiquidVaporEOSFactory());
		}

		private void RadioButton_Checked(object sender, RoutedEventArgs e)
		{
			if (sender.Equals(RadioProduct)) (DataContext as RxnSpecies).IsReactant = false;
			else (DataContext as RxnSpecies).IsReactant = true;
		}
	}
}
