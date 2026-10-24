using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UP01_Ibragimov_Delivery
{
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
            comboBxRole.SelectedIndex = 0; // Роль "User" по умолчанию
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
        // БЛОК ПОДСКАЗОК (PLACEHOLDER)
        // ==========================================
        private void txtbxLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblLogHitn.Visibility = Visibility.Visible;
            if (txtbxLog.Text.Length > 0) lblLogHitn.Visibility = Visibility.Hidden;
        }

        private void lblLogHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { txtbxLog.Focus(); }

        private void passBxFrst_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassHitn.Visibility = Visibility.Visible;
            if (passBxFrst.Password.Length > 0) lblPassHitn.Visibility = Visibility.Hidden;
        }

        private void lblPassHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { passBxFrst.Focus(); }

        private void txtbxFIO_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblFioHint.Visibility = Visibility.Visible;
            if (txtbxFIO.Text.Length > 0) lblFioHint.Visibility = Visibility.Hidden;
        }

        private void lblFioHint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { txtbxFIO.Focus(); }


        // ==========================================
        // ЛОГИКА РЕГИСТРАЦИИ
        // ==========================================
        private void regButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Проверка на пустые поля
            if (string.IsNullOrEmpty(txtbxLog.Text) || string.IsNullOrEmpty(txtbxFIO.Text) ||
                string.IsNullOrEmpty(passBxFrst.Password) || string.IsNullOrEmpty(passBxScnd.Password))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Проверка длины пароля
            if (passBxFrst.Password.Length < 6)
            {
                MessageBox.Show("Пароль слишком короткий, должно быть минимум 6 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 3. Проверка на английскую раскладку и наличие цифр
            bool en = true;
            bool number = false;

            for (int i = 0; i < passBxFrst.Password.Length; i++)
            {
                if (passBxFrst.Password[i] >= '0' && passBxFrst.Password[i] <= '9')
                    number = true;
                else if (!((passBxFrst.Password[i] >= 'A' && passBxFrst.Password[i] <= 'Z') ||
                           (passBxFrst.Password[i] >= 'a' && passBxFrst.Password[i] <= 'z')))
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

            // 4. Проверка совпадения паролей
            if (passBxFrst.Password != passBxScnd.Password)
            {
                MessageBox.Show("Пароли не совпадают!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 5. Сохранение в БД
            try
            {
                using (var db = new Entities())
                {
                    // Проверка, есть ли уже такой логин
                    var userExists = db.User.FirstOrDefault(u => u.Login == txtbxLog.Text);
                    if (userExists != null)
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Создаем нового пользователя
                    User newUser = new User
                    {
                        FIO = txtbxFIO.Text,
                        Login = txtbxLog.Text,
                        Password = GetHash(passBxFrst.Password), // Хешируем!
                        Role = comboBxRole.Text
                    };

                    db.User.Add(newUser);
                    db.SaveChanges(); // Сохраняем в базу
                }

                MessageBox.Show("Пользователь успешно зарегистрирован!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService?.GoBack(); // Возвращаемся на страницу входа
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}