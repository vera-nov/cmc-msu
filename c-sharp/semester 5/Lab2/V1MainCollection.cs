using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Lab2
{
    public class V1MainCollection : List<V1Data>, IEnumerable<DataItem>
    {
        // ======================== 2 =======================
        IEnumerator<DataItem> IEnumerable<DataItem>.GetEnumerator()
        {
            foreach (V1Data item in this)
            {
                foreach (DataItem dataitem in item)
                {
                    yield return dataitem;
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public double MaxAbsY1
        {
            get
            {
                return this.OfType<V1DataArray>()
                       .Concat(this.OfType<V1DataList>().Select(dl => (V1DataArray)dl))
                       .SelectMany(d => d.FieldData)
                       .Where((field, index) => index % 2 == 0)
                       .Select(field => field.Magnitude)
                       .DefaultIfEmpty(-1)
                       .Max();
            }
        }
        public IEnumerable<double>? XCoord
        {
            get
            {
                var xCounts = this.OfType<V1DataArray>()
                       .Concat(this.OfType<V1DataList>().Select(dl => (V1DataArray)dl))
                       .SelectMany(data => data.XCoordinates)
                       .GroupBy(x => x)
                       .Where(g => g.Count() >= 2)
                       .Select(g => g.Key);
                return xCounts.Any() ? xCounts.Distinct().OrderBy(x => x) : null;
            }
        }
        public IEnumerable<V1Data> MaxRes
        {
            get
            {
                var sizes = this.OfType<V1DataArray>()
                .Select(dataArray => dataArray.XCoordinates.Length)
                .Concat(this.OfType<V1DataList>().Select(dl => dl.DataList.Count))
                .ToList();

                int maxSize = sizes.Count > 0 ? sizes.Max() : 0;
                //Console.WriteLine(maxSize);

                /*var res = this.Where(data =>
                    (data is V1DataArray array && array.XCoordinates.Length == maxSize) ||
                    (data is V1DataList list && list.DataList.Count == maxSize)); */
                var res = this.OfType<V1DataArray>()
                       .Concat(this.OfType<V1DataList>().Select(dl => (V1DataArray)dl))
                       .Where(array => array.XCoordinates.Length == maxSize);
                return res.Any() ? res : null;
            }
        }
        // ======================== 2 =======================
        public V1Data this[string key]
        {
            get
            {
                foreach (var item in this)
                {
                    if (item.Key == key) return item;
                }
                return null;
            }
        }
        public new bool Add(V1Data v1Data)
        {
            foreach (var item in this)
            {
                if (item.Key == v1Data.Key && item.Date == v1Data.Date)
                {
                    return false;
                }
            }
            base.Add(v1Data);
            return true;
        }
        public V1MainCollection(int nA, int nL)
        {
            Random random = new Random();

            for (int i = 0; i < nA; i++)
            {
                // double[] x = new double[] { random.NextDouble() * 5, random.NextDouble() * 10, random.NextDouble() * 15 };
                double[] x = new double[] { 1, 2, 3 };
                V1DataArray array = new V1DataArray($"Array_{i}", DateTime.Now, x, DelegateFuncs.FValuesFunc);
                this.Add(array);
            }
            double[] x1 = new double[] { 1, 2, 4 };
            V1DataArray arr = new V1DataArray("Arr", DateTime.Now, x1, DelegateFuncs.FValuesFunc);
            this.Add(arr);

            for (int i = 0; i < nL; i++)
            {
                // double[] x = new double[] { random.NextDouble() * 5, random.NextDouble() * 10, random.NextDouble() * 15 };
                double[] x = new double[] { 2, 3 };
                V1DataList list = new V1DataList($"List_{i}", DateTime.Now, x, DelegateFuncs.FDIFunc);
                this.Add(list);
            }
            this.Add(new V1DataArray("NullArr", DateTime.Now));
            this.Add(new V1DataList("NullList", DateTime.Now));
        }
        public V1MainCollection()
        {
            double[] x = new double[] { 1, 2, 3 };
            V1DataArray array = new V1DataArray("Arr", DateTime.Now, x, DelegateFuncs.FValuesFunc);
            V1DataList list = new V1DataList("List", DateTime.Now, x, DelegateFuncs.FDIFunc);
            this.Add(array);
            this.Add(list);
        }
        public string ToLongString(string format)
        {
            string result = "";
            foreach (var data in this)
            {
                result += data.ToLongString(format) + "\n";
            }
            return result;
        }
        public override string ToString()
        {
            string result = "";
            foreach (var data in this)
            {
                result += data.ToString() + "\n";
            }
            return result;
        }
    }
}
