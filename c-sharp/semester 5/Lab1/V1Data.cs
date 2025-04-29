using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Lab1
{
    public delegate void FValues(double x, ref System.Numerics.Complex y1, ref System.Numerics.Complex y2);
    public delegate DataItem FDI(double x);
    public delegate void Fy3Values(double x, ref Complex y3);

    public static class DelegateFuncs
    {
        public static void FValuesFunc(double x, ref System.Numerics.Complex y1, ref System.Numerics.Complex y2)
        {
            Random random = new Random();
            y1 = new System.Numerics.Complex(x, 0);
            y2 = new System.Numerics.Complex(1, x);
            // y1 = new System.Numerics.Complex(x * random.NextDouble() * random.Next(1, 10), x * random.NextDouble() * random.Next(1, 10));
            // y2 = new System.Numerics.Complex(x * random.NextDouble() * random.Next(1, 10), x * random.NextDouble() * random.Next(1, 10));
        }
        public static DataItem FDIFunc(double x)
        {
            Random random = new Random();
            return new DataItem(x, new System.Numerics.Complex(x, 0), new System.Numerics.Complex(1, x));
            //return new DataItem(x, new System.Numerics.Complex(x * random.NextDouble() * random.Next(1, 10), x * random.NextDouble() * random.Next(1, 10)),
            //    new System.Numerics.Complex(x * random.NextDouble() * random.Next(1, 10), x * random.NextDouble() * random.Next(1, 10)));
        }

        public static void Fy3ValuesFunc(double x, ref Complex y3)
        {
            y3 = new Complex(0, 1);
        }
    }

    public struct DataItem
    {
        public double X { get; set; }
        public System.Numerics.Complex Y1 { get; set; }
        public System.Numerics.Complex Y2 { get; set; }
        public DataItem(double _x, System.Numerics.Complex _y1, System.Numerics.Complex _y2)
        {
            X = _x;
            Y1 = _y1;
            Y2 = _y2;
        }
        public string ToString(string format)
        {
            return $"Coordinate: {X.ToString(format)} Y1: {Y1.ToString(format)} Y2: {Y2.ToString(format)}";
        }
        public override string ToString()
        {
            return ToString("F2");
        }


    }
    public abstract class V1Data
    {
        public string Key { get; set; }
        public DateTime Date { get; set; }
        public V1Data(string key, DateTime date)
        {
            Key = key;
            Date = date;
        }
        public abstract int xLength { get; }
        public abstract (double, double) MinMaxDifference { get; }
        public abstract string ToLongString(string format); // вывести tostring + каждый элемент
        public override string ToString()
        {
            return ToLongString("F");
        }

    }

}
