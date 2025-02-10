using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    /// Логика взаимодействия для ViewOrdersWindow.xaml
    /// </summary>
    public partial class ViewOrdersWindow : Window
    {
        private BookStoreContext _context;
        private bool _isFirstSelection = true;
        private Users _loginUser; // пользователь, который вошел в систему

        public ViewOrdersWindow(Users loguser)
        {
            InitializeComponent();
            _loginUser = loguser;
            _context = new BookStoreContext();
            

            // Подписываемся на событие загрузки окна
            this.Loaded += OnWindowLoaded;

        }
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            LoadUpdatedOrders();
        }

        private void UpdateStatistics()
        {
            try
            {
                var items = ActiveOrdersListView.ItemsSource as IEnumerable<OrderItems>;
                if (items != null)
                {
                    TotalItemsText.Text = items.Count().ToString();
                    TotalAmountText.Text = items.Sum(oi => oi.Price * oi.Quantity).ToString("C2");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления статистики: {ex.Message}");
            }
        }

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isFirstSelection)
            {
                _isFirstSelection = false;
                return;
            }

            LoadUpdatedOrders();
            UpdateStatistics();
        }

        // Обновление статистики при обновлении количества
        private void Quantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(sender is TextBox textBox) || !(textBox.DataContext is OrderItems orderItem))
                return;

            if (!int.TryParse(textBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Введите корректное количество");
                textBox.Text = orderItem.Quantity.ToString();
                return;
            }

            try
            {
                orderItem.Quantity = quantity;
                _context.Entry(orderItem).State = EntityState.Modified;
                _context.SaveChanges();
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления: {ex.Message}");
            }
        }

       

        // Загрузка заказов
        private void LoadUpdatedOrders()
        {
            try
            {
                // Получаем текущего пользователя (он уже авторизован)
                var currentUser = _context.Users.FirstOrDefault(u => u.Username == _loginUser.Username); 
                if (currentUser == null)
                {
                    MessageBox.Show("Пользователь не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Получаем покупателя, связанного с текущим пользователем
                var customer = _context.Customers.FirstOrDefault(c => c.UserID == currentUser.Id);
                if (customer == null)
                {
                    MessageBox.Show("Покупатель не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Получаем выбранный статус
                //var selectedStatus = (StatusFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

                var statuses = new[] { "Оформление", "Ожидает оплаты" };
                //Статус заказа(Оформление, Ожидает оплаты, Оплачен, Завершен)
                // Загружаем активные заказы (со статусом "Оформление")
                var activeOrders = _context.Orders
                    .Include("OrderItems") // Загружаем связанные позиции заказа
                    .Include("OrderItems.Books")   // Загружаем связанные книги 
                    .Include("OrderItems.Orders") // Загружаем данные по заказу
                    .Where(o => o.CustomerId == customer.Id && statuses.Contains(o.Status))
                    .ToList();


                ActiveOrdersListView.ItemsSource = activeOrders.SelectMany(o => o.OrderItems).ToList();
                    
                    //.;

                UpdateStatistics();  // Обновление статистики

                // Загружаем архивные заказы (со статусом "Completed" или "Paid")
                var archivedOrders = _context.Orders
                    .Include("OrderItems") // Загружаем связанные позиции заказа
                    .Include("OrderItems.Books") // Загружаем связанные книги
                    .Where(o => o.CustomerId == customer.Id && (o.Status == "Завершен"))
                    .ToList();

                ArchivedOrdersListView.ItemsSource = archivedOrders.SelectMany(o => o.OrderItems).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заказов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        

        // Удаление позиции
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var orderItem = button?.DataContext as OrderItems;

            if (orderItem == null)
            {
                MessageBox.Show("Выберите позицию для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Удаляем позицию из заказа
            _context.OrderItems.Remove(orderItem);
            _context.SaveChanges();

            // Обновляем данные
            LoadUpdatedOrders();
        }

        // Оформление заказа
        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем текущего пользователя
                var currentUser = _context.Users.FirstOrDefault(u => u.Username == _loginUser.Username); 
                if (currentUser == null)
                {
                    MessageBox.Show("Пользователь не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Получаем покупателя
                var customer = _context.Customers.FirstOrDefault(c => c.UserID == currentUser.Id);
                if (customer == null)
                {
                    MessageBox.Show("Покупатель не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Ищем активный заказ
                var activeOrder = _context.Orders.FirstOrDefault(o => o.CustomerId == customer.Id && o.Status == "Оформление");

                if (activeOrder == null || !activeOrder.OrderItems.Any())
                {
                    MessageBox.Show("Нет активного заказа для оформления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

               
                // Меняем статус заказа на "Completed" (Завершен)
                activeOrder.Status = "Ожидает оплаты";
                _context.SaveChanges();

                MessageBox.Show("Заказ успешно оформлен и ожидает оплаты!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUpdatedOrders(); // Обновляем список заказов
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Оплата заказа
        private void PayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем текущего пользователя
                var currentUser = _context.Users.FirstOrDefault(u => u.Username == _loginUser.Username);
                if (currentUser == null)
                {
                    MessageBox.Show("Пользователь не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Получаем покупателя
                var customer = _context.Customers.FirstOrDefault(c => c.UserID == currentUser.Id);
                if (customer == null)
                {
                    MessageBox.Show("Покупатель не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Ищем активный заказ
                var activeOrder = _context.Orders.FirstOrDefault(o => o.CustomerId == customer.Id && o.Status == "Ожидает оплаты");

                if (activeOrder == null || !activeOrder.OrderItems.Any())
                {
                    MessageBox.Show("Нет оформленного заказа для оплаты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                // Меняем статус заказа на "Completed" (Завершен)
                activeOrder.Status = "Оплачен";
                _context.SaveChanges();

                MessageBox.Show("Заказ успешно оформлен, оплачен и ожидает подтверждения менеджера для отправки!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUpdatedOrders(); // Обновляем список заказов
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            } 
        }
    }
}
