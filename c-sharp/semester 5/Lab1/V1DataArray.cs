using Lab1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Lab1
{
    public class V1DataArray : V1Data
    {
        public double[] XCoordinates { get; set; }
        public System.Numerics.Complex[] FieldData { get; set; }
        public V1DataArray(string key, DateTime date) : base(key, date)
        {
            XCoordinates = new double[0];
            FieldData = new System.Numerics.Complex[0];
        }
        public V1DataArray(string key, DateTime date, double[] x, FValues F) : base(key, date)
        {
            XCoordinates = new double[x.Length];
            FieldData = new System.Numerics.Complex[x.Length * 2];
            for (int i = 0; i < x.Length; i++)
            {
                XCoordinates[i] = x[i];
                System.Numerics.Complex y1 = new System.Numerics.Complex();
                System.Numerics.Complex y2 = new System.Numerics.Complex();
                F(x[i], ref y1, ref y2);
                FieldData[i * 2] = y1;
                FieldData[i * 2 + 1] = y2;
            }
        }
        public DataItem? this[int index]
        {
            get
            {
                if (index < 0 || index >= XCoordinates.Length) return null;
                return new DataItem(XCoordinates[index], FieldData[index * 2], FieldData[index * 2 + 1]);
            }
        }
        public override int xLength => XCoordinates.Length;
        public override (double, double) MinMaxDifference
        {
            get
            {
                if (XCoordinates.Length == 0) return (0, 0);
                double minDiff = double.MaxValue;
                double maxDiff = double.MinValue;

                for (int i = 0; i < XCoordinates.Length; i++)
                {
                    double diff = System.Numerics.Complex.Abs(FieldData[2 * i] - FieldData[2 * i + 1]);
                    if (diff < minDiff) minDiff = diff;
                    if (diff > maxDiff) maxDiff = diff;
                }
                return (minDiff, maxDiff);
            }
        }
        public override string ToString()
        {
            return $"Type: Array, Key: {Key}, Date: {Date}, Number of elements: {XCoordinates.Length}";
        }
        public override string ToLongString(string format)
        {
            string result = ToString() + "\n";
            for (int i = 0; i < XCoordinates.Length; i++)
            {
                result += $"X: {XCoordinates[i].ToString(format)}, Y1: {FieldData[2 * i].ToString(format)}, Y2: {FieldData[2 * i + 1].ToString(format)}\n";
            }
            return result;
        }
    }
}
