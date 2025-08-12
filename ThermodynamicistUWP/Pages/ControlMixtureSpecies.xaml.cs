using CommunityToolkit.Mvvm.Input;
using Core;
using Core.EquationsOfState;
using Core.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace ThermodynamicistUWP
{

	/// <summary>
	/// Defines how the ControlMixtureSpecies user control should interact with the rest of the program.
	/// </summary>
	public sealed partial class ControlMixtureSpecies : UserControl
	{
		/// <summary>
		/// Stores a ViewModel for this species. Mirrors <see cref="Core.Multicomponent.MixtureSpecies"/>.
		/// All inputs in this user control are bound to this field.
		/// </summary>
		public ControlMixtureSpeciesViewModel ViewModel
		{
			// Converts the ViewModelProperty into a usable ViewModel.
			get => (ControlMixtureSpeciesViewModel)GetValue(ViewModelProperty);
			// Sets the ViewModelProperty to the new ViewModel.
			set => SetValue(ViewModelProperty, value);
		}

		/// <summary>
		/// Allows ViewModel to be bound in the XAML specification of this control.
		/// </summary>
		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
			nameof(ViewModel), typeof(ControlMixtureSpeciesViewModel), typeof(ControlMixtureSpecies), new PropertyMetadata(null));

		public ControlMixtureSpecies()
		{
			InitializeComponent();

			// Initialize DropdownSpecies to a value (arbitrarily chosen to be 0).
			// Not including this init will cause null reference exceptions since Chemical must
			// be defined in order to instantiate any EoS class.
			DropdownSpecies.SelectedIndex = 0;

			// Initializes equation of state list in EoS dropdown.
			// Note the use of EoSFactory instead of the EoS object directly.
			// Basically like passing around the idea of an EoS instead of passing around a specific EoS instance.
			DropdownEoS.Items.Add(new VanDerWaalsEOSFactory());
			DropdownEoS.Items.Add(new PengRobinsonEOSFactory());
			DropdownEoS.Items.Add(new ModSolidLiquidVaporEOSFactory());
		}

		/// <summary>
		/// Validates all input properties.
		/// </summary>
		/// <returns>true if all inputs are valid.</returns>
		public bool CheckValidInput()
		{
			if (DropdownEoS.SelectedItem is null ||
				DropdownPhase.SelectedItem is null ||
				DropdownSpecies.SelectedItem == null ||
				NumBoxMoleFraction.Value is double.NaN)
			{
				return false;
			}

			return true;
		}
	}
}
