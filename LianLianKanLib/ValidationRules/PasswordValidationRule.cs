using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LianLianKanLib.ValidationRules
{
    public class PasswordValidationRule : RegularExpressionValidationRule
    {
        public PasswordValidationRule()
        {
            this.Expression = @"^[\w|\d]{6,15}$";
            this.ErrorMessage = "密码，必须由6~15个合法字符（a-z|A-Z|0-9）组成。";
        }
    }

}
