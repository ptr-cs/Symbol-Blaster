using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SymbolBlaster.UI.Validators
{
    public class ArgbHexadecimalColorStringValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            return new ValidationResult(
                Regex.Match((string)value, "^#(?:[0-9a-fA-F]{8})$").Success,
                "String must match a valid ARGB Hexadecimal Color (ex. #FF001122)");
        }
    }

    public class ColorByteStringValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            return new ValidationResult(Byte.TryParse((string)value, out _),
                String.Format("Value must be within the range {0} to {1}", byte.MinValue, byte.MaxValue));
        }
    }
}
