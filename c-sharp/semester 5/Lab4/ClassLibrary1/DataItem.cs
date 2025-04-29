using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLibrary
{
    public class DataItem: INotifyPropertyChanged, IDataErrorInfo
    {
        public string name { get; set; }
        public DateTime date { get; set; }
        public (string, int) sITuple { get; set; }

        public DataItem()
        {
            name = "noName";
            date = DateTime.Now;
            sITuple = ("\"InitStr\"", 12345);
            LowerBound = 0;
            UpperBound = 100;
        }

        public DataItem(int j)
        {
            name = "Name = " + j;
            date = new DateTime(2020 + j, j % 12 + 1, j % 28 + 1);
            sITuple = (" \" " + j * j +  " \" ", j * j * j);
            LowerBound = j;
            UpperBound = j*100;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => name;
            set 
            {
                name = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        public DateTime Date
        {
            get => date;
            set
            {
                date = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(nameof(Date)));
            }
        }

        public (string, int) SITuple 
        { 
            get => sITuple;
            set
            {
                sITuple = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(nameof(SITuple)));
            }
        }

        public int LowerBound { get; set; }
        public int UpperBound { get; set; }

        public override string ToString()
        {
            string res = "";
            res += $"{name}\n";
            res += $"Date = {date}\n";
            res += $"( {sITuple.Item1} , {sITuple.Item2} )\n";
            res += $"lower = {LowerBound}\n";
            res += $"lower = {UpperBound}\n";
            return res;
        }

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Date):
                        if (Date.Year >= 2030)
                            return "Год в свойстве Date должен быть меньше 2030.";
                        break;

                    case nameof(LowerBound):
                        if (LowerBound >= UpperBound)
                            return "LowerBound должен быть меньше UpperBound.";
                        break;
                }
                return null;
            }
        }
    }
}
