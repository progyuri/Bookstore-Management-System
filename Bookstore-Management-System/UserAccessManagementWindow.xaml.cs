using System;
using System.Collections.Generic;
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

namespace Bookstore_Management_System
{
    /// <summary>
    /// Логика взаимодействия для UserAccessManagementWindow.xaml
    /// </summary>
    public partial class UserAccessManagementWindow : Window
    {
        private BookStoreContext _context;
        public UserAccessManagementWindow()
        {
            InitializeComponent();
            _context = new BookStoreContext();
            LoadUsers();
        }

        // Загрузка списка пользователей
        private void LoadUsers()
        {
            try
            {
                // Загружаем пользователей из базы данных
                // Устанавливаем источник данных для DataGrid
                UsersGrid.ItemsSource = _context.Users.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Добавление нового пользователя
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditUserWindow();
            if (editWindow.ShowDialog() == true)
            {
                LoadUsers(); // Перезагружаем список пользователей
            }
        }

        // Редактирование выбранного пользователя
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersGrid.SelectedItem as Users;
            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editWindow = new EditUserWindow(selectedUser);
            if (editWindow.ShowDialog() == true)
            {
                LoadUsers(); // Перезагружаем список пользователей
            }
        }

        // Сохранение изменений
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем измененные данные из DataGrid
                var updatedUsers = UsersGrid.ItemsSource.Cast<Users>().ToList();

                // Обновляем данные в базе если было редактирование в datagrid
                foreach (var user in updatedUsers)
                {
                    var dbUser = _context.Users.FirstOrDefault(u => u.Id == user.Id);
                    if (dbUser != null)
                    {
                        dbUser.IsActive = user.IsActive;
                    }
                }

                // Сохраняем изменения
                _context.SaveChanges();

                MessageBox.Show("Изменения сохранены успешно.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            this.Close();
            
        }

        // Закрытие окна без сохранения
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersGrid.SelectedItem as Users;
            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Подтверждение удаления
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить пользователя {selectedUser.Username}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Находим пользователя в базе данных
                    var dbUser = _context.Users.FirstOrDefault(u => u.Id == selectedUser.Id);
                    if (dbUser != null)
                    {
                        // Удаляем пользователя
                        _context.Users.Remove(dbUser);
                        _context.SaveChanges();

                        // Обновляем список пользователей
                        LoadUsers();

                        MessageBox.Show("Пользователь успешно удален.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }
    }
}

