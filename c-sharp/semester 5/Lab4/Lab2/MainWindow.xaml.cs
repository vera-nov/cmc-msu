using System.Security.Cryptography;
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

using DataLibrary;

namespace Lab2
{
    public partial class MainWindow : Window
    {
        DataCollection dataCollection = new DataCollection();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = dataCollection;
        }

        private void Button_UpdateCollection_Click(object sender, RoutedEventArgs e)
        {
            dataCollection.UpdateDataCollection();
        }

        private void Button_UpdatePropertiesInDataItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataItem? dItem = this.TryFindResource("key_DataItem") as DataItem;
                if (dItem != null)
                {
                    dItem.Name = "NameAfterUpdate";
                    dItem.Date = new DateTime(1999, 9, 9);
                    dItem.sITuple = ("TupleStringAfterUpdate", 1999);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button_ShowDataItem_Click(object sender, RoutedEventArgs e)
        {
            DataItem? dItem = this.TryFindResource("key_DataItem") as DataItem;
            if (dItem != null) MessageBox.Show("Базовый DataItem:\n\n" + dItem.ToString());
            else MessageBox.Show("Error");
        }

        private void Button_ShowDataCollection_Click(object sender, RoutedEventArgs e)
        {
            string res = "";
            foreach (var d in dataCollection.Obs)
            {
                res += d.ToString() + "\n";
            }
            MessageBox.Show("Datacollection:\n" + res);
        }
    }
}