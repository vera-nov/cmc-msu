using System.Collections.ObjectModel;

namespace ClassLibrary
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
            Obs[0].Name = "UpdatedUser";
            Obs[1] = new DataItem(18);
            Obs.Add(new DataItem(8));
        }
        public void Add(DataItem dt) => Obs.Add(dt);
        public override string ToString()
        {
            string res = "";
            for (int j = 0; j < Obs.Count; j++) res += $"{Obs[j].ToString()}\n";
            return res;
        }
    }
}