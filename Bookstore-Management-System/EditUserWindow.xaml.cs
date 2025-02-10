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
    /// Логика взаимодействия для EditUserWindow.xaml
    /// </summary>
    public partial class EditUserWindow : Window
    {
        private Users _user;
        private BookStoreContext _context;

        public EditUserWindow()
        {
            InitializeComponent();
            _context = new BookStoreContext();
        }

        // Конструктор для редактирования существующего пользователя
        public EditUserWindow(Users user) : this()
        {
            _user = user;
            UsernameTextBox.Text = user.Username;
            RoleComboBox.SelectedItem = user.Role;
        }

        // Сохранение изменений
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(UsernameTextBox.Text) || string.IsNullOrEmpty(PasswordBox.Password))
                {
                    MessageBox.Show("Заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_user == null)
                {
                    // Добавление нового пользователя
                    var newUser = new Users
                    {
                        Username = UsernameTextBox.Text,
                        PasswordHash = HashPassword(PasswordBox.Password), // Хэширование пароля
                        Role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                        IsActive = true
                    };

                    _context.Users.Add(newUser);
                }
                else
                {
                    // Редактирование существующего пользователя
                    var dbUser = _context.Users.FirstOrDefault(u => u.Id == _user.Id);
                    if (dbUser != null)
                    {
                        dbUser.Username = UsernameTextBox.Text;
                        if (!string.IsNullOrEmpty(PasswordBox.Password))
                        {
                            dbUser.PasswordHash = HashPassword(PasswordBox.Password); // Хэширование пароля
                        }
                        dbUser.Role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                    }
                }

                _context.SaveChanges();
                DialogResult = true; // Закрываем окно с результатом "ОК"
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Отмена изменений
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Закрываем окно с результатом "Отмена"
        }

        // Хэширование пароля (заглушка)
        private string HashPassword(string password)
        {
            // Здесь должна быть реализация хэширования пароля (например, с использованием BCrypt)
            return password; // Заглушка
        }
    }
}
