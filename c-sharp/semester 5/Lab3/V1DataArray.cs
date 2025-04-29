using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Numerics;
using System.Reflection.PortableExecutable;
using Lab3;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Lab3
{
    public class V1DataArray : V1Data
    {
        // ======================== 2 =======================
        public override IEnumerator<DataItem> GetEnumerator()
        {
            for (int i = 0; i < XCoordinates.Length; i++)
            {
                yield return new DataItem(XCoordinates[i], FieldData[2 * i], FieldData[2 * i + 1]);
            }
        }
        public bool Save(string filename)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(filename,
                             FileMode.OpenOrCreate);
                StreamWriter sW = new StreamWriter(fs);


                sW.WriteLine(this.Key);
                sW.WriteLine(this.Date.ToString());
                sW.WriteLine(XCoordinates.Length);
                foreach (var x in XCoordinates)
                {
                    sW.WriteLine(x);
                }
                sW.WriteLine(FieldData.Length);
                foreach (var field in FieldData)
                {
                    sW.WriteLine($"{field.Real}/{field.Imaginary}");
                }
                sW.Flush();
                sW.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save error: {ex.Message}");
                return false;
            }
            finally
            { if (fs != null) fs.Close(); }
        }
        public static bool Load(string filename, ref V1DataArray new_el)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(filename,
                                  FileMode.Open);
                StreamReader sR = new StreamReader(fs);

                new_el.Key = sR.ReadLine();
                new_el.Date = DateTime.Parse(sR.ReadLine());

                int size = int.Parse(sR.ReadLine());
                new_el.XCoordinates = new double[size];
                int i = 0;
                for (i = 0; i < size; i++)
                {
                    new_el.XCoordinates[i] = double.Parse(sR.ReadLine());
                }
                int fieldDataLength = int.Parse(sR.ReadLine());
                new_el.FieldData = new Complex[fieldDataLength];
                for (i = 0; i < fieldDataLength; i++)
                {
                    var complexParts = sR.ReadLine().Split('/');
                    double real = double.Parse(complexParts[0]);
                    double imaginary = double.Parse(complexParts[1]);
                    new_el.FieldData[i] = new Complex(real, imaginary);
                }
                sR.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load error: {ex.Message}");
                return false;
            }
            finally
            { if (fs != null) fs.Close(); }

        }
        // ======================== 2 =======================
        public double[] XCoordinates { get; set; }
        public System.Numerics.Complex[] FieldData { get; set; }
        public V1DataArray(string key = "NULL", DateTime date = default(DateTime)) : base(key, date)
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
