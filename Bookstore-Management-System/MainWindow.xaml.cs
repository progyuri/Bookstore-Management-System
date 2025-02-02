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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bookstore_Management_System
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BookStoreContext _context;
        private bool _isAdmin = false; // true, если пользователь - Admin
        private bool _isManager = false; // true, если пользователь - Manager

        public MainWindow()
        {
            InitializeComponent();
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

        private void SetVisibilityBasedOnRole()
        {
            // Пример: если пользователь - Customer, скрываем вкладки и кнопки
            if (!_isAdmin && !_isManager)
            {
                //TabControl.Visibility = Visibility.Collapsed;
                //ButtonsPanel.Visibility = Visibility.Collapsed;
            }
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
            var selectedBook = BooksGrid.SelectedItem as Books;
            if (selectedBook != null)
            {
                // Логика покупки книги
                MessageBox.Show($"Книга '{selectedBook.Title}' куплена.");
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
            var addEditWindow = new AddEditBookWindow();
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
                var addEditWindow = new AddEditBookWindow(selectedBook);
                if (addEditWindow.ShowDialog() == true)
                {
                    LoadBooks(); // Обновление списка книг
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
             

                // Применяем фильтры в зависимости от выбранного RadioButton
                if (NewBooksRadioButton.IsChecked == true)
                {
                    // Показываем новые книги (изданные за последний год)
                    //filteredBooks = filteredBooks
                    //    .Where(b => b.YearPublished >= DateTime.Now.AddYears(-1).Year);
                }
                else if (PopularBooksRadioButton.IsChecked == true)
                {
                    /*
                    // Показываем популярные книги (сортировка по рейтингу/кол-ву продаж)
                    var AVG = filteredBooks.Average(b => b.Rating);
                    filteredBooks = filteredBooks.Where(b => b.SalesCount > AVG)
                   //     .OrderByDescending(b => b.Rating);
                    */
                }
                else if (PopularAuthorsRadioButton.IsChecked == true)
                {
                    // Показываем книги по автору (фильтрация по введенному тексту)
                    
                }
                else if (PopularGenresRadioButton.IsChecked == true)
                {
                    // Показываем книги по жанру (фильтрация по выбранному жанру)
                    
                }
                else if (BookPromotionsRadioButton.IsChecked == true)
                {
                    // Показываем книги по акциям (фильтрация по акциям)
                    filteredBooks = filteredBooks
                        .Where(b => b.Promotions.Any(p => p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now));
                }

                // Применяем фильтры и загружаем данные в DataGrid
                BooksGrid.ItemsSource = filteredBooks.ToList();
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                MessageBox.Show($"Ошибка при фильтрации книг: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
