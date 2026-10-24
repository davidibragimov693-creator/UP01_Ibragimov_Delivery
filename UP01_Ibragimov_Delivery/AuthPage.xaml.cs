using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UP01_Ibragimov_Delivery
{
    public partial class AuthPage : Page
    {
        // Счетчик неверных попыток входа для капчи
        private int failedAttempts = 0;

        public AuthPage()
        {
            InitializeComponent();
        }

        // ==========================================
        // МЕТОД ХЕШИРОВАНИЯ ПАРОЛЯ (SHA1)
        // ==========================================
        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        // ==========================================
        // БЛОК 1: ПОДСКАЗКИ (PLACEHOLDER)
        // ==========================================
        private void TextBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtHintLogin.Visibility = Visibility.Visible;
            if (TextBoxLogin.Text.Length > 0) txtHintLogin.Visibility = Visibility.Hidden;
        }

        private void txtHintLogin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { TextBoxLogin.Focus(); }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            txtHintPass.Visibility = Visibility.Visible;
            if (PasswordBox.Password.Length > 0) txtHintPass.Visibility = Visibility.Hidden;
        }

        private void txtHintPass_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { PasswordBox.Focus(); }

        // ==========================================
        // БЛОК 2: ПЕРЕХОДЫ НА ДРУГИЕ СТРАНИЦЫ
        // ==========================================
        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegPage());
        }

        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ChangePassPage());
        }

        // ==========================================
        // БЛОК 3: АВТОРИЗАЦИЯ (С БАЗОЙ ДАННЫХ)
        // ==========================================
        private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxLogin.Text) || string.IsNullOrEmpty(PasswordBox.Password))
            {
                MessageBox.Show("Введите логин или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Хешируем введенный пароль
            string hashedPassword = GetHash(PasswordBox.Password);

            try
            {
                using (var db = new Entities())
                {
                    // Ищем пользователя по логину и хешу пароля
                    var user = db.User.AsNoTracking()
                                       .FirstOrDefault(u => u.Login == TextBoxLogin.Text && u.Password == hashedPassword);

                    if (user == null)
                    {
                        MessageBox.Show("Пользователь с такими данными не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        failedAttempts++; // Увеличиваем счетчик

                        if (failedAttempts >= 3)
                        {
                            if (captcha.Visibility != Visibility.Visible)
                            {
                                CaptchaSwitch();
                                CaptchaChange();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Добро пожаловать, {user.FIO}!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        failedAttempts = 0; // Сбрасываем счетчик

                        // Переход на нужную страницу в зависимости от роли
                        if (user.Role == "Admin")
                        {
                            NavigationService.Navigate(new AdminPage());
                        }
                        else
                        {
                            NavigationService.Navigate(new UserPage());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при подключении к базе: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==========================================
        // БЛОК 4: МЕХАНИЗМ КАПЧИ
        // ==========================================
        public void CaptchaSwitch()
        {
            if (captcha.Visibility == Visibility.Hidden)
            {
                captcha.Visibility = Visibility.Visible;
                captchaInput.Visibility = Visibility.Visible;
                labelCaptcha.Visibility = Visibility.Visible;
                submitCaptcha.Visibility = Visibility.Visible;

                TextBoxLogin.Visibility = Visibility.Hidden;
                PasswordBox.Visibility = Visibility.Hidden;
                txtHintLogin.Visibility = Visibility.Hidden;
                txtHintPass.Visibility = Visibility.Hidden;
                ButtonChangePassword.Visibility = Visibility.Hidden;
                ButtonEnter.Visibility = Visibility.Hidden;
                ButtonReg.Visibility = Visibility.Hidden;
            }
            else
            {
                captcha.Visibility = Visibility.Hidden;
                captchaInput.Visibility = Visibility.Hidden;
                labelCaptcha.Visibility = Visibility.Hidden;
                submitCaptcha.Visibility = Visibility.Hidden;

                TextBoxLogin.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Visible;
                txtHintLogin.Visibility = Visibility.Visible;
                txtHintPass.Visibility = Visibility.Visible;
                ButtonChangePassword.Visibility = Visibility.Visible;
                ButtonEnter.Visibility = Visibility.Visible;
                ButtonReg.Visibility = Visibility.Visible;

                TextBoxLogin.Clear();
                PasswordBox.Clear();
                captchaInput.Clear();
            }
        }

        public void CaptchaChange()
        {
            string allowchar = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";
            allowchar += "a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,y,z";
            allowchar += "1,2,3,4,5,6,7,8,9,0";

            char[] a = { ',' };
            string[] ar = allowchar.Split(a);
            string pwd = "";
            string temp = "";
            Random r = new Random();

            for (int i = 0; i < 6; i++)
            {
                temp = ar[(r.Next(0, ar.Length))];
                pwd += temp;
            }
            captcha.Text = pwd;
        }

        private void submitCaptcha_Click(object sender, RoutedEventArgs e)
        {
            if (captchaInput.Text != captcha.Text)
            {
                MessageBox.Show("Неверно введена капча", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                CaptchaChange();
                captchaInput.Clear();
            }
            else
            {
                MessageBox.Show("Капча введена успешно, можете продолжить авторизацию", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                CaptchaSwitch();
                failedAttempts = 0;
            }
        }

        private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }
    }
}