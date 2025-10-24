using CommunityToolkit.Mvvm.Input;
using Core;
using Core.EquationsOfState;
using Core.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace ThermodynamicistUWP
{

	/// <summary>
	/// Defines how the ControlRxnSpecies user control should interact with the rest of the program.
	/// </summary>
	public sealed partial class ControlRxnSpecies : UserControl
	{
		/// <summary>
		/// Stores a ViewModel for this species. Mirrors <see cref="RxnSpecies"/>.
		/// All inputs in this user control are bound to this field.
		/// </summary>
		public ControlRxnSpeciesViewModel ViewModel
		{
			// Converts the ViewModelProperty into a useable ViewModel.
			get => (ControlRxnSpeciesViewModel)GetValue(ViewModelProperty);
			// Sets the ViewModelProperty to the new ViewModel.
			set => SetValue(ViewModelProperty, value);
		}

		/// <summary>
		/// Allows ViewModel to be bound in the XAML specification of this control.
		/// </summary>
		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
			nameof(ViewModel), typeof(ControlRxnSpeciesViewModel), typeof(ControlRxnSpecies), new PropertyMetadata(null));

		public ControlRxnSpecies()
		{
			InitializeComponent();

			// Initializes equation of state list in EoS dropdown.
			// Note the use of EoSFactory instead of the EoS object directly.
			// Basically like passing around the idea of an EoS instead of passing around a specific EoS instance.
			DropdownEoS.Items.Add(new VanDerWaalsEOSFactory());
			DropdownEoS.Items.Add(new PengRobinsonEOSFactory());
			DropdownEoS.Items.Add(new ModSolidLiquidVaporEOSFactory());
		}
	}
}
