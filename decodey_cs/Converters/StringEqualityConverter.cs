using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Decodey.Converters
{
	/// <summary>
	/// Converts a string to boolean by comparing it with another string
	/// </summary>
	public class StringEqualityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || parameter == null)
				return false;

			return value.ToString().Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || parameter == null)
				return null;

			if ((bool)value)
				return parameter.ToString();

			return null;
		}
	}
}