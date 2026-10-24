using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace UP01_Ibragimov_Delivery
{
    public partial class AddPaymentPage : Page
    {
        private Payment _currentPayment = new Payment();

        public AddPaymentPage(Payment selectedPayment)
        {
            InitializeComponent();

            // Загружаем списки для выпадающих меню
            CBUser.ItemsSource = Entities.GetContext().User.ToList();
            CBCategory.ItemsSource = Entities.GetContext().Category.ToList();

            if (selectedPayment != null)
            {
                _currentPayment = selectedPayment;
                TitleText.Text = "Редактирование заказа";
            }
            DataContext = _currentPayment;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentPayment.Name))
                errors.AppendLine("Укажите название!");
            if (_currentPayment.Num <= 0)
                errors.AppendLine("Укажите количество!");
            if (_currentPayment.Price <= 0)
                errors.AppendLine("Укажите цену!");
            if (_currentPayment.Date == null)
                errors.AppendLine("Укажите дату!");
            if (_currentPayment.UserID == 0)
                errors.AppendLine("Выберите пользователя!");
            if (_currentPayment.CategoryID == 0)
                errors.AppendLine("Выберите категорию!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            if (_currentPayment.ID == 0)
                Entities.GetContext().Payment.Add(_currentPayment);

            try
            {
                Entities.GetContext().SaveChanges();
                MessageBox.Show("Данные успешно сохранены!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBPaymentName.Text = "";
            TBCount.Text = "";
            TBAmount.Text = "";
            DPDate.SelectedDate = null;
            CBUser.SelectedIndex = -1;
            CBCategory.SelectedIndex = -1;
        }
    }
}