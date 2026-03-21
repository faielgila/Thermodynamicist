using CommunityToolkit.Mvvm.ComponentModel;

namespace ThermodynamicistCore.ViewModels;

public partial class ValidatedNumberBoxViewModel : ObservableObject
{
	[ObservableProperty]
	private double _value;
	public ValidatedNumberBoxViewModel() { }
}
