using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ThermodynamicistUWP
{
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			InitializeComponent();

			contentFrame.Navigate(typeof(PageHomoMix));
		}

		private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
		{
			NavigationViewItem nvi = args.SelectedItemContainer as NavigationViewItem;
			var tag = nvi.Tag as string;
			Type type = Type.GetType(tag);
			contentFrame.Navigate(type);
		}
	}
}
