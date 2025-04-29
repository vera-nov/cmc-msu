using Lab1;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Lab1
{
    public class V1MainCollection : List<V1Data>
    {
        
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
                double[] x = new double[] { random.NextDouble() * 5, random.NextDouble() * 10, random.NextDouble() * 15 };
                V1DataArray array = new V1DataArray($"Array_{i}", DateTime.Now, x, DelegateFuncs.FValuesFunc);
                this.Add(array);
            }

            for (int i = 0; i < nL; i++)
            {
                double[] x = new double[] { random.NextDouble() * 5, random.NextDouble() * 10, random.NextDouble() * 15 };
                V1DataList list = new V1DataList($"List_{i}", DateTime.Now, x, DelegateFuncs.FDIFunc);
                this.Add(list);
            }
            this.Add(new V1DataArray("NullArr", DateTime.Now));
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
