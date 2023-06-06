using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LianLianKanLib.ValidationRules
{
    public class IntroduceValidationRule : RegularExpressionValidationRule
    {
        public IntroduceValidationRule()
        {
            this.Expression = @"^[\S|\s]{0,120}$";
            this.ErrorMessage = "个人介绍不能超过120个字符。";
        }
    }

}
