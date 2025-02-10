using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    /// Логика взаимодействия для ManageOrdersWindow.xaml
    /// </summary>
    public partial class ManageOrdersWindow : Window
    {
        private BookStoreContext _context = new BookStoreContext();
       
        private CollectionViewSource _ordersViewSource;
        

       public ManageOrdersWindow()
        {
            InitializeComponent();
            LoadData();
        }

        
        private void LoadData()
        {
            _context = new BookStoreContext();

            // Загрузка данных с включением связанных сущностей
            _context.Orders
                .Include(o => o.Customers)
                .Include(o => o.OrderItems.Select(oi => oi.Books))
                .Load();

            // Загрузка покупателей для фильтра
            _context.Customers.Load();
            CustomerComboBox.ItemsSource = _context.Customers.Local.ToList();

            _ordersViewSource = ((CollectionViewSource)(FindResource("ordersViewSource")));
            _ordersViewSource.Source = _context.Orders.Local;
        }

        private void FilterOrders()
        {
            var query = _context.Orders.AsQueryable();

            // Фильтр по покупателю
            if (CustomerComboBox.SelectedItem != null)
            {
                var selectedCustomer = (Customers)CustomerComboBox.SelectedItem;
                query = query.Where(o => o.CustomerId == selectedCustomer.Id);
            }

            // Фильтр по дате
            if (OrderDatePicker.SelectedDate != null)
            {
                query = query.Where(o => DbFunctions.TruncateTime(o.OrderDate) == OrderDatePicker.SelectedDate.Value.Date);
            }

            // Фильтр по статусу
            if (!ShowCompletedCheckBox.IsChecked.Value)
            {
                query = query.Where(o => o.Status != "Завершен");
            }

            OrdersDataGrid.ItemsSource = query
                .Include(o => o.Customers)
                .Include(o => o.OrderItems)
                .ToList()
                .Select(o => new
                {
                    o.Id,
                    o.OrderDate,
                    Customers = o.Customers,
                    o.TotalAmount,
                    o.Status,
                    OrderItems = o.OrderItems,
                    IsActive = o.Status != "Завершен"
                }).ToList();
        }
        private void FilterChanged(object sender, RoutedEventArgs e)
        {
            FilterOrders();
        }

        private void CompleteOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var order = button?.DataContext as Orders;

                if (order != null)
                {
                    order.Status = "Завершен";
                    _context.SaveChanges();
                    FilterOrders();
                    MessageBox.Show("Заказ успешно завершен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при завершении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void CanselOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var order = button?.DataContext as Orders;

                if (order != null)
                {
                    // Удаляем целиком заказ
                    _context.Orders.Remove(order);
                    _context.SaveChanges();
                    FilterOrders();
                    MessageBox.Show("Заказ успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            /*
            var selectedOrder = OrdersGrid.SelectedItem as Orders;
            if (selectedOrder != null)
            {
                selectedOrder.Status = "Подтвержден";
                _context.SaveChanges();
                LoadOrders();
            }
            */
        }
    }
}
