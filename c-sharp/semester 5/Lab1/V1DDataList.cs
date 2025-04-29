using Lab1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Lab1
{
    public class V1DDataList : V1DataList
    {
        public List<Complex> Y3Data;
        public V1DDataList(string key, DateTime date, double[] x, FDI F, Fy3Values Fy3) : base(key, date, x, F)
        {
            Y3Data = new List<Complex>();
            foreach (var xCoord in x)
            {
                Complex y3 = new Complex();
                Fy3(xCoord, ref y3);
                Y3Data.Add(y3);
            }
        }
        public override (double, double) MinMaxDifference
        {
            get
            {
                if (Y3Data.Count == 0) return (0, 0);
                double minDiff = double.MaxValue;
                double maxDiff = double.MinValue;

                for (int i = 0; i < Y3Data.Count; i++)
                {
                    double diff = Complex.Abs(DataList[i].Y1 - Y3Data[i]);
                    if (diff < minDiff) minDiff = diff;
                    if (diff > maxDiff) maxDiff = diff;
                }
                return (minDiff, maxDiff);
            }
        }
        public override string ToString()
        {
            return $"Type: V1DList, Key: {Key}, Date: {Date}, Number of elements: {DataList.Count}";
        }
        public override string ToLongString(string format)
        {
            string result = ToString() + "\n";
            for (int i = 0; i < Y3Data.Count; i++)
            {
                result += DataList[i].ToString(format) + $" Y3: {Y3Data[i].ToString(format)}\n";
            }
            return result;
        }
    }
}
