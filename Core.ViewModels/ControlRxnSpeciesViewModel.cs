using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.EquationsOfState;

namespace Core.ViewModels;

public partial class ControlRxnSpeciesViewModel : ObservableObject
{
	private EquationOfState? _eos;

	private Chemical _chemical;
	public Chemical Chemical
	{
		get => _chemical;
		set
		{
			SetProperty(ref _chemical, value);
			UpdateEoS();
		}
	}

	private IEquationOfStateFactory _EoSFactory;
	public IEquationOfStateFactory EoSFactory
	{
		get => _EoSFactory;
		set
		{
			SetProperty(ref _EoSFactory, value);
			UpdateEoS();
		}
	}

	[ObservableProperty]
	private int _stoich;

	[ObservableProperty]
	private List<string> _modeledPhases = [];

	[ObservableProperty]
	private string _phase;

	[ObservableProperty]
	private bool _isReactant;

	[ObservableProperty]
	private IRelayCommand<ControlRxnSpeciesViewModel> _deleteCommand;

	public RxnSpecies ToModel() => new(Chemical, EoSFactory.Create(Chemical), Stoich, Phase, IsReactant);

	private void UpdateEoS()
	{
		if (EoSFactory is null)
		{
			_eos = null;
			ModeledPhases = [];
			return;
		}

		_eos = EoSFactory.Create(Chemical);
		ModeledPhases = _eos.ModeledPhases;
	}
}
