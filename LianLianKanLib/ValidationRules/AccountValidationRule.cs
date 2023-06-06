using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LianLianKanLib.ValidationRules
{
    public class AccountValidationRule : RegularExpressionValidationRule
    {
        public AccountValidationRule()
        {
            this.Expression = @"^[\w|\d]{4,15}$";
            this.ErrorMessage = "账号，必须由4~15个合法字符（a-z|A-Z|0-9）组成。";
        }
    }

}
