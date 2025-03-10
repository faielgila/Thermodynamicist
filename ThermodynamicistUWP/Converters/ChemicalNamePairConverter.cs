using System;
using Core;
using Windows.UI.Xaml.Data;

namespace ThermodynamicistUWP.Converters
{
	public class ChemicalNamePairConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (!(value is Chemical chem))
				return null;

			return new ChemicalNamePair(chem);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if (!(value is ChemicalNamePair pair))
				return Chemical.CarbonDioxide;

			return pair.Chem;
		}
	}
}
