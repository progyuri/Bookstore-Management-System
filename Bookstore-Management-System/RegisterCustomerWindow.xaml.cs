using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
    /// Логика взаимодействия для RegisterCustomerWindow.xaml
    /// </summary>
    public partial class RegisterCustomerWindow : Window
    {
        public RegisterCustomerWindow()
        {
            InitializeComponent();
        }
        // Обработчик кнопки "Зарегистрировать"
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка заполнения полей
                if (string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||

                    string.IsNullOrWhiteSpace(UsernameTextBox.Text)||
                    string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Создание нового покупателя
              

                // Сохранение в базу данных
                using (var context = new BookStoreContext())
                {
                    var newCustomer = new Customers
                    {
                        CustomerName = FirstNameTextBox.Text,
                        CustomerSurname = LastNameTextBox.Text,
                        City = CityTextBox.Text,
                        Address = AddressTextBox.Text,
                        Tel = PhoneTextBox.Text,
                        Email = EmailTextBox.Text
                    };

                    context.Customers.Add(newCustomer);
  
                    var newUser = new Users
                    {
                        Username = UsernameTextBox.Text,
                        PasswordHash = PasswordBox.Password,
                        Role = "Customer"
                    };

                    context.Users.Add(newUser);
                    context.SaveChanges();
                }

                MessageBox.Show("Покупатель успешно зарегистрирован.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close(); // Закрытие окна после успешной регистрации
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации покупателя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчик кнопки "Отмена"
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Закрытие окна
        }
    }
}
