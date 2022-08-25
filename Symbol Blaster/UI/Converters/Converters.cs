using SymbolBlaster.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SymbolBlaster.UI.Converters
{
    [ValueConversion(typeof(MainTabPage), typeof(bool))]
    public class MainTabPageComparisonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MainTabPage valueToConvert = (MainTabPage)value;
            MainTabPage compareValue = (MainTabPage)parameter;

            return valueToConvert == compareValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return MainTabPage.Configure; // Conversions back (not necessary) should default to Configure page
        }
    }

    [ValueConversion(typeof(PresetsTabPage), typeof(bool))]
    public class PresetsTabPageComparisonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PresetsTabPage valueToConvert = (PresetsTabPage)value;
            PresetsTabPage compareValue = (PresetsTabPage)parameter;

            return valueToConvert == compareValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return PresetsTabPage.BuiltIn; // Conversions back (not necessary) should default to Built-In page
        }
    }

    [ValueConversion(typeof(double), typeof(double))]
    public class FontSizeModifier : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double valueToConvert = (double)value;
                double modifier = System.Convert.ToDouble(parameter);
                return valueToConvert += modifier;
            }
            catch (Exception) { }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
