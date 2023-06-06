using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LianLianKanLib.ValidationRules
{
    public class NameValidationRule : RegularExpressionValidationRule
    {
        public NameValidationRule()
        {
            this.Expression = @"^[\S]{1,9}$";
            this.ErrorMessage = "名字不能为空白字符。";
        }
    }

}
