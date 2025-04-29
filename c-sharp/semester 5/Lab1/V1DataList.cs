using Lab1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Lab1
{
    public class V1DataList : V1Data
    {
        public List<DataItem> DataList { get; set; }
        public V1DataList(string key, DateTime date) : base(key, date)
        {
            DataList = new List<DataItem>();
        }
        public V1DataList(string key, DateTime date, double[] x, FDI F) : base(key, date)
        {
            DataList = new List<DataItem>();
            foreach (var xCoord in x.Distinct())
            {
                DataList.Add(F(xCoord));
            }
        }
        public override int xLength => DataList.Count;

        public override (double, double) MinMaxDifference
        {
            get
            {
                var sorted = DataList.Select(num => System.Numerics.Complex.Abs(num.Y1 - num.Y2)).ToList();
                return (sorted.Min(), sorted.Max());
            }
        }
        public static explicit operator V1DataArray(V1DataList source)
        {
            V1DataArray array = new V1DataArray(source.Key, source.Date);
            array.XCoordinates = new double[source.xLength];
            array.FieldData = new System.Numerics.Complex[source.xLength * 2];
            for (int i = 0; i < source.xLength; i++)
            {
                array.XCoordinates[i] = source.DataList[i].X;
                array.FieldData[2 * i] = source.DataList[i].Y1;
                array.FieldData[2 * i + 1] = source.DataList[i].Y2;
            }
            return array;
        }
        public override string ToString()
        {
            return $"Type: List, Key: {Key}, Date: {Date}, Number of elements: {DataList.Count}";
        }
        public override string ToLongString(string format)
        {
            string result = ToString() + "\n";
            foreach (var item in DataList)
            {
                result += item.ToString(format) + "\n";
            }
            return result;
        }
    }
}
