using System.Windows;
using System.Windows.Controls;

namespace UP01_Ibragimov_Delivery
{
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
        }

        // 1. Переход на таблицу пользователей
        private void BtnTab1_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new UsersTabPage());
        }

        // 2. Переход на таблицу категорий
        private void BtnTab2_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CategoryTabPage());
        }

        // 3. Переход на таблицу платежей/заказов
        private void BtnTab3_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new PaymentTabPage());
        }

        // 4. Переход на страницу диаграмм
        private void BtnTab4_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new DiagrammPage());
        }

        // 5. Кнопка возврата к обычному списку пользователей
        private void BtnBackToUsers_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new UserPage());
        }
    }
}