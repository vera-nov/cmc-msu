using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lab3
{
    public class SplineData
    {
        public V1DataArray DataArray { get; set; }
        public int SplineKnots { get; set; } // число узлов равномерной сетки n_s
        public double LeftDerivative { get; set; }
        public double RightDerivative { get; set; }
        public double[] SplineValues { get; set; } // значение сплайна на узлах исходной сетки
        public int MaxIterations { get; set; } // максимальное число итераций в процессе решения задачи минимизации невязки
        public double InitialResidual { get; set; } // значение невязки для начального приближения
        public int Iterations { get; set; } // число сделанных итераций
        public double MinResidual { get; set; } // минимальное значение невязки
        public string StopMessage { get; set; } // причина остановки итераций
        public int vars { get; set; }
        public SplineData(V1DataArray dataArray, int knots, double leftDerivative, double rightDerivative, int maxIter, int _vars)
        {
            DataArray = dataArray;
            SplineKnots = knots;
            LeftDerivative = leftDerivative;
            RightDerivative = rightDerivative;
            MaxIterations = maxIter;
            vars = _vars;
        }

        [DllImport("..\\..\\..\\..\\..\\..\\x64\\DEBUG\\DllLab3.dll",
                       CallingConvention = CallingConvention.Cdecl)]
        public static extern
            void min_norm(
            int n,            // число независимых переменных
            int m,            // число компонент векторной функции 
            double[] x,            // начальное приближение и решение 
            double[] eps,    // массив с 6 элементами, определяющих критерии  
                                    // остановки итерационного процесса 
            double jac_eps,       // точность вычисления элементов матрицы Якоби  
            int niter1,       // максимальное число итераций 
            int niter2,        // максимальное число итераций при выборе пробного шага 
            double rs,            // начальный размер доверительной области 
            ref int ndoneIter,   // число выполненных итераций 
            ref double resInitial,   // начальное значение невязки 
            ref double resFinal,     // финальное значение невязки  
            ref int stopCriteria,// выполненный критерий остановки  
            ref int[] checkInfo,   // информация об ошибках при проверке данных  
            ref int error,     // информация об ошибках
            double[] X,      // массив узлов сплайна  
            double d1L,           // первая производная сплайна на левом конце 
            double d1R,           // первая производная сплайна на правом конце 
            double sL,            // левый конец равномерной сетки 
            double sR,            // правый конец равномерной сетки 
            double[] r,  // массив  значений поля
            double[] f_save); //значения многочлена в точках x

        // вызываем функцию для сплайн-интерполяции
        public void CalculateSpline()
        {
            double[] z = new double[SplineKnots]; // равномерная сетка с n_s узлами (массив узлов сплайна)
            z[0] = DataArray.XCoordinates[0];
            double h = (DataArray.XCoordinates[DataArray.XCoordinates.Length - 1] - z[0]) / (SplineKnots - 1);
            for (int i = 1; i < SplineKnots; i++)
            {
                z[i] = z[0] + h * i;
            }
            double[] x = new double[SplineKnots]; // массив с начальным приближением и решением
            for (int i = 0; i < SplineKnots; i++)
            {
                x[i] = 0;
            }
            if (vars == 2)
            {
                x[0] = 100;
            } else if (vars == 3)
            {
                for (int i = 0; i < SplineKnots; i++)
                {
                    x[i] = 1000;
                }
                x[0] = 0;
            }
            int niter1 = MaxIterations; // максимальное число итераций 
            int niter2 = 100; // максимальное число итераций при выборе пробного шага 
            int ndone_iter = 0; // число выполненных итераций 
            double rs = 10; // начальное значение для доверительного интервала 
            double[] eps = // массив критериев остановки 
            {
                1.0E-12, // размер доверительной области 
                1.0E-12, // норма целевой функции 
                1.0E-12, // норма строк матрицы Якоби 
                1.0E-12, // точность пробного шага
                1.0E-12, // разность нормы целевой функции и погрешности аппроксимации функции 
                1.0E-12 // точность вычисления пробного шага
            };
            double jac_eps = 1.0E-8; // точность вычисления элементов матрицы Якоби
            double[] spline_values = new double[DataArray.XCoordinates.Length]; // значения сплайна в точках
            double resInitial = 1; // начальное значение невязки
            double resFinal = 0; // финальное значение невязки 
            int stopCriteria = 0; // причина остановки итераций 
            int[] checkInfo = { 1, 1, 1, 1 };  // результат проверки корректности данных
            int error = 0;
            var rValues = DataArray.FieldData // массив значений действительной части первой компоненты поля
                 .Where((_, index) => index % 2 == 0)
                 .Select(x => x.Real)
                 .ToArray();

            min_norm(
                SplineKnots, // число независимых переменных
                DataArray.XCoordinates.Length, // число компонент векторной функции
                x, // начальное приближение и решение
                eps, // массив с 6 элементами, определяющих критерии остановки итерационного процесса 
                jac_eps, // точность вычисления элементов матрицы Якоби 
                niter1, // максимальное число итераций 
                niter2, // максимальное число итераций при выборе пробного шага 
                rs, // начальный размер доверительной области 
                ref ndone_iter, // число выполненных итераций
                ref resInitial, // начальное значение невязки 
                ref resFinal, // финальное значение невязки 
                ref stopCriteria, // выполненный критерий остановки 
                ref checkInfo, // информация об ошибках при проверке данных
                ref error, // информация об ошибках 
                DataArray.XCoordinates, // массив узлов сплайна  
                LeftDerivative, // первая производная сплайна на левом конце 
                RightDerivative, // первая производная сплайна на правом конце 
                DataArray.XCoordinates[0], // левый конец равномерной сетки 
                DataArray.XCoordinates[DataArray.XCoordinates.Length - 1], // правый конец равномерной сетки 
                rValues, // массив  значений поля
                spline_values // значения многочлена в точках x
                );

            SplineValues = spline_values;
            InitialResidual = resInitial;
            MinResidual = resFinal;
            Iterations = ndone_iter;

            if (error != 0)
                StopMessage = $"Ошибка в min_norm: {error}";
            else
            {
                StopMessage = "Остановка итерационного процесса по критерию: ";
                if (stopCriteria == 1)
                    StopMessage += $" превышено заданное число итераций {MaxIterations}\n";
                else if (stopCriteria == 2)
                    StopMessage += $" размер доверительной области < {eps[0]}\n";
                else if (stopCriteria == 3)
                    StopMessage += $" норма невязки < {eps[1]}\n";
                else if (stopCriteria == 4)
                    StopMessage += $" норма строк матрицы Якоби < {eps[2]}\n";
                else if (stopCriteria == 5)
                    StopMessage += $" пробный шаг < {eps[3]}\n";
                else if (stopCriteria == 6)
                    StopMessage += $" разность нормы функции и погрешности < {eps[4]}\n";
                else StopMessage += $"\n"; ;
            }

        }
        public string ToLongString(string format)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Data Array:\n{DataArray.ToLongString(format)}");
            sb.AppendLine($"Calculated results:");
            for (int i = 0; i < SplineValues.Length; i++)
            {
                sb.AppendLine($"x_{i}: {DataArray.XCoordinates[i]} r_{i}: {DataArray.FieldData[2 * i].Real} S_{i}: {SplineValues[i].ToString(format)}");
            }
            sb.AppendLine($"Initial Residual: {InitialResidual.ToString(format)}");
            sb.AppendLine($"Min Residual: {MinResidual.ToString(format)}");
            sb.AppendLine($"Iterations Taken: {Iterations}");
            sb.AppendLine($"Stop message: {StopMessage}");
            return sb.ToString();
        }
        public string ToLongString()
        {
            return ToLongString("F15");
        }
        public bool Save(string filename, string format)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(filename,
                         FileMode.Append);
                StreamWriter sW = new StreamWriter(fs);
                sW.WriteLine(ToLongString(format));
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
    }
}
