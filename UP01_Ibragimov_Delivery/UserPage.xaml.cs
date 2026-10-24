using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace UP01_Ibragimov_Delivery
{
    public partial class UserPage : Page
    {
        public UserPage()
        {
            InitializeComponent();

            // Загружаем всех пользователей при старте страницы
            var currentUsers = Entities.GetContext().User.ToList();
            ListUser.ItemsSource = currentUsers;
        }

        // Метод обновления списка (вызывается при любом изменении фильтров)
        private void UpdateUsers()
        {
            // Проверка, что страница уже загружена (чтобы не было ошибок)
            if (!IsInitialized) return;

            try
            {
                // Получаем всех пользователей из БД
                List<User> currentUsers = Entities.GetContext().User.ToList();

                // 1. Фильтрация по ФИО
                if (!string.IsNullOrWhiteSpace(fioFilterTextBox.Text))
                {
                    currentUsers = currentUsers.Where(x => x.FIO.ToLower().Contains(fioFilterTextBox.Text.ToLower())).ToList();
                }

                // 2. Фильтрация по роли (только админы)
                if (onlyAdminCheckBox.IsChecked == true)
                {
                    currentUsers = currentUsers.Where(x => x.Role == "Admin").ToList();
                }

                // 3. Сортировка по ФИО
                if (sortComboBox.SelectedIndex == 0)
                {
                    currentUsers = currentUsers.OrderBy(x => x.FIO).ToList(); // По возрастанию
                }
                else
                {
                    currentUsers = currentUsers.OrderByDescending(x => x.FIO).ToList(); // По убыванию
                }

                // Привязываем отфильтрованный список к ListView
                ListUser.ItemsSource = currentUsers;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
            }
        }

        // Обработчики событий фильтров
        private void fioFilterTextBox_TextChanged(object sender, TextChangedEventArgs e) { UpdateUsers(); }
        private void sortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { UpdateUsers(); }
        private void onlyAdminCheckBox_Checked(object sender, RoutedEventArgs e) { UpdateUsers(); }
        private void onlyAdminCheckBox_Unchecked(object sender, RoutedEventArgs e) { UpdateUsers(); }

        // Кнопка "Очистить фильтр"
        private void clearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            fioFilterTextBox.Text = "";
            sortComboBox.SelectedIndex = 0;
            onlyAdminCheckBox.IsChecked = false;
            UpdateUsers();
        }
    }
}