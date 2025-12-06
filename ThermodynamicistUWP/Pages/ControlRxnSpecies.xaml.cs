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
		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
			nameof(ViewModel), typeof(ControlRxnSpeciesViewModel),
			typeof(ControlRxnSpecies),
			new PropertyMetadata(null));

		public bool IsConcentrationRequired
		{
			get { return (bool)GetValue(IsConcentrationRequiredProperty); }
			set { SetValue(IsConcentrationRequiredProperty, value); }
		}
		public static readonly DependencyProperty IsConcentrationRequiredProperty =
			DependencyProperty.Register(
				nameof(IsConcentrationRequired), typeof(bool),
				typeof(ControlRxnSpecies),
				new PropertyMetadata(false,
					new PropertyChangedCallback(IsConcentrationRequiredPropertyChanged)));

		public ControlRxnSpecies()
		{
			InitializeComponent();

			// Initializes equation of state list in EoS dropdown.
			// Note the use of EoSFactory instead of the EoS object directly.
			// Basically like passing around the idea of an EoS instead of passing around a specific EoS instance.
			DropdownEoS.Items.Add(new VanDerWaalsEOSFactory());
			DropdownEoS.Items.Add(new PengRobinsonEOSFactory());
			DropdownEoS.Items.Add(new ModSolidLiquidVaporEOSFactory());

			UpdateValidationStyles();
		}

		public void UpdateValidationStyles()
		{
			if (ViewModel is null)
			{
				MarkWithError_Expander();
				MarkWithError_DropdownSpecies();
				MarkWithError_Concentration();
				MarkWithError_Stoichiometry();
				MarkWithError_Phase();
				MarkWithError_EoS();
				return;
			}
			var chemical = DropdownSpecies.SelectedValue;
			var stoich = NumBoxStoich.Value;
			var conc = NumBoxConc.Value;
			var phase = DropdownPhase.SelectedValue;
			var EoS = DropdownEoS.SelectedValue;

			var anyError = false;


			if (chemical is null)
			{
				MarkWithError_DropdownSpecies();
				anyError = true;
			} else
			{
				ClearMarks_DropdownSpecies();
			}

			if (double.IsNaN(stoich) || stoich <= 0)
			{
				MarkWithError_Stoichiometry();
				anyError = true;
			} else
			{
				ClearMarks_Stoichiometry();
			}

			if (ViewModel.IsConcentrationRequired &&
				(double.IsNaN(conc) || conc < 0))
			{
				MarkWithError_Concentration();
				anyError = true;
			} else
			{
				ClearMarks_Concentration();
			}

			if (phase is null || phase.Equals(""))
			{
				MarkWithError_Phase();
				anyError = true;
			} else
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
			} else
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


		private void MarkWithError_Stoichiometry()
		{
			NumBoxStoich.Style = this.FindResource("NumberBoxErrorStyle") as Style;
			InfoBadgeNumBoxStoich.Style = this.FindResource("ControlErrorInfoBadgeStyle") as Style;
			InfoBadgeNumBoxStoich.Visibility = Visibility.Visible;
		}

		private void ClearMarks_Stoichiometry()
		{
			NumBoxStoich.Style = this.FindResource("NumberBoxDefaultStyle") as Style;
			InfoBadgeNumBoxStoich.Visibility = Visibility.Collapsed;
		}

		private void MarkWithError_Concentration()
		{
			NumBoxConc.Style = this.FindResource("NumberBoxErrorStyle") as Style;
			InfoBadgeNumBoxConc.Style = this.FindResource("ControlErrorInfoBadgeStyle") as Style;
			InfoBadgeNumBoxConc.Visibility = Visibility.Visible;
		}

		private void ClearMarks_Concentration()
		{
			NumBoxConc.Style = this.FindResource("NumberBoxDefaultStyle") as Style;
			InfoBadgeNumBoxConc.Visibility = Visibility.Collapsed;
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

		private static void IsConcentrationRequiredPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
		{
			ControlRxnSpecies control = (ControlRxnSpecies)d;
			control.UpdateValidationStyles();
		}
	}
}
