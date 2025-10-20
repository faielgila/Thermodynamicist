using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ViewModels;

/// <summary>
/// View model for page errors to be displayed in an InfoBar.
/// </summary>
public partial class ErrorInfoViewModel : ObservableObject
{
	[ObservableProperty]
	private string _message;

	[ObservableProperty]
	private string _severity;

	public ErrorInfoViewModel(bool isSevere, string message)
	{
		_message = message;
		_severity = isSevere ? "Error" : "Warning";
	}
}
