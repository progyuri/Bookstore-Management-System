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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace Bookstore_Management_System
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BookStoreContext _context;
        private Users _loginUser; // пользователь, который вошел в систему

        public MainWindow(Users loguser)
        {
            InitializeComponent();
            _loginUser = loguser;
            _context = new BookStoreContext();
            

            // Загрузка данных
            LoadBooks();

            // Настройка видимости элементов в зависимости от роли
            SetVisibilityBasedOnRole();
        }
        private void LoadBooks()
        {
            BooksGrid.ItemsSource = _context.Books.Include("Genres").ToList();
            SearchGenreComboBox.ItemsSource = _context.Genres.ToList();
        }

        // Видимость кнопок в зависимости от роли пользователя

        private void SetVisibilityBasedOnRole()
        {
            
            bool isAdmin = _loginUser.Role == "Admin"; // true, если пользователь - Admin
            bool isManager = _loginUser.Role == "Manager"; // true, если пользователь - Manager
            bool isCustomer = _loginUser.Role == "Customer"; // true, если пользователь - Customer

                // Управление кнопками
                AddBookButton.Visibility = isAdmin || isManager ? Visibility.Visible : Visibility.Collapsed;
                EditBookButton.Visibility = isAdmin || isManager ? Visibility.Visible : Visibility.Collapsed;
                DeleteBookButton.Visibility = isAdmin || isManager ? Visibility.Visible : Visibility.Collapsed;
                ViewBookButton.Visibility = isCustomer ? Visibility.Visible : Visibility.Collapsed;

                // Правая панель
                AdminPanel.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
                ManageOrdersButton.Visibility = isManager ? Visibility.Visible : Visibility.Collapsed;

                // Для менеджеров скрываем управление доступом
                if (isManager)
                {
                    foreach (var btn in AdminPanel.Children.OfType<Button>())
                    {
                        btn.Visibility = btn.Content.ToString() == "Упр. доступом"
                            ? Visibility.Collapsed
                            : Visibility.Visible;
                    }
                }

            // // Для менеджеров скрываем кнопки "Заказать" "Зарезервировать" и "Корзина", оставляем только кнопку "Управление заказами"
                if (isManager)
                {
                    QuantityTextBlock.Visibility = Visibility.Collapsed;
                    QuantityTextBox.Visibility = Visibility.Collapsed;
                    AddToOrderButton.Visibility = Visibility.Collapsed;
                    ReserveButton.Visibility = Visibility.Collapsed;
                    OrderButton.Visibility = Visibility.Collapsed;
                    ViewOrdersButton.Visibility = Visibility.Visible;
                }

            // Для покупателей скрываем кнопку "Упр. заказами"
            if (isCustomer) ViewOrdersButton.Visibility = Visibility.Collapsed;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var filteredBooks = _context.Books
                .Where(b => b.Title.Contains(SearchTitleTextBox.Text) &&
                            b.Author.Contains(SearchAuthorTextBox.Text) &&
                            b.Genres.Name.Contains(SearchGenreComboBox.Text))
                .ToList();

            BooksGrid.ItemsSource = filteredBooks;
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем выбранную книгу из DataGrid
                var selectedBook = BooksGrid.SelectedItem as Books;
                if (selectedBook == null)
                {
                    MessageBox.Show("Выберите книгу для покупки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Получаем количество книг из текстового поля
                int quantity;
                if (string.IsNullOrEmpty(QuantityTextBox.Text))
                {
                    // Если поле пустое, покупаем одну книгу по умолчанию
                    quantity = 1;
                }
                else if (!int.TryParse(QuantityTextBox.Text, out quantity) || quantity <= 0)
                {
                    MessageBox.Show("Введите корректное количество книг.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверяем, есть ли достаточное количество книг на складе
                if (selectedBook.StockQuantity < quantity)
                {
                    MessageBox.Show($"Недостаточно книг на складе. Доступно: {selectedBook.StockQuantity}.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Получаем текущего пользователя (предположим, что он уже авторизован)
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

                // Получаем менеджера (например, первого в списке)
                var manager = _context.Managers.FirstOrDefault();
                if (manager == null)
                {
                    MessageBox.Show("Менеджер не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверяем, что цена книги не равна null
                if (selectedBook.SellingPrice == null)
                {
                    MessageBox.Show("Цена книги не указана.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Приводим SellingPrice к decimal
                decimal sellingPrice = selectedBook.SellingPrice.Value;

                // Создаем запись о продаже
                var sale = new Sales
                {
                    BookId = selectedBook.Id,
                    SaleDate = DateTime.Now,
                    Quantity = quantity, // Покупаем указанное количество книг
                    TotalAmount = sellingPrice * quantity, // Общая сумма равна цене книги, умноженной на количество
                    ManagerId = manager.Id,
                    CustomerId = customer.Id
                };

                // Уменьшаем количество доступных экземпляров
                selectedBook.StockQuantity -= quantity;

                // Добавляем запись о продаже в базу данных
                _context.Sales.Add(sale);

                // Сохраняем изменения в базе данных
                _context.SaveChanges();

                // Обновляем DataGrid
                LoadBooks();

                MessageBox.Show($"Книга успешно куплена в количестве {quantity} шт.!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при покупке книги: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReserveButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBook = BooksGrid.SelectedItem as Books;
            if (selectedBook != null)
            {
                // Логика резервирования книги
                MessageBox.Show($"Книга '{selectedBook.Title}' зарезервирована.");
            }
        }

        private void AddBookButton_Click(object sender, RoutedEventArgs e)
        {
            var addEditWindow = new AddEditBookWindow(_loginUser);
            if (addEditWindow.ShowDialog() == true)
            {
                LoadBooks(); // Обновление списка книг
            }
        }

        private void EditBookButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBook = BooksGrid.SelectedItem as Books;
            if (selectedBook != null)
            {
                // Перезагружаем книгу через текущий контекст
                var bookToEdit = _context.Books
                    .Include("Genres")
                    .FirstOrDefault(b => b.Id == selectedBook.Id);

                if (bookToEdit == null) return;

                var addEditWindow = new AddEditBookWindow(_loginUser, bookToEdit);
                if (addEditWindow.ShowDialog() == true)
                {
                    LoadBooks();
                }
            }
        }

        private void DeleteBookButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedBook = BooksGrid.SelectedItem as Books;
                if (selectedBook != null)
                {
                    _context.Books.Remove(selectedBook);
                    _context.SaveChanges();
                    LoadBooks(); // Обновление списка книг
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при удалении книги, возможно, она используется в других таблицах." + ex.Message);
            }
        }

        private void ManageUsersButton_Click(object sender, RoutedEventArgs e)
        {
            // Логика управления пользователями
            var UserAccessManagementWindow = new UserAccessManagementWindow();
            UserAccessManagementWindow.ShowDialog();

        }

        private void ManageManagersButton_Click(object sender, RoutedEventArgs e)
        {
            // Логика управления менеджерами
        }

        private void ManageCustomersButton_Click(object sender, RoutedEventArgs e)
        {
            // Логика управления покупателями
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Инициализация переменной для фильтрации книг
                var filteredBooks = _context.Books.AsQueryable();

                // Показываем все книги
                if (AllBooksRadioButton.IsChecked == true)
                {
                    BooksGrid.ItemsSource = _context.Books.ToList();
                }

                // Применяем фильтры в зависимости от выбранного RadioButton
                else if (NewBooksRadioButton.IsChecked == true)
                {
                    // Показываем новые книги (изданные за последний год)
                    BooksGrid.ItemsSource = filteredBooks
                        .Where(b => b.YearPublished >= DateTime.Now.AddYears(-1).Year).ToList();
                }
                else if (PopularBooksRadioButton.IsChecked == true)
                {

                    // Показываем популярные книги (сортировка по рейтингу и количеству продаж)
                    BooksGrid.ItemsSource = _context.Books
                        .GroupJoin(
                            _context.BookRatings,
                            book => book.Id,
                            rating => rating.BookId,
                            (book, ratings) => new
                            {
                                Book = book,
                                AverageRating = ratings.Any() ? ratings.Average(r => r.Rating) : 0
                            })
                        .OrderByDescending(b => b.AverageRating)
                        .ThenByDescending(b => b.Book.SalesCount)
                        .Select(b => b.Book)
                        .ToList();
                }
                else if (PopularAuthorsRadioButton.IsChecked == true)
                {
                    // Показываем книги наиболее популярных авторов (сортировка по количеству продаж автора)
                    var popularAuthors = _context.AuthorPopularity
                        .OrderByDescending(ap => ap.SalesCount)
                        .Take(10) // Топ-10 авторов
                        .Select(ap => ap.Author)
                        .ToList();

                    BooksGrid.ItemsSource = _context.Books
                        .Where(b => popularAuthors.Contains(b.Author)) // Фильтруем книги по популярным авторам
                        .OrderByDescending(b => b.SalesCount) // Сортируем по количеству продаж
                        .ToList();
                }
                else if (PopularGenresRadioButton.IsChecked == true)
                {
                    // Показываем книги наиболее популярных жанров (сортировка по количеству продаж жанра)
                    var popularGenres = _context.GenrePopularity
                        .OrderByDescending(gp => gp.SalesCount)
                        .Take(5) // Топ-5 жанров
                        .Select(gp => gp.GenreId)
                        .ToList();

                    BooksGrid.ItemsSource = _context.Books
                        .Where(b => popularGenres.Contains(b.GenreId)) // Фильтруем книги по популярным жанрам
                        .OrderByDescending(b => b.SalesCount) // Сортируем по количеству продаж
                        .ToList();
                }
                else if (BookPromotionsRadioButton.IsChecked == true)
                {
                    // Показываем книги по акциям (фильтрация по акциям)
                    BooksGrid.ItemsSource = filteredBooks
                        .Where(b => b.Promotions.Any(p => p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now));
                }

                // Применяем фильтры и загружаем данные в DataGrid
                
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                MessageBox.Show($"Ошибка при фильтрации книг: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddToOrderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем выбранную книгу из DataGrid
                var selectedBook = BooksGrid.SelectedItem as Books;
                if (selectedBook == null)
                {
                    MessageBox.Show("Выберите книгу для заказа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Получаем количество книг из текстового поля
                int quantity;
                if (string.IsNullOrEmpty(QuantityTextBox.Text))
                {
                    // Если поле пустое, добавляем одну книгу по умолчанию
                    quantity = 1;
                }
                else if (!int.TryParse(QuantityTextBox.Text, out quantity) || quantity <= 0)
                {
                    MessageBox.Show("Введите корректное количество книг.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверяем, есть ли достаточное количество книг на складе
                if (selectedBook.StockQuantity < quantity)
                {
                    MessageBox.Show($"Недостаточно книг на складе. Доступно: {selectedBook.StockQuantity}.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Получаем текущего пользователя (предположим, что он уже авторизован)
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

                // Проверяем, что цена книги не равна null
                if (selectedBook.SellingPrice == null)
                {
                    MessageBox.Show("Цена книги не указана.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Приводим SellingPrice к decimal
                decimal sellingPrice = selectedBook.SellingPrice.Value;

                // Ищем активный заказ (со статусом "Pending")
                var activeOrder = _context.Orders
                    .FirstOrDefault(o => o.CustomerId == customer.Id && o.Status == "Оформление");

                if (activeOrder == null)
                {
                    // Создаем новый заказ, если активного нет
                    activeOrder = new Orders
                    {
                        CustomerId = customer.Id,
                        OrderDate = DateTime.Now,
                        TotalAmount = 0, // Изначально сумма заказа 0
                        Status = "Оформление"
                    };

                    _context.Orders.Add(activeOrder);
                    _context.SaveChanges();
                }

                // Проверяем, есть ли уже такая книга в заказе
                var existingOrderItem = _context.OrderItems
                    .FirstOrDefault(oi => oi.OrderId == activeOrder.Id && oi.BookId == selectedBook.Id);

                if (existingOrderItem != null)
                {
                    // Если книга уже есть в заказе, увеличиваем количество
                    existingOrderItem.Quantity += quantity;
                }
                else
                {
                    // Если книги нет в заказе, добавляем новую позицию
                    var orderItem = new OrderItems
                    {
                        OrderId = activeOrder.Id,
                        BookId = selectedBook.Id,
                        Quantity = quantity,
                        Price = sellingPrice
                    };

                    _context.OrderItems.Add(orderItem);
                }

                // Обновляем общую сумму заказа
                activeOrder.TotalAmount += sellingPrice * quantity;

                // Уменьшаем количество доступных экземпляров книги
                selectedBook.StockQuantity -= quantity;

                // Сохраняем изменения в базе данных
                _context.SaveChanges();

                MessageBox.Show("Книга успешно добавлена в заказ!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении книги в заказ: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Создаем экземпляр окна ViewOrdersWindow
                var viewOrdersWindow = new ViewOrdersWindow(_loginUser);
                // Открываем окно
                viewOrdersWindow.ShowDialog(); // Используем ShowDialog для модального окна
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии окна заказов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ManageOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            var ordersWindow = new ManageOrdersWindow();
            ordersWindow.ShowDialog();
        }

        private void ViewBookButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBook = BooksGrid.SelectedItem as Books;
            if (selectedBook != null)
            {
                var addEditWindow = new AddEditBookWindow(_loginUser, selectedBook);
                if (addEditWindow.ShowDialog() == true)
                {
                    LoadBooks(); // Обновление списка книг
                }
            }
        }

        private void ViewOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Создаем экземпляр окна ViewOrdersWindow
                var ManageOrdersWindow = new ManageOrdersWindow();
                // Открываем окно
                ManageOrdersWindow.ShowDialog(); // Используем ShowDialog для модального окна
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии окна управления заказами: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
