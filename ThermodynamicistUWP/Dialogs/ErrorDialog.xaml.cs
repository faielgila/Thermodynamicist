using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace ThermodynamicistUWP.Dialogs
{
	public sealed partial class ErrorDialog : ContentDialog
	{
		public string ErrorText;

		public ErrorDialog()
		{
			this.InitializeComponent();
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
			await errorPopup.ShowAsync();
		}

		/// <summary>
		/// Pops up the "cancel calculation" error dialog with exception information.
		/// </summary>
		public static async void ShowErrorDialog(Exception e)
		{
			var errorPopup = new ErrorDialog
			{
				ErrorText = e.Message + "\n\nStack trace:\n" + e.StackTrace
			};
			await errorPopup.ShowAsync();
		}
	}
}
