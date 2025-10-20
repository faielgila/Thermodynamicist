using Core;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace ThermodynamicistUWP.Converters
{
	/// <summary>
	/// Converts from a string to an InfoBarSeverity enum.
	/// </summary>
	public class ErrorSeverityConverter : IValueConverter
	{
		/// <summary>
		/// Converts the backend string into the frontend InfoBarSeverity. 
		/// </summary>
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (targetType == typeof(string))
				return value;

			return Equals(value, "Warning") ? InfoBarSeverity.Warning : InfoBarSeverity.Error;
		}

		/// <summary>
		/// Converts the frontend InfoBarSeverity into the backend string.
		/// </summary>
		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if (targetType != typeof(InfoBarSeverity))
				return null;

			return ((InfoBarSeverity)value == InfoBarSeverity.Warning) ? "Warning" : "Error";
		}
	}
}
