using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UP01_Ibragimov_Delivery
{
    public partial class ChangePassPage : Page
    {
        public ChangePassPage()
        {
            InitializeComponent();
        }

        // Метод хеширования (такой же, как на других страницах)
        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        // ==========================================
        // БЛОК ПОДСКАЗОК (PLACEHOLDER)
        // ==========================================
        private void TbLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblLoginHint.Visibility = Visibility.Visible;
            if (TbLogin.Text.Length > 0) lblLoginHint.Visibility = Visibility.Hidden;
        }
        private void lblLoginHint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { TbLogin.Focus(); }

        private void CurrentPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblCurrentPassHint.Visibility = Visibility.Visible;
            if (CurrentPasswordBox.Password.Length > 0) lblCurrentPassHint.Visibility = Visibility.Hidden;
        }
        private void lblCurrentPassHint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { CurrentPasswordBox.Focus(); }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblNewPassHint.Visibility = Visibility.Visible;
            if (NewPasswordBox.Password.Length > 0) lblNewPassHint.Visibility = Visibility.Hidden;
        }
        private void lblNewPassHint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { NewPasswordBox.Focus(); }

        // ==========================================
        // ЛОГИКА СМЕНЫ ПАРОЛЯ
        // ==========================================
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Проверка заполнения ВСЕХ полей
            if (string.IsNullOrEmpty(TbLogin.Text) ||
                string.IsNullOrEmpty(CurrentPasswordBox.Password) ||
                string.IsNullOrEmpty(NewPasswordBox.Password) ||
                string.IsNullOrEmpty(ConfirmPasswordBox.Password))
            {
                MessageBox.Show("Все поля обязательны к заполнению!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Проверка правильности введенного логина и старого пароля
            string hashedPass = GetHash(CurrentPasswordBox.Password);

            using (var db = new Entities())
            {
                var user = db.User.FirstOrDefault(u => u.Login == TbLogin.Text && u.Password == hashedPass);

                if (user == null)
                {
                    MessageBox.Show("Текущий пароль/Логин неверный!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 3. Проверка корректности нового пароля (длина, английская раскладка, цифра)
                if (NewPasswordBox.Password.Length < 6)
                {
                    MessageBox.Show("Новый пароль слишком короткий, должно быть минимум 6 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                bool en = true;
                bool number = false;

                for (int i = 0; i < NewPasswordBox.Password.Length; i++)
                {
                    if (NewPasswordBox.Password[i] >= '0' && NewPasswordBox.Password[i] <= '9')
                        number = true;
                    else if (!((NewPasswordBox.Password[i] >= 'A' && NewPasswordBox.Password[i] <= 'Z') ||
                               (NewPasswordBox.Password[i] >= 'a' && NewPasswordBox.Password[i] <= 'z')))
                        en = false;
                }

                if (!en)
                {
                    MessageBox.Show("Используйте только английскую раскладку!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!number)
                {
                    MessageBox.Show("Добавьте хотя бы одну цифру!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 4. Проверка совпадения новых паролей
                if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
                {
                    MessageBox.Show("Новые пароли не совпадают!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 5. Сохранение нового пароля в БД
                try
                {
                    user.Password = GetHash(NewPasswordBox.Password); // Хешируем новый
                    db.SaveChanges();

                    MessageBox.Show("Пароль успешно изменен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Возвращаемся на страницу авторизации
                    NavigationService?.Navigate(new AuthPage());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении в БД: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}