using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Reactions;
using Core.Reactions.Kinetics;
using Core.VariableTypes;
using System.Collections.ObjectModel;

namespace Core.ViewModels;

/// <summary>
/// ViewModel for ControlRxn (or specifically, PageRxnKin).
/// This is an Observable wrapper of <see cref="Reaction"/> for use in <see cref="PageRxnViewModel"/>.
/// </summary>
public partial class PageRxnViewModel : ObservableObject
{
	/// <summary>
	/// Stores the temperature to calculate the reaction at.
	/// </summary>
	/// No need to explicitly define a public property for this field since
	/// no special logic is needed to change the ViewModel when this value is changed.
	[ObservableProperty]
	private Temperature _T;

	/// <summary>
	/// Stores the pressure to calculate the reaction at.
	/// </summary>
	/// No need to explicitly define a public property for this field since
	/// no special logic is needed to change the ViewModel when this value is changed.
	[ObservableProperty]
	private Pressure _P;

	/// <summary>
	/// Stores the frequency factor for the reaction rate law.
	/// </summary>
	/// No need to explicitly define a public property for this field since
	/// no special logic is needed to change the ViewModel when this value is changed.
	[ObservableProperty]
	private double _frequencyFactor;

	/// <summary>
	/// Stores the activation energy for the reaction rate law.
	/// </summary>
	/// No need to explicitly define a public property for this field since
	/// no special logic is needed to change the ViewModel when this value is changed.
	[ObservableProperty]
	private GibbsEnergy _activationEnergy;

	/// <summary>
	/// Stores a list of <see cref="ControlRxnSpeciesViewModel"/>s as an Observable
	/// so the list is bound to the UI.
	/// </summary>
	[ObservableProperty]
	private ObservableCollection<ControlRxnSpeciesViewModel> _items = [];

	/// <summary>
	/// Stores the IActivityModelFactory for the selected activity model.
	/// </summary>
	private IRateLawFactory _rateLawFactory;

	/// <summary>
	/// Stores the <see cref="IRateLawFactory"/> for the selected activity model.
	/// Modifies private <see cref="_rateLawFactory"/>.
	/// </summary>
	/// <remarks>
	/// This property is required to ensure proper setting of the relevant Observable in the ViewModel.
	/// </remarks>
	public IRateLawFactory RateLawFactory
	{
		get => _rateLawFactory;
		set
		{
			// Update the Observable with a reference to the new value.
			SetProperty(ref _rateLawFactory, value);
		}
	}

	/// <summary>
	/// Stores reference to <see cref="DeleteItem(ControlRxnSpeciesViewModel?)"/> to be passed on to
	/// <see cref="ControlRxnSpeciesViewModel.DeleteCommand"/>.
	/// Gives the "Delete species" button in <see cref="ControlRxnSpecies"/> the ability to remove itself from the ViewModel list.
	/// </summary>
	[ObservableProperty]
	private IRelayCommand<ControlRxnSpeciesViewModel> _deleteCommand;

	[ObservableProperty]
	private ObservableCollection<RxnKinOutputItem> _availableOutputOptions;

	[ObservableProperty]
	private ObservableCollection<RxnKinOutputItem> _selectedOutputOptions;

	public PageRxnViewModel()
	{
		// Pass down the DeleteItem command to each ControlRxnSpeciesViewModel.
		DeleteCommand = new RelayCommand<ControlRxnSpeciesViewModel>(DeleteItem);
	}

	public void AddItem(ControlRxnSpeciesViewModel? rxnSpeciesViewModel)
	{
		if (rxnSpeciesViewModel is null)
			return;

		Items.Add(rxnSpeciesViewModel);
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
	/// Converts <see cref="_items"/> into a usable form for instatiating <see cref="IrreversibleSimpleReaction"/>.
	/// </summary>
	/// <returns>List of <see cref="RxnSpecies"/> items whose data is defined by <see cref="_items"/>.</returns>
	public List<RxnSpecies> GetRxnSpeciesList()
	{
		return Items.Select(i => i.ToModel()).ToList();
	}

	public List<RxnKinOutputItem.ItemName> GetSelectedOutputItems()
	{
		return SelectedOutputOptions.Select(x => x.Item).ToList();
	}
}

public class RxnKinOutputItem
{
	public enum ItemType
	{
		Folder,
		Number,
		Plot
	}

	public enum ItemName
	{
		MolarEnthalpyOfReaction,
		MolarEntropyOfReaction,
		MolarGibbsEnergyOfReaction,

		PlotMolarityTransience
	}
	public static readonly Dictionary<ItemName, string> ItemNameToString = new()
	{
		[ItemName.MolarEntropyOfReaction] = "Molar entropy of reaction\n[J/K/mol]",
		[ItemName.MolarEnthalpyOfReaction] = "Molar enthalpy of reaction\n[J/mol]",
		[ItemName.MolarGibbsEnergyOfReaction] = "Molar Gibbs energy of reaction\n[J/mol]",
		[ItemName.PlotMolarityTransience] = "Species molarity vs time\n[mol/L vs s]"
	};
	public static readonly Dictionary<ItemName, Func<string, string>> ItemNameToOutputString = new()
	{
		[ItemName.MolarEntropyOfReaction] = x => $"Molar entropy of reaction: {x}",
		[ItemName.MolarEnthalpyOfReaction] = x => $"Molar enthalpy of reaction: {x}",
		[ItemName.MolarGibbsEnergyOfReaction] = x => $"Molar Gibbs energy of reaction {x}"
	};

	public string Name { get; set; }

	public ItemName Item { get; set; }

	public ItemType Type { get; set; }

	public string Glyph { get; set; }

	public static readonly Dictionary<ItemType, string> ItemTypeToString = new()
	{
		[ItemType.Folder] = "\uED41",
		[ItemType.Number] = "\uE8EF",
		[ItemType.Plot] = "\uE9D2"
	};

	public ObservableCollection<RxnKinOutputItem> Children { get; set; } = [];

	public RxnKinOutputItem(ItemName _item, ItemType _type)
	{
		Item = _item;
		Name = ItemNameToString[_item];
		Type = _type;
		Glyph = ItemTypeToString[_type];
	}

	public RxnKinOutputItem() { }
}