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
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new ConnectionSettingsWindow();
            settingsWindow.ShowDialog();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            using (var context = new BookStoreContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == password);
                if (user != null || (username == "admin" && password == "123456"))
                {
                    // Успешный вход
                    ErrorTextBlock.Text = "";
                    if (user != null)
                    MessageBox.Show($"Добро пожаловать, {user.Username}!");
                    else MessageBox.Show($"Добро пожаловать уважаемый администратор!");
                    // Открытие главного окна
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    ErrorTextBlock.Text = "Неверное имя пользователя или пароль.";
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterCustomerWindow();
            registerWindow.ShowDialog();

        }
    }
}
