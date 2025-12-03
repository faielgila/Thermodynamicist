using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.EquationsOfState;
using Core.VariableTypes;

namespace Core.ViewModels;

/// <summary>
/// ViewModel for an EoS (or specifically, PagePCSF).
/// This is an observable wrapper of <see cref="EquationOfState"/>.
/// </summary>
public partial class PagePCSFViewModel : ObservableObject
{
	/// <summary>
	/// Stores the temperature to calculate thermo state variables at.
	/// </summary>
	/// No need to explicitly define a public property for this field since
	/// no special logic is needed to change the ViewModel when this value is changed.
	[ObservableProperty]
	private double _T;

	/// <summary>
	/// Stores the pressure to calculate thermo state variables at.
	/// </summary>
	/// No need to explicitly define a public property for this field since
	/// no special logic is needed to change the ViewModel when this value is changed.
	[ObservableProperty]
	private double _P;

	/// <summary>
	/// Stores the <see cref="EquationOfState"/> to be used for calculating thermo state variables.
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

	[ObservableProperty]
	private ObservableCollection<ErrorInfoViewModel> _errors = [];

	/// <summary>
	/// Generates an <see cref="EquationOfState"/> with the values of this ViewModel.
	/// </summary>
	public EquationOfState ToModel() => EoSFactory.Create(Chemical);

	/// <summary>
	/// Updates <see cref="_EoS"/> whenever
	/// <see cref="EoSFactory"/> or <see cref="Chemical"/> is changed.
	/// </summary>
	private void UpdateEoS()
	{
		// Should EoSFactory be null, do not try to look at a non-existent instance of a factory.
		if (EoSFactory is null)
		{
			_EoS = null;
			return;
		}

		_EoS = EoSFactory.Create(Chemical);
	}
}
