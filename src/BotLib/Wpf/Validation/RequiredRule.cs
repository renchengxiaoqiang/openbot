using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BotLib.Wpf.Validation
{
    public class RequiredRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult validationResult;
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                validationResult = new ValidationResult(false, "必填！");
            }
            else
            {
                validationResult = new ValidationResult(true, null);
            }
            return validationResult;
        }
    }
}
