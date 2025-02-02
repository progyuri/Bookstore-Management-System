using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Логика взаимодействия для AddEditBookWindow.xaml
    /// </summary>
    public partial class AddEditBookWindow : Window
    {
        private BookStoreContext _context;
        private Books _book;
        private byte[] _coverImageBytes;

        public AddEditBookWindow(Books book = null)
        {
            InitializeComponent();
            _context = new BookStoreContext();
            _book = book;

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
                SellingPriceTextBox.Text = _book.SellingPrice.ToString();
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
                    !decimal.TryParse(CostPriceTextBox.Text, out decimal costPrice) ||
                    !decimal.TryParse(SellingPriceTextBox.Text, out decimal sellingPrice))
                {
                    MessageBox.Show("Пожалуйста, заполните все поля корректно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                _book.Genres.Name = GenreComboBox.SelectedItem.ToString();

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
                        SellingPrice = decimal.Parse(SellingPriceTextBox.Text),
                        IsSequel = IsSequelCheckBox.IsChecked ?? false,
                        SequelToBookId = IsSequelCheckBox.IsChecked == true ? ((Books)SequelToBookComboBox.SelectedItem).Id : (int?)null,
                        CoverImage = _coverImageBytes
                    };
                    _context.Books.Add(newBook);
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
                    _book.SellingPrice = decimal.Parse(SellingPriceTextBox.Text);
                    _book.IsSequel = IsSequelCheckBox.IsChecked ?? false;
                    _book.SequelToBookId = IsSequelCheckBox.IsChecked == true ? ((Books)SequelToBookComboBox.SelectedItem).Id : (int?)null;
                    _book.CoverImage = _coverImageBytes;
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
