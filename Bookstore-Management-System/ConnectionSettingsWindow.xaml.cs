using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
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
    /// Логика взаимодействия для ConnectionSettingsWindow.xaml
    /// </summary>
    public partial class ConnectionSettingsWindow : Window
    {
        // Список последних введенных серверов и баз данных
        private List<string> _recentServers = new List<string>();
        private List<string> _recentDatabases = new List<string>();

        public ConnectionSettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            // Загрузка  всех вводимых значений
            if (Properties.Settings.Default.RecentServers != null)
            {
                ServerComboBox.ItemsSource = Properties.Settings.Default.RecentServers.Cast<string>().ToList();
            }
            if (Properties.Settings.Default.RecentDatabases != null)
            {
                DatabaseComboBox.ItemsSource = Properties.Settings.Default.RecentDatabases.Cast<string>().ToList();
            }

            // Установка текущих значений
            // Загрузка последних серверов и баз данных из настроек
            
            ServerComboBox.Text = Properties.Settings.Default.Server;
            DatabaseComboBox.Text = Properties.Settings.Default.Database;
            WindowsAuthCheckBox.IsChecked = Properties.Settings.Default.UseWindowsAuth;

            // Скрыть или показать поля для логина и пароля
            SqlAuthPanel.Visibility = Properties.Settings.Default.UseWindowsAuth ? Visibility.Collapsed : Visibility.Visible;
        }

      
        

        private void WindowsAuthCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Скрыть поля для логина и пароля при выборе Windows-аутентификации
            SqlAuthPanel.Visibility = Visibility.Collapsed;
        }
        

        private void WindowsAuthCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Показать поля для логина и пароля при выборе SQL-аутентификации
            SqlAuthPanel.Visibility = Visibility.Visible;
        }

        // Сохранение настроек подключения
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                Properties.Settings.Default.Server = ServerComboBox.Text;
                Properties.Settings.Default.Database = DatabaseComboBox.Text;
                Properties.Settings.Default.UseWindowsAuth = WindowsAuthCheckBox.IsChecked ?? false;

                if (!Properties.Settings.Default.UseWindowsAuth)
                {
                    Properties.Settings.Default.Username = UsernameTextBox.Text;
                    Properties.Settings.Default.Password = PasswordBox.Password;
                }

                // Добавление сервера и базы данных в список последних
                if (!_recentServers.Contains(ServerComboBox.Text))
                {
                    _recentServers.Add(ServerComboBox.Text);
                    Properties.Settings.Default.RecentServers = new System.Collections.Specialized.StringCollection();
                    Properties.Settings.Default.RecentServers.AddRange(_recentServers.ToArray());
                }

                if (!_recentDatabases.Contains(DatabaseComboBox.Text))
                {
                    _recentDatabases.Add(DatabaseComboBox.Text);
                    Properties.Settings.Default.RecentDatabases = new System.Collections.Specialized.StringCollection();
                    Properties.Settings.Default.RecentDatabases.AddRange(_recentDatabases.ToArray());
                }

                Properties.Settings.Default.Save();

                // Обновление строки подключения в Entity Framework
                UpdateConnectionString();

                MessageBox.Show("Настройки подключения сохранены и применены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении настроек: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void UpdateConnectionString()
        {
            // Формирование строки подключения
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = Properties.Settings.Default.Server,
                InitialCatalog = Properties.Settings.Default.Database
            };

            if (Properties.Settings.Default.UseWindowsAuth)
            {
                builder.IntegratedSecurity = true;
            }
            else
            {
                builder.UserID = Properties.Settings.Default.Username;
                builder.Password = Properties.Settings.Default.Password;
            }

            string connectionString = builder.ToString();

            // Обновление строки подключения в текущем контексте
            if (Application.Current.Properties["BookStoreContext"] is BookStoreContext currentContext)
            {
                currentContext.Database.Connection.ConnectionString = connectionString;
            }

            // Сохранение строки подключения в настройках
            Properties.Settings.Default.ConnectionString = connectionString;
            Properties.Settings.Default.Save();
        }

        private async void CheckConnectionButton_Click(object sender, RoutedEventArgs e)
        {
                try
                {
                    var builder = new SqlConnectionStringBuilder
                    {
                        DataSource = ServerComboBox.Text,
                        InitialCatalog = DatabaseComboBox.Text
                    };

                    if (WindowsAuthCheckBox.IsChecked ?? false)
                    {
                        builder.IntegratedSecurity = true;
                    }
                    else
                    {
                        builder.UserID = UsernameTextBox.Text;
                        builder.Password = PasswordBox.Password;
                    }

                    string connectionString = builder.ToString();

                    using (var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync(); // Асинхронное открытие соединения

                        MessageBox.Show("Подключение к базе данных успешно установлено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
}
