using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp26
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly HttpClient httpClient;
        private CancellationTokenSource _downloadCts;

        public MainWindow()
        {
            InitializeComponent();
            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
        }


        private async void CountClick(object sender, RoutedEventArgs e)
        {
            Result.Text = "Вычисление...";

            try
            {
                if (!int.TryParse(First.Text, out int number1) || !int.TryParse(Second.Text, out int number2))
                {
                    Result.Text = "Ошибка: введите корректные числа";
                    return;
                }


                var progress = new Progress<int>(value =>
                {
                    ProgressBar.Value = value;
                    Result.Text = $"Вычисление... {value}%";
                });

                int sum = await Task.Run(() => CalculaterSum(number1, number2, progress));

                Result.Text = $"Результат суммы: {sum}";
            }
            catch (OperationCanceledException)
            {
                Result.Text = "Операция отменена";
            }
            catch (Exception ex)
            {
                Result.Text = $"Ошибка: {ex.Message}";
            }
        }



        private int CalculaterSum(int num1, int num2, IProgress<int> progress)
        {
            int result = 0;

            for (int i = 0; i <= 100; i++)
            {
                Thread.Sleep(50);

                progress?.Report(i);

                if (i == 100)
                {
                    result = num1 + num2;
                }
            }
            return result;
        }


        private async void UploadClick(object sender, RoutedEventArgs e)
        {
            _downloadCts = new CancellationTokenSource();
            try
            {
                Status.Text = "Загрузка данных...";

                JsonData.Text = string.Empty;

                string jsonData = await DownloadJsonDataAsync(_downloadCts.Token);

                JsonData.Text = jsonData;
                Status.Text = "Данные успешно загружены!";
            }
            catch (Exception ex)
            {
                Status.Text = $"Ошибка: {ex.Message}";
            }
        }



        private async Task<string> DownloadJsonDataAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(1000 + new Random().Next(1000), cancellationToken);

                string apiUrl = "https://jsonplaceholder.typicode.com/posts/1";

                HttpResponseMessage response = await httpClient.GetAsync(apiUrl, cancellationToken);

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }



        private async void BreakClick(object sender, RoutedEventArgs e)
        {
            Text.Text = "Блокирующая операция выполняется...";

            try
            {
                DateTime startTime = DateTime.Now;

                while ((DateTime.Now - startTime).TotalSeconds < 5)
                {
                    double result = 0;
                    for (int i = 0; i < 10000; i++)
                    {
                        result += Math.Sqrt(i) * Math.Sin(i);
                    }
                    Thread.Sleep(100);
                }

                Text.Text = "Операция завершена";
            }
            catch (Exception ex)
            {
                Text.Text = $"Ошибка: {ex.Message}";
            }
        }


        private void ClearClick(object sender, RoutedEventArgs e)
        {
            _downloadCts?.Cancel();

            First.Clear();
            Second.Clear();
            ProgressBar.Value = 0;
            Result.Text = "Результат суммы: ";

            Status.Text = "Ожидание загрузки";
            JsonData.Clear();

            Text.Text = "Статус выполнения операции";
        }
    }
}