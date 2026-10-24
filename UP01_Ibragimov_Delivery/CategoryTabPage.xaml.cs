using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace UP01_Ibragimov_Delivery
{
    public partial class CategoryTabPage : Page
    {
        public CategoryTabPage()
        {
            InitializeComponent();
            DataGridCategory.ItemsSource = Entities.GetContext().Category.ToList();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        // Обновление данных при возвращении на страницу
        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                // Важно: отключаем отслеживание изменений, чтобы гарантированно получить свежие данные из БД
                Entities.GetContext().ChangeTracker.Entries().ToList().ForEach(x => x.State = System.Data.Entity.EntityState.Detached);
                DataGridCategory.ItemsSource = Entities.GetContext().Category.ToList();
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddCategoryPage(null));
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            // Получаем категорию из выбранной строки и передаем в редактор
            NavigationService?.Navigate(new AddCategoryPage((sender as Button).DataContext as Category));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var categoriesForRemoving = DataGridCategory.SelectedItems.Cast<Category>().ToList();

            if (categoriesForRemoving.Count == 0)
            {
                MessageBox.Show("Выберите категории для удаления!");
                return;
            }

            if (MessageBox.Show($"Вы точно хотите удалить записи в количестве {categoriesForRemoving.Count()} элементов?", "Внимание",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    Entities.GetContext().Category.RemoveRange(categoriesForRemoving);
                    Entities.GetContext().SaveChanges();
                    MessageBox.Show("Данные успешно удалены!");
                    DataGridCategory.ItemsSource = Entities.GetContext().Category.ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }
    }
}