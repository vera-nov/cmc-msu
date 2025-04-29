using ClassLibrary;
using System.Windows;
using System.Windows.Controls;
namespace lab4
{
    public partial class MainWindow : Window
    {
        private DataCollection dataCollection = new DataCollection();
        public int TimePause { get; set; }
        public int NewDataItemsCount { get; set; }
        CancellationTokenSource tokenSource { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            gridDataCollection.DataContext = dataCollection;
            TimePause = 2000;
            NewDataItemsCount = 5;
        }
        private async void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            buttonStart.IsEnabled = false;
            buttonCancel.IsEnabled = true;
            textBlock_Info.Text = "";
            string textInfo = "";
            tokenSource = new CancellationTokenSource();
            int current = 0;
            try
            {
                while (current < NewDataItemsCount)
                {
                    textInfo = "Operation in progress";
                    textBlock_Info.Text = textInfo;
                    DataItem result = await Task.Run(() =>
                    DataItem.CreateLongTimeDataItem(current++, TimePause));
                    if (tokenSource.Token.IsCancellationRequested)
                    {
                        buttonCancel.IsEnabled = false;
                        textInfo = "Operation cancelled";
                        tokenSource.Token.ThrowIfCancellationRequested();
                    }
                    dataCollection.Add(result);
                }
                textInfo = "Operation completed";
                buttonCancel.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            textBlock_Info.Text = textInfo;
            buttonStart.IsEnabled = true;
        }
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            tokenSource.Cancel();
            buttonCancel.IsEnabled = false;
        }
        private void Button_ShowDataCollection_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(dataCollection.ToString());
        }
        private void dataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string? header = e.Column.Header.ToString();
            if (header == "Error") e.Cancel = true;
        }
    }
}
