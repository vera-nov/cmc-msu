using ClassLibrary;
using System.Windows;
using System.Windows.Controls;
using ViewModel;
namespace lab4
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MyViewModel();
            if (DataContext is MyViewModel viewModel)
            {
                viewModel.CancelRequested += () => MessageBox.Show("Operation was cancelled", "Cancel");
                viewModel.ShowDataRequested += data => MessageBox.Show(data, "Data Collection");
            }
        }
    }
}
