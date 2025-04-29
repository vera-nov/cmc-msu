using System.ComponentModel;

namespace ClassLibrary
{
    public class DataItem : INotifyPropertyChanged, IDataErrorInfo
    {
        public string name { get; set; }
        public DateTime date { get; set; }
        public (string, int) siTuple { get; set; }
        public int lowerBound { get; set; }
        public int upperBound { get; set; }
        public DataItem()
        {
            name = "PrivilegedUser";
            date = DateTime.Now;
            siTuple = ("Flower", 10);
        }
        public DataItem(int j)
        {
            name = "User" + j;
            date = new DateTime(2020 + j, j % 12 + 1, j % 20 + 1);
            siTuple = ("Type" + ((j + 7) % 10 + 1), 10 * ((j + 3) % 10 + 1));
            lowerBound = (j + 5) * 3;
            upperBound = (j + 7) * 5;
        }
        public static DataItem CreateLongTimeDataItem(int j, int pause)
        {
            DataItem dt = new DataItem(j);
            Thread.Sleep(pause);
            return dt;
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Name"));
            }
        }
        public DateTime Date
        {
            get => date;
            set
            {
                date = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Date"));
            }
        }
        public (string, int) SITuple
        {
            get => siTuple;
            set
            {
                SITuple = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("SITuple"));
            }
        }
        public int LowerBound
        {
            get => lowerBound;
            set
            {
                lowerBound = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("LowerBound"));
                    PropertyChanged(this, new PropertyChangedEventArgs("UpperBound"));
                }
            }
        }
        public int UpperBound
        {
            get => upperBound;
            set
            {
                upperBound = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("UpperBound"));
                    PropertyChanged(this, new PropertyChangedEventArgs("LowerBound"));
                }
            }
        }
        public string Error => null;
        public string this[string columnName]
        {
            get
            {
                string res = null;
                switch (columnName)
                {
                    case nameof(Date):
                        if (Date.Year >= 2030)
                        {
                            res = "Year must be less than 2030";
                        }
                        break;
                    case nameof(LowerBound):
                        if (LowerBound >= UpperBound)
                        {
                            res = "LowerBound must be less than UpperBound.";
                        }
                        break;
                    case nameof(UpperBound):
                        if (UpperBound <= LowerBound)
                        {
                            res = "UpperBound must be more than LowerBound.";
                        }
                        break;
                }
                return res;
            }
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public override string ToString() =>
            $"{Name} {Date.ToShortDateString()} {SITuple.ToString()} {LowerBound} {UpperBound}";
    }
}