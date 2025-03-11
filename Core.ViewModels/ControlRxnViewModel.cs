using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Core.ViewModels;

/// <summary>
/// ViewModel for ControlRxn (or specifically, PageRxnKin).
/// This is an Observable wrapper of <see cref="Reaction"/> for use in <see cref="ControlRxnViewModel"/>.
/// </summary>
public partial class ControlRxnViewModel : ObservableObject
{
	/// <summary>
	/// Stores a list of <see cref="ControlRxnSpeciesViewModel"/>s as an Observable
	/// so the list is bound to the UI.
	/// </summary>
	[ObservableProperty]
	private ObservableCollection<ControlRxnSpeciesViewModel> _items = [];

	/// <summary>
	/// Stores reference to <see cref="DeleteItem(ControlRxnSpeciesViewModel?)"/> to be passed on to
	/// <see cref="ControlRxnSpeciesViewModel.DeleteCommand"/>.
	/// Gives the "Delete species" button in <see cref="ControlRxnSpecies"/> the ability to remove itself from the ViewModel list.
	/// </summary>
	[ObservableProperty]
	private IRelayCommand<ControlRxnSpeciesViewModel> _deleteCommand;

	public ControlRxnViewModel()
	{
		// Pass down the DeleteItem command to each ControlRxnSpeciesViewModel.
		DeleteCommand = new RelayCommand<ControlRxnSpeciesViewModel>(DeleteItem);
	}

	/// <summary>
	/// Removes the specified <see cref="ControlRxnSpeciesViewModel"/> from the ObservableCollection <see cref="Items"/>.
	/// </summary>
	/// <param name="rxnSpeciesViewModel">The species view model to remove from the list.</param>
	public void DeleteItem(ControlRxnSpeciesViewModel? rxnSpeciesViewModel)
	{
		if (rxnSpeciesViewModel is null)
			return;

		Items.Remove(rxnSpeciesViewModel);
	}

	/// <summary>
	/// Converts <see cref="_items"/> into a usable form for instatiating <see cref="Reaction"/>.
	/// </summary>
	/// <returns>List of <see cref="RxnSpecies"/> items whose data is defined by <see cref="_items"/>.</returns>
	public List<RxnSpecies> GetRxnSpeciesList()
	{
		return Items.Select(i => i.ToModel()).ToList();
	}
}
