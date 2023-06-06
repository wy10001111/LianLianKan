using LianLianKan.ViewModel;
using LianLianKanLib.ValidationRules;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
    public enum LoginResult
    {
        LoginResultWorking,
        LoginResultLoginFail,
        LoginResultLoginSuccesful,
        LoginResultRegist,
        LoginResultTourist,
        LoginResultExit,
    }

    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            LoginResult = LoginResult.LoginResultWorking;
            Account = "123456";
            InitializeComponent();
            Password = "123456";
        }

        #region 属性和变量

        public string Account { get; set; }

        public string Password
        {
            get => passwordBox.Password;
            set => passwordBox.Password = value;
        }

        public LoginResult LoginResult { get; set; }

        public MyUser User { get; set; }

        public string RegisteredAccount { get; set; }

        public string RegisteredPassword
        {
            get => registeredPasswordBox.Password;
            set => registeredPasswordBox.Password = value;
        }

        public string RegisteredName { get; set; }

        public string RegisteredIntroduce { get; set; } = "Hi !";

        #endregion

        #region 方法

        /// <summary>
        /// 转换登录状态
        /// </summary>
        private void OnLoginState(object sender = null, RoutedEventArgs e = null)
        {
            VisualStateManager.GoToElementState(this, "LoginState", true);
        }

        /// <summary>
        /// 转换注册状态
        /// </summary>
        private void OnRegister(object sender = null, RoutedEventArgs e =  null)
        {
            VisualStateManager.GoToElementState(this, "RegisterState", true);
        }

        /// <summary>
        /// 退出动画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoubleAnimation_Completed(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 界面拖动
        /// </summary>
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// 退出
        /// </summary>
        private void OnExit(object sender, RoutedEventArgs e)
        {
            LoginResult = LoginResult.LoginResultExit;
            this.OnClose();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        private void OnClose()
        {
            VisualStateManager.GoToElementState(this, "CloseState", true);
        }

        /// <summary>
        /// 登录
        /// </summary>
        private void OnLogin(object sender, RoutedEventArgs e)
        {
            if (Validation.GetHasError(accountBox))
            {
                MessageWindow.Show(this, "请输入正确的账号");
                return;
            }

            if (Validation.GetHasError(passwordBox))
            {
                MessageWindow.Show(this, "请输入正确的密码");
                return;
            }

            MyUser user = new MyUser(0, Account, Password, "");
            Action<string> feedback = (error) => {
                MessageWindow.Show(this, error);
            };
            user.ErrorEventHandler += feedback;
            if (user.Login())
            {
                this.User = user;
                LoginResult = LoginResult.LoginResultLoginSuccesful;
                MessageWindow.Show(this, "登录成功！");
                this.OnClose();
            }
            user.ErrorEventHandler -= feedback;
        }

        /// <summary>
        /// 游客登陆
        /// </summary>
        private void OnTourist(object sender, RoutedEventArgs e)
        {
            User = new MyUser(0, "123456", "游客", "");
            User.Introduce = "你好呀！";
            var filePath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Image\Logo.ico");
            User.ChangeHead(filePath);

            LoginResult = LoginResult.LoginResultTourist;
            this.OnClose();
        }

        /// <summary>
        /// 转换成BitmapImage
        /// </summary>
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

        /// <summary>
        /// 将图片裁剪成正方形
        /// </summary>
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

        /// <summary>
        /// 重新调整图片大小
        /// </summary>
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

        /// <summary>
        ///  点击男头像
        /// </summary>
        private void OnClickDefaultGenleman(object sender, RoutedEventArgs e)
        {
            userHead.Source = defaultGenleman.Source;
        }

        /// <summary>
        /// 点击女头像
        /// </summary>
        private void OnClickDefaultLady(object sender, RoutedEventArgs e)
        {
            userHead.Source = defaultLady.Source;
        }

        /// <summary>
        /// 检查注册信息
        /// </summary>
        private bool CheckRegisteringInfomation ()
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

            return true;
        }

        /// <summary>
        /// 注册账号
        /// </summary>
        private void OnApplyRegister(object sender, RoutedEventArgs e)
        {
            try
            {
                //检查注册信息
                if (this.CheckRegisteringInfomation())
                {
                    var user = new MyUser(0, RegisteredAccount, RegisteredPassword, RegisteredName);
                    user.Introduce = RegisteredIntroduce;
                    //头像
                    var encoder = new BmpBitmapEncoder();
                    var bitmapSource = this.userHead.Source as BitmapSource;
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    using (var stream = new MemoryStream())
                    {
                        encoder.Save(stream);
                        user.ChangeHeadStream(stream);
                    }
                    Action<string> feedback = (error) => {
                        MessageWindow.Show(this, error);
                    };
                    user.ErrorEventHandler += feedback;
                    if (user.Register())
                    {
                        MessageWindow.Show(this, "注册成功，赶紧登录去吧！");
                        this.OnLoginState();
                    }
                    user.ErrorEventHandler -= feedback;
                }
            }
            catch (Exception ex)
            {
                MessageWindow.Show(this, ex.Message);
            }
        }

        /// <summary>
        /// 自定义头像
        /// </summary>
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

        /// <summary>
        /// 当密码改变时
        /// </summary>
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


        #endregion

    }
}
