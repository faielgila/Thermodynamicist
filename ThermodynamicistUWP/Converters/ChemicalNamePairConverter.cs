using System;
using Core;
using Windows.UI.Xaml.Data;

namespace ThermodynamicistUWP.Converters
{

	/// <summary>
	/// Converts between <see cref="ChemicalNamePair"/> and <see cref="Chemical"/> for use in chemical dropdown selectors.
	/// </summary>
	public class ChemicalNamePairConverter : IValueConverter
	{
		/// <summary>
		/// Converts <see cref="Chemical"/> into <see cref="ChemicalNamePair"/>.
		/// </summary>
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (!(value is Chemical chem))
				return null;

			return new ChemicalNamePair(chem);
		}

		/// <summary>
		/// Converts <see cref="ChemicalNamePair"/> into <see cref="Chemical"/>.
		/// </summary>
		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if (!(value is ChemicalNamePair pair))
				return Chemical.CarbonDioxide;

			return pair.Chem;
		}
	}
}
