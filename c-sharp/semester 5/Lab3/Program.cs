namespace Lab3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            double[] x_coords = { 1, 3, 8, 9 };
    
            V1DataArray dataArray = new V1DataArray("Test Data", DateTime.Now, x_coords, DelegateFuncs.FValuesFunc);

            SplineData splineData = new SplineData(dataArray, 2, 2, 18, 10000, 1);
            splineData.CalculateSpline();
            Console.WriteLine("Начальное приближение 1 - нулевое");
            Console.WriteLine(splineData.ToLongString());
            splineData.Save("output.txt", "F15");

            SplineData splineData2 = new SplineData(dataArray, 2, 2, 18, 10000, 2);
            splineData2.CalculateSpline();
            Console.WriteLine("Начальное приближение 2 - x[0] = 100, x[i] = 0");
            Console.WriteLine(splineData2.ToLongString());
            splineData2.Save("output.txt", "F15");

            SplineData splineData3 = new SplineData(dataArray, 2, 2, 18, 10000, 3);
            splineData3.CalculateSpline();
            Console.WriteLine("Начальное приближение 3 - x[0] = 0, x[i] = 1000");
            Console.WriteLine(splineData3.ToLongString());
            splineData3.Save("output.txt", "F15");

        }
    }
}
