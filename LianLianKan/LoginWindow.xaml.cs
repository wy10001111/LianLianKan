using LianLianKanLib;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LianLianKan
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow(Window owner, MyClientProtocol protocol)
        {
            this.Owner = owner;
            _protocol = protocol;
            Account = "wy10001111";
            InitializeComponent();
            LoginResult = LoginResult.LoginResultWorking;
            Password = "qq666999LL"; 
        }

        private MyClientProtocol _protocol;
        public LoginResult LoginResult { get; set; }

        public string Account { get; set; }
        public string Password
        {
            get
            {
                return passwordBox.Password;
            }
            set
            {
                passwordBox.Password = value;
            }
        }

        public string RegisteredAccount { get; set; }
        public string RegisteredPassword
        {
            get
            {
                return registeredPasswordBox.Password;
            }
            set
            {
                registeredPasswordBox.Password = value;
            }
        }
        public string RegisteredName { get; set; }
        public string RegisteredIntroduce { get; set; } = "Hi !";

        public UIUser User;

        private void OnLoginState(object sender = null, RoutedEventArgs e = null)
        {
            VisualStateManager.GoToElementState(this, "LoginState", true);
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void OnLogin(object sender, RoutedEventArgs e)
        {
            if (_protocol.IsConnected == false)
                _protocol.Connect();
            if (_protocol.IsConnected == false)
                return;
            UIUser user = null;
            string error = null;
            var msg = new MessageLoginRequest()
            {
                CallID = DateTime.Now.GetHashCode(),
                UserID = Account.GetHashCode(),
                User = this._protocol.UserList.First().Value,
                Account = Account,
                Password = Password
            };
            using (var signal = new AutoResetEvent(false))
            {
                EventHandler<Message> waitBack = (object sd, Message respondsMsg) =>
                {
                    if (msg.CallID != respondsMsg.CallID)
                        return;
                    if (respondsMsg.MessageID == MessageID.MI_LOGIN_FAILED)
                    {
                        error = (respondsMsg as MessageLoginFailed).Error;
                        signal.Set();
                    }
                    else if (respondsMsg.MessageID == MessageID.MI_LOGIN_RESPONSE)
                    {
                        var realMsg = respondsMsg as MessageLoginResponds;
                        user = new UIUser(realMsg.UserID, realMsg.Account, "", realMsg.Name);
                        user.Introduce = realMsg.Introduce;
                        user.Client = realMsg.User.Client;
                        _protocol.UserList[user.Client] = user;
                        signal.Set();
                    }
                };
                _protocol.MessageEvent += waitBack;
                _protocol.SendMsg(msg);
                if (signal.WaitOne(2000) == false)
                    error = "登录超时，请重试！";
                _protocol.MessageEvent -= waitBack;
            }
            if (user != null)
            {
                User = user;
                LoginResult = LoginResult.LoginResultLoginSuccesful;
                MessageWindow.Show(this.Owner, "登录成功！");
                this.OnClose();
            }
            else
                MessageWindow.Show(this.Owner, error);
        }

        private void OnTourist(object sender, RoutedEventArgs e)
        {
            User = new UIUser("123456", "游客");
            User.Introduce = "你好呀！";
            User.ChangeHead(@"Image\Logo.ico");
            LoginResult = LoginResult.LoginResultTourist;
            VisualStateManager.GoToElementState(this, "CloseState", true);
            this.OnClose();
        }

        private void OnRegister(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToElementState(this, "RegisterState", true);
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            LoginResult = LoginResult.LoginResultExit;
            this.OnClose();
        }
        
        private ManualResetEventSlim _closeSignal = null;
        private async void OnClose()
        {
            using (_closeSignal = new ManualResetEventSlim(false))
            {
                VisualStateManager.GoToElementState(this, "CloseState", true);
                await Task.Run(() => { _closeSignal.Wait(-1); });
                this.Close();
            }
        }
        private void DoubleAnimation_Completed(object sender, EventArgs e)
        {
            _closeSignal?.Set();
        }

        private void OnClickDefaultGenleman(object sender, RoutedEventArgs e)
        {
            userHead.Source = defaultGenleman.Source;
        }

        private void OnClickDefaultLady(object sender, RoutedEventArgs e)
        {
            userHead.Source = defaultLady.Source;
        }

        private void OnApplyRegister(object sender, RoutedEventArgs e)
        {
            try
            {
                //检查注册信息
                if (userHead.Source == null)
                    throw new Exception("请上传头像！");

                var validateResult = new AccountValidationRule().Validate(RegisteredAccount, null);
                if (validateResult.IsValid == false)
                    throw new Exception(validateResult.ErrorContent.ToString());

                if (repeatedRegisteredPasswordBox.Password != registeredPasswordBox.Password)
                    throw new Exception(PasswordValidattion.GetErrorMessage(repeatedRegisteredPasswordBox));

                validateResult = new PasswordValidationRule().Validate(RegisteredPassword, null);
                if (validateResult.IsValid == false)
                    throw new Exception(validateResult.ErrorContent.ToString());

                validateResult = new NameValidationRule().Validate(RegisteredName, null);
                if (validateResult.IsValid == false)
                    throw new Exception(validateResult.ErrorContent.ToString());
                //连接网络
                if (_protocol.IsConnected == false)
                    _protocol.Connect();
                if (_protocol.IsConnected == false)
                    return;
                //填写消息
                var msg = new MessageRegisterRequest()
                {
                    CallID = DateTime.Now.GetHashCode(),
                    UserID = Account.GetHashCode(),
                    Account = RegisteredAccount,
                    Password = RegisteredPassword,
                    Name = RegisteredName,
                    Introduce = RegisteredIntroduce,
                    User = _protocol.UserList.First().Value
                };
                var encoder = new BmpBitmapEncoder();
                var bitmapSource = this.userHead.Source as BitmapSource;
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    msg.HeadsculptStream = stream.GetBuffer();
                }
                //发送
                string error = null;
                bool result = false;
                using (var signal = new ManualResetEventSlim())
                {
                    EventHandler<Message> waitBack = (object sd, Message respondsMsg) =>
                    {
                        if (msg.CallID != respondsMsg.CallID)
                            return;
                        if (respondsMsg.MessageID == MessageID.MI_REGISTER_FAILED)
                        {
                            error = (respondsMsg as MessageRegisterFailed).Error;
                            signal.Set();
                        }
                        else if (respondsMsg.MessageID == MessageID.MI_REGISTER_SUCCESSFULLY)
                        {
                            var realMsg = (respondsMsg as MessageRegisterSuccessfully);
                            result = true;
                            signal.Set();
                        }
                    };
                    this._protocol.MessageEvent += waitBack;
                    this._protocol.SendBigMsg(msg);
                    if (signal.Wait(2000) == false)
                        error = "服务器没有响应，请重试！";
                    this._protocol.MessageEvent -= waitBack;
                }
                if (result == false)
                    throw new Exception(error);
                MessageWindow.Show(this, "注册成功，赶紧登录去吧！");
                this.OnLoginState();
            }
            catch (Exception ex)
            {
                MessageWindow.Show(this, ex.Message);
            }
            _protocol.Disconnect();
        }
        
        private void OnPickUpHeadImage(object sender, RoutedEventArgs e)
        {
            try
            {
                var ofd = new OpenFileDialog();
                ofd.Filter = "Image File (*.bmp, *.png, *.jpg) | *.bmp;*.png;*.jpg; | All Files | *.*";
                ofd.RestoreDirectory = true;
                ofd.Multiselect = false;
                if (ofd.ShowDialog(this) == true)
                {
                    (userHead.Source as BitmapImage)?.StreamSource.Dispose();
                    userHead.Source = this.GetImageSource(ofd.FileName);
                }
                userHead.ClipToBounds = true;
            }
            catch (Exception ex)
            {
                MessageWindow.Show(this, ex.Message);
            }
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            var box = sender as PasswordBox;
            var validateResult = new PasswordValidationRule().Validate(box.Password, null);
            if (validateResult.IsValid == false)
            {
                PasswordValidattion.SetHasError(box, true);
                PasswordValidattion.SetErrorMessage(box, validateResult.ErrorContent.ToString());
                return;
            }
            if (repeatedRegisteredPasswordBox == box || registeredPasswordBox == box)
            {
                if (repeatedRegisteredPasswordBox.Password != registeredPasswordBox.Password)
                {
                    PasswordValidattion.SetHasError(repeatedRegisteredPasswordBox, true);
                    PasswordValidattion.SetErrorMessage(repeatedRegisteredPasswordBox, "两次输入密码不一致！");
                    PasswordValidattion.SetHasError(registeredPasswordBox, false);
                    PasswordValidattion.SetErrorMessage(registeredPasswordBox, null);
                }
                else
                {
                    PasswordValidattion.SetHasError(repeatedRegisteredPasswordBox, false);
                    PasswordValidattion.SetErrorMessage(repeatedRegisteredPasswordBox, null);
                    PasswordValidattion.SetHasError(registeredPasswordBox, false);
                    PasswordValidattion.SetErrorMessage(registeredPasswordBox, null);
                }
                return;
            }
            PasswordValidattion.SetHasError(box, false);
            PasswordValidattion.SetErrorMessage(box, null);
        }

        public BitmapImage GetImageSource(string imagePath)
        {
            //先裁剪
            var squaredSource = SquaredImage(new BitmapImage(new Uri(imagePath)) as BitmapSource);
            //再缩小
            using (var sourceStream = new MemoryStream())
            {
                var encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(squaredSource));
                encoder.Save(sourceStream);
                using (var newImage = ReasizeImage(new Bitmap(sourceStream), new System.Drawing.Size(100, 100)))
                {
#if false
                    newImage.Save($@"Image\test.png");
#endif
                    var destStream = new MemoryStream();
                    newImage.Save(destStream, System.Drawing.Imaging.ImageFormat.Png);
                    var result = new BitmapImage();
                    result.BeginInit();
                    result.StreamSource = destStream;
                    result.EndInit();
                    return result;
                }
            }
        }
        private BitmapSource SquaredImage(BitmapSource bitmapSource)
        {
            int x, y, width, height;
            if (bitmapSource.PixelWidth > bitmapSource.PixelHeight)
            {
                int diff = bitmapSource.PixelWidth - bitmapSource.PixelHeight;
                x = diff / 2;
                y = 0;
                width = bitmapSource.PixelHeight;
                height = bitmapSource.PixelHeight;
            }
            else
            {
                int diff = bitmapSource.PixelHeight - bitmapSource.PixelWidth;
                x = 0;
                y = diff / 2;
                width = bitmapSource.PixelWidth;
                height = bitmapSource.PixelWidth;
            }
            int bitsPerPixel = bitmapSource.Format.BitsPerPixel / 8;
            byte[] pixelBuff = new byte[width * height * bitsPerPixel];
            bitmapSource.CopyPixels(new Int32Rect(x, y, width, height), pixelBuff, width * bitsPerPixel, 0);
            var cutSource = BitmapSource.Create(width, height,
                bitmapSource.DpiX, bitmapSource.DpiY, bitmapSource.Format, bitmapSource.Palette,
                pixelBuff, width * bitsPerPixel);
            return cutSource;
        }
        private System.Drawing.Image ReasizeImage(System.Drawing.Image image, System.Drawing.Size size)
        {
            double sourceWidth = image.Width;
            double sourceHeight = image.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;
            nPercentW = (float)(size.Width / sourceWidth);
            nPercentH = (float)(size.Height / sourceHeight);
            nPercent = Math.Min(nPercentW, nPercentH);
            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage(b as System.Drawing.Image);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, 0, 0, destWidth, destHeight);
            g.Dispose();
            return b as System.Drawing.Image;
        }

    }

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

    public enum LoginResult
    {
        LoginResultWorking,
        LoginResultLoginFail,
        LoginResultLoginSuccesful,
        LoginResultRegist,
        LoginResultTourist,
        LoginResultExit,
    }

}
