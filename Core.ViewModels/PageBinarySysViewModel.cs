using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.EquationsOfState;
using Core.Multicomponent;
using Core.Multicomponent.ActivityModels;
using System.Collections.ObjectModel;

namespace Core.ViewModels;

/// <summary>
/// ViewModel for ControlRxn (or specifically, PageHomoMix).
/// This is an Observable wrapper of <see cref="Multicomponent.MultiphaseSystem"/>.
/// </summary>
public partial class PageBinarySysViewModel : ObservableObject
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
	/// Stores the mole fraction of species 1 in phase 1.
	/// </summary>
	[ObservableProperty]
	private double _moleFraction_p1s1;

	/// <summary>
	/// Stores the mole fraction of species 2 in phase 1.
	/// </summary>
	[ObservableProperty]
	private double _moleFraction_p1s2;

	/// <summary>
	/// Stores the mole fraction of species 1 in phase 2.
	/// </summary>
	[ObservableProperty]
	private double _moleFraction_p2s1;

	/// <summary>
	/// Stores the mole fraction of species 2 in phase 2.
	/// </summary>
	[ObservableProperty]
	private double _moleFraction_p2s2;

	/// <summary>
	/// Stores the first chemical for the system.
	/// </summary>
	[ObservableProperty]
	private Chemical _species1;

	/// <summary>
	/// Stores the second chemical for the system.
	/// </summary>
	[ObservableProperty]
	private Chemical _species2;

	/// <summary>
	/// Stores the <see cref="IEquationOfStateFactory"/> for phase 1 species 1.
	/// The use of a factory here is important since the UI code does not need
	/// a specific instance of the EoS, only the calculations do.
	/// </summary>
	[ObservableProperty]
	private IEquationOfStateFactory _EoSFactory_p1s1;

	/// <summary>
	/// Stores the <see cref="IEquationOfStateFactory"/> for phase 1 species 2.
	/// The use of a factory here is important since the UI code does not need
	/// a specific instance of the EoS, only the calculations do.
	/// </summary>
	[ObservableProperty]
	private IEquationOfStateFactory _EoSFactory_p1s2;

	/// <summary>
	/// Stores the <see cref="IEquationOfStateFactory"/> for phase 2 species 1.
	/// The use of a factory here is important since the UI code does not need
	/// a specific instance of the EoS, only the calculations do.
	/// </summary>
	[ObservableProperty]
	private IEquationOfStateFactory _EoSFactory_p2s1;

	/// <summary>
	/// Stores the <see cref="IEquationOfStateFactory"/> for phase 1 species 1.
	/// The use of a factory here is important since the UI code does not need
	/// a specific instance of the EoS, only the calculations do.
	/// </summary>
	[ObservableProperty]
	private IEquationOfStateFactory _EoSFactory_p2s2;

	/// <summary>
	/// Stores the list of available equations of state.
	/// </summary>
	[ObservableProperty]
	private List<IEquationOfStateFactory> _availableEoS = [];

	/// <summary>
	/// Stores the first phase for the system.
	/// </summary>
	[ObservableProperty]
	private string _phase1;

	/// <summary>
	/// Stores the second phase for the system.
	/// </summary>
	[ObservableProperty]
	private string _phase2;

	/// <summary>
	/// Stores the list of phases available for use to define the system.
	/// </summary>
	[ObservableProperty]
	private List<string> _availablePhases = [];

	/// <summary>
	/// Stores the IActivityModelFactory for the selected activity model for phase 1.
	/// </summary>
	[ObservableProperty]
	private IActivityModelFactory _activityModelFactoryPhase1;

	/// <summary>
	/// Stores the IActivityModelFactory for the selected activity model for phase 2.
	/// </summary>
	[ObservableProperty]
	private IActivityModelFactory _activityModelFactoryPhase2;

	/// <summary>
	/// Stores the list of available activity models.
	/// </summary>
	[ObservableProperty]
	private List<IActivityModelFactory> _availableActivityModels = [];

	[ObservableProperty]
	private ObservableCollection<ErrorInfoViewModel> _errors = [];

	public List<MixtureSpecies> GetMixtureSpeciesList(int phaseIdx)
	{
		return (phaseIdx == 0) ?
		[
			new MixtureSpecies(Species1, MoleFraction_p1s1, Phase1),
			new MixtureSpecies(Species2, MoleFraction_p1s2, Phase1)
		] : [
			new MixtureSpecies(Species1, MoleFraction_p2s1, Phase2),
			new MixtureSpecies(Species2, MoleFraction_p2s2, Phase2)
		];
	}

	public ActivityModel CreateActivityModel(int phaseIdx)
	{
		return (phaseIdx == 0) ?
			ActivityModelFactoryPhase1.Create(GetMixtureSpeciesList(0)) :
			ActivityModelFactoryPhase2.Create(GetMixtureSpeciesList(1)) ;
	}
}
