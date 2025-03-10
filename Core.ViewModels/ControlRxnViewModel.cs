using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Core.ViewModels
{
	public partial class ControlRxnViewModel : ObservableObject
	{
		[ObservableProperty]
		private ObservableCollection<ControlRxnSpeciesViewModel> _items = [];

		[ObservableProperty]
		private IRelayCommand<ControlRxnSpeciesViewModel> _deleteCommand;

		public ControlRxnViewModel()
		{
			DeleteCommand = new RelayCommand<ControlRxnSpeciesViewModel>(DeleteItem);
		}

		public void DeleteItem(ControlRxnSpeciesViewModel? rxnSpeciesViewModel)
		{
			if (rxnSpeciesViewModel is null)
				return;

			Items.Remove(rxnSpeciesViewModel);
		}
	}
}
