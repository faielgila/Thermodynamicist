using CommunityToolkit.WinUI;
using Core.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ThermodynamicistUWP
{
	public sealed partial class OutputOptionsButton : UserControl
	{
		public OutputSelectionPopupViewModel ViewModel { get; } = new OutputSelectionPopupViewModel();


		/// <summary>
		/// Stores the event handler for the 'Click' event, to be exposed in ClickProperty for use in the parent Page.
		/// </summary>
		public string Click
		{
			get { return (string)GetValue(ClickProperty); }
			set { SetValue(ClickProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Click.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ClickProperty =
			DependencyProperty.Register("Click", typeof(string), typeof(OutputOptionsButton), new PropertyMetadata(0));
		public ObservableCollection<OutputItem> AllOutputOptions
		{
			get { return (ObservableCollection<OutputItem>)GetValue(AllOutputOptionsProperty); }
			set { SetValue(AllOutputOptionsProperty, value); }
		}

		// Using a DependencyProperty as the backing store for AllOutputOptions. This enables animation, styling, binding, etc...
		public static readonly DependencyProperty AllOutputOptionsProperty =
			DependencyProperty.Register(
				"AllOutputOptions",
				typeof(ObservableCollection<OutputItem>),
				typeof(OutputOptionsButton),
				new PropertyMetadata(0));


		public OutputOptionsButton()
		{
			this.InitializeComponent();

			TextNoOutputsSelected.Text = "There are no outputs selected.\nSelect one from the list and click '>' to add them.";
			TextNoOutputsAvailable.Text = "There are no other outputs available.";
		}

		private void ButtonSelectOutputItems_Click(object sender, RoutedEventArgs e)
		{
			// TODO: For some reason, moving this to the initializer (where it really should be) makes
			//       XAML compiler try to cast an int into an ObservableCollection<OutputItem>...
			ViewModel.AvailableOutputOptions = AllOutputOptions;

			UpdateMarks();

			// Open the output selection popup if not already open.
			if (!PopupSelectOutputItems.IsOpen) { PopupSelectOutputItems.IsOpen = true; }
		}

		private void ButtonAddOutputItem_Click(object sender, RoutedEventArgs e)
		{
			// Move all selected items from left to right list.
			var selected = ListOutputItems_Left.SelectedItems.Cast<OutputItem>().ToList();
			foreach (var item in selected)
			{
				ViewModel.AvailableOutputOptions.Remove(item);
				ViewModel.SelectedOutputOptions.Add(item);
			}
			UpdateMarks();
		}

		private void ButtonRemoveOutputItem_Click(object sender, RoutedEventArgs e)
		{
			// Move all selected items from right to left list.
			var selected = ListOutputItems_Right.SelectedItems.Cast<OutputItem>().ToList();
			foreach (var item in selected)
			{
				ViewModel.SelectedOutputOptions.Remove(item);
				ViewModel.AvailableOutputOptions.Add(item);
			}
			UpdateMarks();
		}

		private void PopupSelectOutputItems_Closed(object sender, object e)
		{
			UpdateMarks();
		}

		public void MarkWithWarning()
		{
			ButtonSelectOutputItems.Style = this.FindResource("WarningOutputButtonStyle") as Style;
			AniIconButtonSelectOutputItems.Foreground = this.FindResource("SystemFillColorCautionBrush") as Brush;
			InfoBadgeSelectOutputItems.Visibility = Visibility.Visible;
		}

		public void ClearMarks()
		{
			ButtonSelectOutputItems.Style = this.FindResource("DefaultOutputButtonStyle") as Style;
			AniIconButtonSelectOutputItems.Foreground = this.FindResource("TextFillColorPrimaryBrush") as Brush;
			InfoBadgeSelectOutputItems.Visibility = Visibility.Collapsed;
		}

		public void UpdateMarks()
		{
			if (ViewModel.SelectedOutputOptions is null || ViewModel.SelectedOutputOptions.Count == 0)
			{
				MarkWithWarning();
				TextNoOutputsSelected.Visibility = Visibility.Visible;
			} else {
				ClearMarks();
				TextNoOutputsSelected.Visibility = Visibility.Collapsed;
			}

			if (ViewModel.AvailableOutputOptions is null || ViewModel.AvailableOutputOptions.Count == 0)
			{
				TextNoOutputsAvailable.Visibility = Visibility.Visible;
			} else
			{
				TextNoOutputsAvailable.Visibility = Visibility.Collapsed;
			}
		}
	}
}
