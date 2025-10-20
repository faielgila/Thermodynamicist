using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ThermodynamicistUWP.Dialogs
{
	public sealed partial class ErrorDialog : ContentDialog
	{
		public string ErrorText;
		public string ErrorStacktrace;

		public ErrorDialog()
		{
			InitializeComponent();
			Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
		}

		/// <summary>
		/// Pops up the "cancel calculation" error dialog with the specified text.
		/// </summary>
		public static async void ShowErrorDialog(string text)
		{
			var errorPopup = new ErrorDialog
			{
				ErrorText = text
			};
			errorPopup.TextBlockTitleStacktrace.Visibility = Visibility.Collapsed;
			errorPopup.TextBlockErrorStacktrace.Visibility = Visibility.Collapsed;
			errorPopup.ScrollViewerStacktrace.Visibility = Visibility.Collapsed;
			await errorPopup.ShowAsync();
		}

		/// <summary>
		/// Pops up the "cancel calculation" error dialog with exception information.
		/// </summary>
		public static async void ShowErrorDialog(Exception e)
		{
			var errorPopup = new ErrorDialog
			{
				ErrorText = e.Message,
				ErrorStacktrace = e.StackTrace
			};
			await errorPopup.ShowAsync();
		}
	}
}
