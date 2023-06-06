using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LianLianKan.ViewModel
{
    public class PasswordValidattion : DependencyObject
    {
        public bool HasError
        {
            get
            {
                return (bool)GetValue(HasErrorProperty);
            }
            set
            {
                SetValue(HasErrorProperty, value);
            }
        }
        public static readonly DependencyProperty HasErrorProperty =
            DependencyProperty.RegisterAttached(nameof(HasError), typeof(bool), typeof(PasswordValidattion), new PropertyMetadata(false));
        public string ErrorMessage
        {
            get
            {
                return (string)GetValue(ErrorMessageProperty);
            }
            set
            {
                SetValue(ErrorMessageProperty, value);
            }
        }
        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.RegisterAttached(nameof(ErrorMessage), typeof(string), typeof(PasswordValidattion));


        public static void SetHasError(UIElement element, bool value)
        {
            element.SetValue(HasErrorProperty, value);
        }

        public static bool GetHasError(UIElement element)
        {
            return (bool)element.GetValue(HasErrorProperty);
        }

        public static void SetErrorMessage(UIElement element, string value)
        {
            element.SetValue(ErrorMessageProperty, value);
        }

        public static string GetErrorMessage(UIElement element)
        {
            return (string)element.GetValue(ErrorMessageProperty);
        }


    }
}
