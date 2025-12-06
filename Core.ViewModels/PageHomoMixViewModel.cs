using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Multicomponent;
using Core.Multicomponent.ActivityModels;

namespace Core.ViewModels;

/// <summary>
/// ViewModel for ControlRxn (or specifically, PageHomoMix).
/// This is an Observable wrapper of <see cref="Multicomponent.HomogeneousMixture"/> for use in <see cref="PageHomoMixViewModel"/>.
/// </summary>
public partial class PageHomoMixViewModel : ObservableObject
{
	/// <summary>
	/// Stores the temperature to calculate the reaction at.
	/// </summary>
	[ObservableProperty]
	private double _T;

	/// <summary>
	/// Stores the pressure to calculate the reaction at.
	/// </summary>
	[ObservableProperty]
	private double _P;

	/// <summary>
	/// Stores a list of <see cref="ControlMixtureSpeciesViewModel"/>s as an Observable
	/// so the list is bound to the UI.
	/// </summary>
	[ObservableProperty]
	private ObservableCollection<ControlMixtureSpeciesViewModel> _items = [];

	/// <summary>
	/// Stores the activity model to be used for estimating activity coefficients.
	/// </summary>
	private ActivityModel? _activityModel;

	/// <summary>
	/// Stores the IActivityModelFactory for the selected activity model.
	/// </summary>
	private IActivityModelFactory _activityModelFactory;

	/// <summary>
	/// Stores the <see cref="IActivityModelFactory"/> for the selected activity model.
	/// Modifies private <see cref="_activityModelFactory"/>.
	/// </summary>
	/// <remarks>
	/// This property is required to ensure proper setting of the relevant Observable in the ViewModel,
	/// as well as telling the ViewModel to update <see cref="_activityModel"/>.
	/// </remarks>
	public IActivityModelFactory ActivityModelFactory
	{
		get => _activityModelFactory;
		set
		{
			// Update the Observable with a reference to the new value.
			SetProperty(ref _activityModelFactory, value);
			// Tell the ViewModel to update the activity model in the ViewModel.
			UpdateActivityModel();
		}
	}

	/// <summary>
	/// Stores reference to <see cref="DeleteItem(ControlMixtureSpeciesViewModel?)"/> to be passed on to
	/// <see cref="ControlMixtureSpeciesViewModel.DeleteCommand"/>.
	/// Gives the "Delete species" button in <see cref="ControlMixtureSpecies"/> the ability to remove itself from the ViewModel list.
	/// </summary>
	[ObservableProperty]
	private IRelayCommand<ControlMixtureSpeciesViewModel> _deleteCommand;

	[ObservableProperty]
	private ObservableCollection<ErrorInfoViewModel> _errors = [];

	public PageHomoMixViewModel()
	{
		// Pass down the DeleteItem command to each ControlMixtureSpeciesViewModel.
		DeleteCommand = new RelayCommand<ControlMixtureSpeciesViewModel>(DeleteItem);
	}

	public void AddItem(ControlMixtureSpeciesViewModel? mixtureSpeciesViewModel)
	{
		if (mixtureSpeciesViewModel is null)
			return;

		Items.Add(mixtureSpeciesViewModel);
	}

	/// <summary>
	/// Removes the specified <see cref="ControlMixtureSpeciesViewModel"/> from the ObservableCollection <see cref="Items"/>.
	/// </summary>
	/// <param name="mixtureSpeciesViewModel">The species view model to remove from the list.</param>
	public void DeleteItem(ControlMixtureSpeciesViewModel? mixtureSpeciesViewModel)
	{
		if (mixtureSpeciesViewModel is null)
			return;

		Items.Remove(mixtureSpeciesViewModel);
	}

	/// <summary>
	/// Converts <see cref="_items"/> into a usable form for instantiating <see cref="HomogeneousMixture"/>.
	/// </summary>
	/// <returns>List of <see cref="MixtureSpecies"/> items whose data is defined by <see cref="_items"/>.</returns>
	public List<MixtureSpecies> GetMixtureSpeciesList()
	{
		return Items.Select(i => i.ToModel()).ToList();
	}

	/// <summary>
	/// Updates <see cref="_activityModel"/> whenever
	/// <see cref="ActivityModel"/> is changed.
	/// </summary>
	private void UpdateActivityModel()
	{
		// Should ActivityModelFactory be null, do not try to look at a non-existent instance of a factory.
		if (ActivityModelFactory is null)
		{
			_activityModel = null;
			return;
		}

		_activityModel = ActivityModelFactory.Create(GetMixtureSpeciesList());
	}
}
