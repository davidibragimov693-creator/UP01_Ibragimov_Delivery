using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace UP01_Ibragimov_Delivery
{
    public partial class MainWindow : Window
    {
        // Таймер для обновления времени
        DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            // Настраиваем таймер (обновление раз в секунду)
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        // Событие таймера: обновляет дату и время в правом нижнем углу
        private void Timer_Tick(object sender, EventArgs e)
        {
            DateTimeNow.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        }

        // Событие загрузки окна (вызывается один раз при старте)
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Вызываем сразу, чтобы время появилось без задержки в 1 секунду
            Timer_Tick(null, null);

            // 👇 ДОБАВЛЯЕМ ЭТУ СТРОКУ: Загружаем страницу авторизации при старте
            MainFrame.Navigate(new AuthPage());
        }

        // Событие закрытия окна (крестик)
        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите закрыть приложение?",
                                                      "Подтверждение",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                e.Cancel = true; // Отменяем закрытие
            }
            else
            {
                e.Cancel = false; // Разрешаем закрытие
            }
        }

        // Обработчик кнопки "Назад"
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }

        // Обработчик переключения тем (Светлая / Тёмная)
        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Проверка, чтобы код не сработал во время инициализации окна
            if (ThemeComboBox.SelectedIndex == -1 || Application.Current == null) return;

            try
            {
                // Выбираем файл в зависимости от индекса (0 - светлая, 1 - тёмная)
                string themeFile = ThemeComboBox.SelectedIndex == 0 ? "Dictionary.xaml" : "DictionaryDark.xaml";

                // Загружаем словарь ресурсов
                var uri = new Uri(themeFile, UriKind.Relative);
                ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;

                // Очищаем старые стили и применяем новые
                Application.Current.Resources.Clear();
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке темы: " + ex.Message);
            }
        }
    }
}