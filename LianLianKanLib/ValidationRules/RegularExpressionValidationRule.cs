using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LianLianKanLib.ValidationRules
{
    public class RegularExpressionValidationRule : ValidationRule
    {
        public RegularExpressionValidationRule()
        {
            this.ValidationStep = ValidationStep.ConvertedProposedValue;
        }

        public string Expression { get; set; }
        public string ErrorMessage { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value != null)
            {
                var regEx = new Regex(Expression);
                bool isMatch = regEx.IsMatch(value.ToString());
                if (isMatch)
                    return new ValidationResult(true, "验证正确");
            }
            return new ValidationResult(false, ErrorMessage);
        }
    }

}
