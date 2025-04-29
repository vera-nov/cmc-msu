using System.Numerics;

namespace Lab1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 1
            Console.WriteLine("==================== 1 ====================\n");
            double[] xList = { 1.0, 2.0, 3.0 };
            V1DataList dataList = new V1DataList("DataList_1", DateTime.Now, xList, DelegateFuncs.FDIFunc);
            Console.WriteLine("V1DataList ToLongString:");
            Console.WriteLine(dataList.ToLongString("F2"));

            V1DataArray dataArrayFromList = (V1DataArray)dataList;
            Console.WriteLine("V1DataArray (from V1DataList) ToLongString:");
            Console.WriteLine(dataArrayFromList.ToLongString("F2"));

            // 2
            Console.WriteLine("\n==================== 2 ====================\n");
            double[] xArray = { 1.5, 2.5, 3.5 };
            V1DataArray dataArray = new V1DataArray("DataArray_1", DateTime.Now, xArray, DelegateFuncs.FValuesFunc);
            Console.WriteLine("Value for index = 1:");
            Console.WriteLine(dataArray[1]?.ToString() ?? "null");

            Console.WriteLine("Value for index = 10 (out of range):");
            Console.WriteLine(dataArray[10]?.ToString() ?? "null");

            // 3
            Console.WriteLine("\n==================== 3 ====================\n");
            V1MainCollection mainCollection = new V1MainCollection(2, 3);
            Console.WriteLine("V1MainCollection ToLongString:");
            Console.WriteLine(mainCollection.ToLongString("F2"));

            // 4
            Console.WriteLine("\n==================== 4 ====================\n");
            Console.WriteLine("xLength and MinMaxDifference in mainCollection:");
            foreach (var data in mainCollection)
            {
                var (mindiff, maxdiff) = data.MinMaxDifference;
                Console.WriteLine($"{data.GetType().Name}: xLength = {data.xLength}, MinMaxDifference = ({mindiff.ToString("F2")}, {maxdiff.ToString("F2")})");
            }

            // 5
            Console.WriteLine("\n==================== 5 ====================\n");
            Console.WriteLine("Index exists (key = 'Array_0'):");
            Console.WriteLine(mainCollection["Array_0"]?.ToString() ?? "null");

            Console.WriteLine("Index doesn't exist (key = 'AAAAA':");
            Console.WriteLine(mainCollection["AAAAA"]?.ToString() ?? "null");

            // 6
            Console.WriteLine("\n==================== 6 ====================\n");
            V1MainCollection mainCol = new V1MainCollection();
            Console.WriteLine(mainCol.ToLongString("F2"));
            foreach (var data in mainCol)
            {
                var (mindiff, maxdiff) = data.MinMaxDifference;
                Console.WriteLine($"{data.GetType().Name}: xLength = {data.xLength}, MinMaxDifference = ({mindiff.ToString("F2")}, {maxdiff.ToString("F2")})");
            }
        }
    }
}
