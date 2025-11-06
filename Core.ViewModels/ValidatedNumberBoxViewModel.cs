using CommunityToolkit.Mvvm.ComponentModel;

namespace Core.ViewModels;

public partial class ValidatedNumberBoxViewModel : ObservableObject
{
	[ObservableProperty]
	private double _value;
	public ValidatedNumberBoxViewModel() { }
}
