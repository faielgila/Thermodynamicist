using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.EquationsOfState;
using Core.Reactions;

namespace Core.ViewModels;

/// <summary>
/// ViewModel for ControlRxnSpecies.
/// This is an Observable wrapper of <see cref="RxnSpecies"/> for use in <see cref="PageRxnViewModel"/>.
/// </summary>
public partial class ControlRxnSpeciesViewModel : ObservableObject
{
	/// <summary>
	/// Stores the <see cref="EquationOfState"/> to be used for modeling this species in the reaction.
	/// </summary>
	/// <remarks>
	/// This field is private since it should only ever be set from the EoSFactory,
	/// which accesses the relevant Observable in the ViewModel.
	/// </remarks>
	private EquationOfState? _EoS;

	/// <summary>
	/// Stores which <see cref="Chemical"/> this is. Exactly what it says on the tin.
	/// </summary>
	/// <remarks>
	/// This field is private since it should only ever be set from <see cref="Chemical"/>
	/// to ensure proper setting of the relevant Observable in the ViewModel.
	/// </remarks>
	private Chemical _chemical;

	/// <summary>
	/// Stores which <see cref="Chemical"/> this is. Modifies private <see cref="_chemical"/>.
	/// </summary>
	/// <remarks>
	/// This property is required to ensure proper setting of the relevant Observable in the ViewModel,
	/// as well as telling the ViewModel to update the EoS with the new value.
	/// </remarks>
	public Chemical Chemical
	{
		get => _chemical;
		set
		{
			// Update the Observable with a reference to the new value.
			SetProperty(ref _chemical, value);
			// Tell the ViewModel to update the EoS with the new value.
			UpdateEoS();
		}
	}

	/// <summary>
	/// Stores the <see cref="IEquationOfStateFactory"/> for the selected EoS.
	/// The use of a factory here is important since the UI code does not need
	/// a specific instance of the EoS, only the calculations do.
	/// </summary>
	/// <remarks>
	/// This field is private since it should only ever be set from <see cref="EoSFactory"/>
	/// to ensure proper setting of the relevant Observable in the ViewModel.
	/// </remarks>
	private IEquationOfStateFactory _EoSFactory;

	/// <summary>
	/// Stores the <see cref="IEquationOfStateFactory"/> for the selected EoS. Modifies private <see cref="_EoSFactory"/>.
	/// </summary>
	/// <remarks>
	/// This property is required to ensure proper setting of the relevant Observable in the ViewModel,
	/// as well as telling the ViewModel to update <see cref="_EoS"/>.
	/// </remarks>
	public IEquationOfStateFactory EoSFactory
	{
		get => _EoSFactory;
		set
		{
			// Update the Observable with a reference to the new value.
			SetProperty(ref _EoSFactory, value);
			// Tell the ViewModel to update the EoS in the ViewModel.
			UpdateEoS();
		}
	}

	/// <summary>
	/// Stores the stoichiometric coefficient for this species in the reaction.
	/// </summary>
	/// No need to explicitly define a public property for this field since
	/// no special logic is needed to change the ViewModel when this value is changed.
	[ObservableProperty]
	private int _stoich;

	/// <summary>
	/// Stores the list of modeled phases for the selected EoS.
	/// </summary>
	/// No need to explicitly define a public property for this field since
	/// no special logic is needed to change the ViewModel when this value is changed.
	[ObservableProperty]
	private List<string> _modeledPhases = [];

	/// <summary>
	/// Stores the phase for this species in the reaction.
	/// </summary>
	/// No need to explicitly define a public property for this field since
	/// no special logic is needed to change the ViewModel when this value is changed.
	[ObservableProperty]
	private string _phase;

	/// <summary>
	/// Stores whether this species is a reactant or a product in the reaction.
	/// </summary>
	/// No need to explicitly define a public property for this field since
	/// no special logic is needed to change the ViewModel when this value is changed.
	[ObservableProperty]
	private bool _isReactant;

	/// <summary>
	/// Stores a relay command for which method to use when the "Delete species" button is clicked.
	/// </summary>
	/// No need to explicitly define a public property for this field since
	/// no special logic is needed to change the ViewModel when this value is changed.
	[ObservableProperty]
	private IRelayCommand<ControlRxnSpeciesViewModel> _deleteCommand;

	/// <summary>
	/// Generates a <see cref="RxnSpecies"/> with the values of this ViewModel.
	/// </summary>
	public RxnSpecies ToModel() => new(Chemical, EoSFactory.Create(Chemical), Stoich, Phase, IsReactant);

	/// <summary>
	/// Updates <see cref="_EoS"/> and <see cref="ModeledPhases"/> whenever
	/// <see cref="EoSFactory"/> or <see cref="Chemical"/> is changed.
	/// </summary>
	private void UpdateEoS()
	{
		// Should EoSFactory be null, do not try to look at a non-existent instance of a factory.
		if (EoSFactory is null)
		{
			_EoS = null;
			ModeledPhases = [];
			return;
		}

		_EoS = EoSFactory.Create(Chemical);
		ModeledPhases = _EoS.ModeledPhases;
	}

	/// <summary>
	/// Validates all input properties.
	/// </summary>
	/// <returns>null if all inputs are valid, combined string of each invalid input.</returns>
	public string? CheckValidInput()
	{
		var missingInputs = new List<string>();
		if (EoSFactory is null) missingInputs.Add("Equation of state");
		if (Phase is null) missingInputs.Add("Phase");
		if (Chemical == null) missingInputs.Add("Chemical");
		if (Stoich == null) missingInputs.Add("Stoichiometry");
		if (DeleteCommand is null) missingInputs.Add("Delete");

		if (_EoS is null) UpdateEoS();

		// Combine missingInputs string.
		if (missingInputs.Count != 0)
		{
			string missingInputsString = missingInputs.First();
			missingInputs.Remove(missingInputs.First());
			foreach (var item in missingInputs)
			{
				missingInputsString += "; " + item;
			}
			return missingInputsString;
		}
		return null;
	}
}
