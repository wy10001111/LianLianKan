using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace LianLianKanLib
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

    public class AccountValidationRule : RegularExpressionValidationRule
    {
        public AccountValidationRule()
        {
            this.Expression = @"^[\w|\d]{4,15}$";
            this.ErrorMessage = "账号，必须由4~15个合法字符（a-z|A-Z|0-9）组成。";
        }
    }

    public class PasswordValidationRule : RegularExpressionValidationRule
    {
        public PasswordValidationRule()
        {
            this.Expression = @"^[\w|\d]{6,15}$";
            this.ErrorMessage = "密码，必须由6~15个合法字符（a-z|A-Z|0-9）组成。";
        }
    }

    public class NameValidationRule : RegularExpressionValidationRule
    {
        public NameValidationRule()
        {
            this.Expression = @"^[\S]{1,9}$";
            this.ErrorMessage = "名字不能为空白字符。";
        }
    }

    public class IntroduceValidationRule : RegularExpressionValidationRule
    {
        public IntroduceValidationRule()
        {
            this.Expression = @"^[\S|\s]{0,120}$";
            this.ErrorMessage = "个人介绍不能超过120个字符。";
        }
    }

}
