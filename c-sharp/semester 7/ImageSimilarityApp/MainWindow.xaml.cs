using Microsoft.Win32;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using ImageSimilarityApp.Services;
using System.Linq;
using ImageSimilarityApp.Data;
using Microsoft.EntityFrameworkCore;

namespace ImageSimilarityApp
{
    public partial class MainWindow : Window
    {
        private readonly SimilarityService _similarityService;

        private string? _leftImagePath;
        private string? _rightImagePath;

        private bool _isComputing = false;
        private CancellationTokenSource? _cts;

        public MainWindow()
        {
            InitializeComponent();

            var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                         "Models",
                                         "arcfaceresnet100-8.onnx");

            _similarityService = new SimilarityService(modelPath);
            TxtStatus.Text = "Загрузите два изображения.";
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadResultsFromDbAsync();
        }

        private async Task LoadResultsFromDbAsync()
        {
            try
            {
                using var db = new AppDbContext();
                await db.Database.EnsureCreatedAsync();

                var list = await db.ImagePairResults
                                   .OrderByDescending(r => r.CreatedAt)
                                   .ToListAsync();

                LstResults.ItemsSource = list;
            }
            catch (Exception ex)
            {
                TxtStatus.Text = "Ошибка чтения БД.";
                Console.WriteLine(ex);
            }
        }


        private void BtnLoadLeft_Click(object sender, RoutedEventArgs e)
        {
            var path = ShowOpenFileDialog();
            if (path == null) return;

            _leftImagePath = path;
            TxtLeftPath.Text = path;

            LoadImageToControl(path, ImgLeft);

            ClearResults();
            TryStartComputeIfReady();
        }

        private void BtnLoadRight_Click(object sender, RoutedEventArgs e)
        {
            var path = ShowOpenFileDialog();
            if (path == null) return;

            _rightImagePath = path;
            TxtRightPath.Text = path;

            LoadImageToControl(path, ImgRight);

            ClearResults();
            TryStartComputeIfReady();
        }

        private string? ShowOpenFileDialog()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All files|*.*"
            };

            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }

        private void LoadImageToControl(string path, System.Windows.Controls.Image imgControl)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                imgControl.Source = bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения:\n{ex.Message}");
            }
        }

        private void ClearResults()
        {
            TxtSimilarity.Text = "";
            TxtDistance.Text = "";
            TxtStatus.Text = "Ожидание второй картинки...";
        }

        private void TryStartComputeIfReady()
        {
            if (string.IsNullOrEmpty(_leftImagePath) || string.IsNullOrEmpty(_rightImagePath))
            {
                TxtStatus.Text = "Загрузите оба изображения.";
                return;
            }

            if (_isComputing)
            {
                TxtStatus.Text = "Расчёт уже выполняется...";
                return;
            }

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            _ = ComputeSimilarityAsync(_cts.Token);
        }

        private async Task ComputeSimilarityAsync(CancellationToken cancellationToken)
        {
            _isComputing = true;
            TxtStatus.Text = "Вычисление сходства.";

            try
            {
                if (_leftImagePath == null || _rightImagePath == null)
                    return;

                var (similarity, distance) = await _similarityService
                    .GetOrComputeAsync(_leftImagePath, _rightImagePath, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;

                TxtSimilarity.Text = similarity.ToString("F4");
                TxtDistance.Text = distance.ToString("F4");
                TxtStatus.Text = "Готово.";

                await LoadResultsFromDbAsync();
            }
            catch (Exception ex)
            {
                TxtStatus.Text = "Ошибка.";
                MessageBox.Show($"Ошибка при вычислении сходства:\n{ex.Message}");
            }
            finally
            {
                _isComputing = false;
            }
        }


        private async void BtnClearDb_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TxtStatus.Text = "Очищаем базу данных...";
                await _similarityService.ClearDatabaseAsync();
                TxtStatus.Text = "База очищена.";

                await LoadResultsFromDbAsync();
            }
            catch (Exception ex)
            {
                TxtStatus.Text = "Ошибка очистки БД.";
                MessageBox.Show($"Ошибка очистки базы данных:\n{ex.Message}");
            }
        }

    }
}
