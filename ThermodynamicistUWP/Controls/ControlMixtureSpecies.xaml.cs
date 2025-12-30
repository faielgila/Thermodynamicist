using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Core;
using Core.EquationsOfState;
using Core.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;


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
			nameof(ViewModel), typeof(ControlMixtureSpeciesViewModel),
			typeof(ControlMixtureSpecies),
			new PropertyMetadata(null, ViewModelPropertyChanged));

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

		private static void ViewModelPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var obj = (ControlMixtureSpecies)sender;
			if (obj.ViewModel.SuppressDeletion) obj.ButtonDelete.Visibility = Visibility.Collapsed;
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

		public void UpdateValidationStyles()
		{
			if (ViewModel is null)
			{
				MarkWithError_Expander();
				MarkWithError_DropdownSpecies();
				MarkWithError_MoleFraction();
				MarkWithError_Phase();
				MarkWithError_EoS();
				return;
			}
			var chemical = DropdownSpecies.SelectedValue;
			var moleFraction = NumBoxMoleFraction.Value;
			var phase = DropdownPhase.SelectedValue;
			var EoS = DropdownEoS.SelectedValue;

			var anyError = false;


			if (chemical is null)
			{
				MarkWithError_DropdownSpecies();
				anyError = true;
			}
			else
			{
				ClearMarks_DropdownSpecies();
			}

			if (double.IsNaN(moleFraction) || moleFraction <= 0 || moleFraction > 1)
			{
				MarkWithError_MoleFraction();
				anyError = true;
			}
			else
			{
				ClearMarks_MoleFraction();
			}

			if (phase is null || phase.Equals(""))
			{
				MarkWithError_Phase();
				anyError = true;
			}
			else
			{
				ClearMarks_Phase();
			}

			if (EoS is null)
			{
				MarkWithError_EoS();
				anyError = true;
			}
			else
			{
				ClearMarks_EoS();
			}

			if (anyError)
			{
				MarkWithError_Expander();
			}
			else
			{
				ClearMarks_Expander();
			}
		}

		#region Mark and Clear Errors functions
		private void MarkWithError_Expander()
		{
			BorderControl.BorderBrush = this.FindResource("SystemFillColorCriticalBrush") as Brush;
		}

		private void ClearMarks_Expander()
		{
			//#FFFF99A4
			BorderControl.BorderBrush = new SolidColorBrush(new Color() { R = 0xFF, G = 0x99, B = 0xA4 });
		}

		private void MarkWithError_DropdownSpecies()
		{
			DropdownSpecies.Style = this.FindResource("ComboBoxErrorStyle") as Style;
			InfoBadgeDropdownSpecies.Style = this.FindResource("ControlErrorInfoBadgeStyle") as Style;
			InfoBadgeDropdownSpecies.Visibility = Visibility.Visible;
		}
		private void ClearMarks_DropdownSpecies()
		{
			DropdownSpecies.Style = this.FindResource("ComboBoxDefaultStyle") as Style;
			InfoBadgeDropdownSpecies.Visibility = Visibility.Collapsed;
		}

		public void MarkWithError_MoleFraction()
		{
			NumBoxMoleFraction.Style = this.FindResource("NumberBoxErrorStyle") as Style;
			InfoBadgeNumBoxMoleFraction.Style = this.FindResource("ControlErrorInfoBadgeStyle") as Style;
			InfoBadgeNumBoxMoleFraction.Visibility = Visibility.Visible;
		}

		public void ClearMarks_MoleFraction()
		{
			NumBoxMoleFraction.Style = this.FindResource("NumberBoxDefaultStyle") as Style;
			InfoBadgeNumBoxMoleFraction.Visibility = Visibility.Collapsed;
		}

		private void MarkWithError_Phase()
		{
			DropdownPhase.Style = this.FindResource("ComboBoxErrorStyle") as Style;
			InfoBadgeDropdownPhase.Style = this.FindResource("ControlErrorInfoBadgeStyle") as Style;
			InfoBadgeDropdownPhase.Visibility = Visibility.Visible;
		}

		private void ClearMarks_Phase()
		{
			DropdownPhase.Style = this.FindResource("ComboBoxDefaultStyle") as Style;
			InfoBadgeDropdownPhase.Visibility = Visibility.Collapsed;
		}

		private void MarkWithError_EoS()
		{
			DropdownEoS.Style = this.FindResource("ComboBoxErrorStyle") as Style;
			InfoBadgeDropdownEoS.Style = this.FindResource("ControlErrorInfoBadgeStyle") as Style;
			InfoBadgeDropdownEoS.Visibility = Visibility.Visible;
		}

		private void ClearMarks_EoS()
		{
			DropdownEoS.Style = this.FindResource("ComboBoxDefaultStyle") as Style;
			InfoBadgeDropdownEoS.Visibility = Visibility.Collapsed;
		}
		#endregion

		private void Dropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdateValidationStyles();
		}

		private void NumBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
		{

			UpdateValidationStyles();
		}
	}
}
