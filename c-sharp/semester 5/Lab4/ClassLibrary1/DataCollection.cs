using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLibrary
{
    public class DataCollection
    {
        public ObservableCollection<DataItem> Obs { get; set; }

        public DataCollection() 
        { 
            Obs = new ObservableCollection<DataItem>();
            Obs.Add(new DataItem(1));
            Obs.Add(new DataItem(2));
        }

        public void UpdateDataCollection()
        {
            Obs[0].Name = "Updated";
            Obs[1] = new DataItem(23);
            Obs.Add(new DataItem(15));
        }

        public override string ToString()
        {
            string res = "";
            for (int i = 0; i < Obs.Count; i++)
            {
                res += $"\n{Obs[i].ToString()}";
            }
            return res;
        }
    }
}
