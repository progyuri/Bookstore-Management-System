using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Bookstore_Management_System
{
    /// <summary>
    /// Логика взаимодействия для AddEditBookWindow.xaml
    /// </summary>
    public partial class AddEditBookWindow : Window
    {
        private BookStoreContext _context;
        private Books _book;
        private byte[] _coverImageBytes;
        private Users _loguser;

        private int _hoverRating;
        private int _selectedRating;
        public List<int> StarCounts { get; } = new List<int> { 1, 2, 3, 4, 5 };

        public AddEditBookWindow(Users loguser, Books book = null)
        {
            InitializeComponent();
            _context = new BookStoreContext();
            _book = book;
            _loguser = loguser;

            // Загрузка жанров и книг для ComboBox
            GenreComboBox.ItemsSource = _context.Genres.ToList();
            SequelToBookComboBox.ItemsSource = _context.Books.ToList();

            if (_book != null)
            {
                // Редактирование существующей книги
                Title = "Редактировать книгу";
                TitleTextBox.Text = _book.Title;
                AuthorTextBox.Text = _book.Author;
                GenreComboBox.SelectedItem = _context.Genres.Find(_book.GenreId);
                PublisherTextBox.Text = _book.Publisher;
                PageCountTextBox.Text = _book.PageCount.ToString();
                YearPublishedTextBox.Text = _book.YearPublished.ToString();
                CostPriceTextBox.Text = _book.CostPrice.ToString();
                IsSequelCheckBox.IsChecked = _book.IsSequel;

                if (_book.IsSequel == true)
                {
                    SequelPanel.Visibility = Visibility.Visible;
                    SequelToBookComboBox.SelectedItem = _context.Books.Find(_book.SequelToBookId);
                }

                if (_book.CoverImage != null)
                {
                    _coverImageBytes = _book.CoverImage;
                    CoverImage.Source = ConvertByteArrayToBitmapImage(_book.CoverImage);
                }
            }
            else
            {
                // Добавление новой книги
                Title = "Добавить книгу";
            }

            // Обработка изменения состояния CheckBox
            IsSequelCheckBox.Checked += IsSequelCheckBox_Checked;
            IsSequelCheckBox.Unchecked += IsSequelCheckBox_Unchecked;


            // Загружаем дополнительную информацию о рейтинге книги
            RatingItems.DataContext = this;
            if (_book != null)
            {
                var newrating = _context.BookRatings.Where(r => r.BookId == _book.Id && r.UserId == loguser.Id).FirstOrDefault();
                if (newrating != null)
                {
                    _selectedRating = newrating.Rating;
                    UpdateStarsVisual();
                }
                else
                {
                    _selectedRating = 0;
                    UpdateStarsVisual();
                }
            }

        
            

            // Настройка возможности редактирования элементов в зависимости от роли (для покупателя только просмотр)
            bool isCustomer = loguser.Role == "Customer";

            AuthorTextBox.IsReadOnly = isCustomer;
            TitleTextBox.IsReadOnly = isCustomer;
            GenreComboBox.IsReadOnly = isCustomer;
            PublisherTextBox.IsReadOnly = isCustomer;
            PageCountTextBox.IsReadOnly = isCustomer;
            YearPublishedTextBox.IsReadOnly = isCustomer;
            CostPriceTextBox.IsReadOnly = isCustomer;
            IsSequelCheckBox.IsEnabled = !isCustomer;
            SequelPanel.IsEnabled = !isCustomer;
            SequelToBookComboBox.IsEnabled = !isCustomer;
            LoadCoverButton.Visibility = !isCustomer ? Visibility.Visible : Visibility.Collapsed;
            
        }


        private void UpdateStarsVisual()
        {
            if (RatingItems.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            {
                // Если элементы ещё не созданы, отложим обновление
                RatingItems.UpdateLayout();
            }

            for (int i = 0; i < RatingItems.Items.Count; i++)
            {
                var container = RatingItems.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
                if (container == null) continue;

                var button = FindVisualChild<Button>(container);
                if (button == null) continue;

                var path = FindVisualChild<System.Windows.Shapes.Path>(button);
                if (path == null) continue;

                int starValue = (int)button.Tag;
                path.Fill = starValue <= _selectedRating
                    ? (SolidColorBrush)FindResource("FilledStarBrush")
                    : Brushes.LightGray;
            }
        }

        // Вспомогательный метод для поиска дочерних элементов
        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T result)
                    return result;

                var childResult = FindVisualChild<T>(child);
                if (childResult != null)
                    return childResult;
            }
            return null;
        }


        private void StarButton_MouseEnter(object sender, MouseEventArgs e)
        {
            var button = (Button)sender;
            button.ToolTip = $"Оценка: {button.Tag} звезд";
            _hoverRating = (int)((Button)sender).Tag;
            UpdateStarsVisual();
        }

        private void StarButton_MouseLeave(object sender, MouseEventArgs e)
        {
            _hoverRating = 0;
            UpdateStarsVisual();
        }

        private void StarButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedRating = (int)((Button)sender).Tag;
            UpdateStarsVisual();
        }


        private void IsSequelCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SequelPanel.Visibility = Visibility.Visible;
        }

        private void IsSequelCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SequelPanel.Visibility = Visibility.Collapsed;
        }

        private void LoadCoverButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _coverImageBytes = File.ReadAllBytes(openFileDialog.FileName);
                CoverImage.Source = ConvertByteArrayToBitmapImage(_coverImageBytes);
            }
        }

        private BitmapImage ConvertByteArrayToBitmapImage(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return null;

            using (var ms = new MemoryStream(imageBytes))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация данных
                if (string.IsNullOrWhiteSpace(TitleTextBox.Text) ||
                    string.IsNullOrWhiteSpace(AuthorTextBox.Text) ||
                    GenreComboBox.SelectedItem == null ||
                    string.IsNullOrWhiteSpace(PublisherTextBox.Text) ||
                    !int.TryParse(PageCountTextBox.Text, out int pageCount) ||
                    !int.TryParse(YearPublishedTextBox.Text, out int yearPublished) ||
                    !decimal.TryParse(CostPriceTextBox.Text, out decimal costPrice))
                {
                    MessageBox.Show("Пожалуйста, заполните все поля корректно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                if (_book == null)
                {
                    // Добавление новой книги
                    var newBook = new Books
                    {
                        Title = TitleTextBox.Text,
                        Author = AuthorTextBox.Text,
                        GenreId = ((Genres)GenreComboBox.SelectedItem).Id,
                        Publisher = PublisherTextBox.Text,
                        PageCount = int.Parse(PageCountTextBox.Text),
                        YearPublished = int.Parse(YearPublishedTextBox.Text),
                        CostPrice = decimal.Parse(CostPriceTextBox.Text),
                        IsSequel = IsSequelCheckBox.IsChecked ?? false,
                        SequelToBookId = IsSequelCheckBox.IsChecked == true ? ((Books)SequelToBookComboBox.SelectedItem)?.Id : null,
                        CoverImage = _coverImageBytes
                    };
                    _context.Books.Add(newBook);
                    _context.SaveChanges(); // Сохраняем для получения ID

                    // Добавление нового рейтинга для новой книги при выборе рейтинга
                    if (_selectedRating > 0)
                    {
                        _context.BookRatings.Add(new BookRatings
                        {
                            BookId = newBook.Id,
                            UserId = _loguser.Id,
                            Rating = _selectedRating,
                            RatingDate = DateTime.Now
                        });
                    }
                }
                else
                {
                    // Редактирование существующей книги
                    
                    _book.Title = TitleTextBox.Text;
                    _book.Author = AuthorTextBox.Text;
                    _book.GenreId = ((Genres)GenreComboBox.SelectedItem).Id;
                    _book.Publisher = PublisherTextBox.Text;
                    _book.PageCount = int.Parse(PageCountTextBox.Text);
                    _book.YearPublished = int.Parse(YearPublishedTextBox.Text);
                    _book.CostPrice = decimal.Parse(CostPriceTextBox.Text);
                    _book.IsSequel = IsSequelCheckBox.IsChecked ?? false;
                    _book.SequelToBookId = IsSequelCheckBox.IsChecked == true ? ((Books)SequelToBookComboBox.SelectedItem).Id : (int?)null;
                    _book.CoverImage = _coverImageBytes;
                    
                    _context.SaveChanges();



                    // Обновляем рейтинг книги (если рейтинга еще не было добавляем его)
                    var existingRating = _context.BookRatings
                            .FirstOrDefault(r =>
                                r.BookId == _book.Id &&
                                r.UserId == _loguser.Id);
                    if (_selectedRating > 0)
                    {
                        if (existingRating == null)
                        {
                            _context.BookRatings.Add(new BookRatings
                            {
                                BookId = _book.Id,
                                UserId = _loguser.Id,
                                Rating = _selectedRating,
                                RatingDate = DateTime.Now
                            });
                        }
                        else
                        {
                            existingRating.Rating = _selectedRating;
                            existingRating.RatingDate = DateTime.Now;
                        }
                    }

                    //если убрали рейтинг, а он существовал
                    else if (existingRating != null)
                    {
                        _context.BookRatings.Remove(existingRating);
                    }
                }



                _context.SaveChanges();
                DialogResult = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении книги: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

    }
}
