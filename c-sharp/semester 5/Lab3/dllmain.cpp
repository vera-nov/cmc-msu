#include "pch.h"
#include <iostream>
#include "C:\Users\Vera\source\repos\Lab3\packages\intelmkl.devel.win-x64.2025.0.1.5\build\native\include\mkl.h"
using namespace std;

int calculateSpline(
    int x_len, // число узлов сплайна 
    double* x_coords, // массив узлов сплайна
    double* y_values, // массив заданных значений векторной функции 
    double deriv_left,
    double deriv_right,
    int ns, // число узлов равномерной сетки, на которой вычисляются значения сплайна 
    double sL, // левый конец равномерной сетки 
    double sR, // правый конец равномерной сетки 
    double* spline_values) // массив вычисленных значений сплайна
{
    try
    {
        int n = x_len;
        MKL_INT spline_order = DF_PP_CUBIC; //  степень кубического сплайна 
        MKL_INT s_type = DF_PP_NATURAL;  // тип сплайна
        MKL_INT bc_type = DF_BC_1ST_LEFT_DER | DF_BC_1ST_RIGHT_DER; // тип граничных условий - первая производная на обоих концах 
        double bc_values[2] = { deriv_left, deriv_right };  // массив граничных значений
        double grid[2] { x_coords[0], x_coords[n - 1] };// массив концов равномерной сетки, на которой вычисляются значения сплайна

        // массив для коэффициентов сплайна 
        double* coef = new double[(ns - 1) * spline_order]; // ns - number of knots, spline_order - degree + 1
        DFTaskPtr task;
        int status = -1;
        // Cоздание задачи (task) 
        status = dfdNewTask1D(&task, ns, grid, DF_UNIFORM_PARTITION, 1, y_values, DF_NO_HINT);
        if (status != DF_STATUS_OK)  throw 1;

        // Настройка параметров задачи
        status = dfdEditPPSpline1D(task, spline_order, s_type, bc_type, bc_values, DF_NO_IC, NULL, coef, DF_NO_HINT);
        if (status != DF_STATUS_OK) throw 2;

        // Создание сплайна
        status = dfdConstruct1D(task, DF_PP_SPLINE, DF_METHOD_STD);
        if (status != DF_STATUS_OK) throw 3;

        int nDorder = 1;         // число производных, которые вычисляются, плюс 1 
        MKL_INT dorder[] = { 1 };  // вычисляются значения сплайна,  
        status = dfdInterpolate1D(task, DF_INTERP, DF_METHOD_PP, n,
            x_coords, DF_NON_UNIFORM_PARTITION, nDorder, dorder, NULL,
            spline_values, DF_NO_HINT, NULL);
        if (status != DF_STATUS_OK) throw 4;

        status = dfDeleteTask(&task);
        if (status != DF_STATUS_OK) throw 6;
        delete[] coef;
    }
    catch (int ret)
    {
        return ret;
    }
    return 0;
}

struct user_data
{
    double* r; // массив значений поля
    double* X; // массив узлов сплайна (XCoordinates)
    double d1L; // первая производная слева
    double d1R; // первая производная справа
    double sL; // левый конец равномерной сетки 
    double sR; // правый конец равномерной сетки
    double* spline_values; // значения многочлена в точках x
};

void TestFunction(int* m, int* ns, double* x, double* f, void* parameters)
{
    double* r = ((user_data*)parameters)->r;
    int res;
    double* f_s = ((user_data*)parameters)->spline_values;
    res = calculateSpline(*m, ((user_data*)parameters)->X, x, ((user_data*)parameters)->d1L,
        ((user_data*)parameters)->d1R, *ns, ((user_data*)parameters)->sL, ((user_data*)parameters)->sR, f);
    if (res != 0) {
        throw 1;
    }
    for (int i = 0; i < *m; i++) {
        f_s[i] = f[i];
    }
    for (int i = 0; i < *m; i++) {
        f[i] = f[i] - r[i];
    }
}

extern "C" _declspec(dllexport)
void min_norm(
    MKL_INT n,            // число независимых переменных (ns)
    MKL_INT m,            // число компонент векторной функции (n)
    double* x,            // начальное приближение и решение 
    const double* eps,    // массив с 6 элементами, определяющих критерии  
                          // остановки итерационного процесса 
    double jac_eps,       // точность вычисления элементов матрицы Якоби  
    MKL_INT niter1,       // максимальное число итераций 
    MKL_INT niter2,       // максимальное число итераций при выборе пробного шага 
    double rs,            // начальный размер доверительной области 
    MKL_INT* ndoneIter,   // число выполненных итераций 
    double* resInitial,   // начальное значение невязки 
    double* resFinal,     // финальное значение невязки  
    MKL_INT* stopCriteria,// выполненный критерий остановки  
    MKL_INT* checkInfo,   // информация об ошибках при проверке данных  
    int* error,           // информация об ошибках
    double* X,            // массив узлов сплайна  
    double d1L,           // первая производная сплайна на левом конце 
    double d1R,           // первая производная сплайна на правом конце 
    double sL,            // левый конец равномерной сетки 
    double sR,            // правый конец равномерной сетки 
    double* r,            // массив значений поля
    double* spline_values)// значения многочлена в точках x
{
    _TRNSP_HANDLE_t handle = NULL;  // переменная для дескриптора задачи 

    double* fvec = NULL;           // массив значений векторной функции 
    double* fjac = NULL;           // массив с элементами матрицы Якоби 

    error = 0;
    int spl;
    try
    {
        fvec = new double[m];         // массив значений векторной функции 
        fjac = new double[n * m];     // массив с элементами матрицы Якоби 
        user_data user_data = { r, X, d1L, d1R,  sL,  sR, spline_values };
        // Инициализация задачи 
        MKL_INT ret = dtrnlsp_init(&handle, &n, &m, x, eps, &niter1, &niter2, &rs);
        if (ret != TR_SUCCESS) throw 1;//(ErrorEnum(ErrorEnum::INIT));

        // Проверка корректности входных данных  
        ret = dtrnlsp_check(&handle, &n, &m, fjac, fvec, eps, checkInfo);
        if (ret != TR_SUCCESS) 2;//throw (ErrorEnum(ErrorEnum::CHECK));

        MKL_INT RCI_Request = 0;   // надо инициализировать 0 !!! 

        // Итерационный процесс 
        while (true)
        {
            ret = dtrnlsp_solve(&handle, fvec, fjac, &RCI_Request);
            if (ret != TR_SUCCESS) 3;//throw (ErrorEnum(ErrorEnum::SOLVE));

            if (RCI_Request == 0) continue;
            else if (RCI_Request == 1)
            {
                try
                {
                    TestFunction(&m, &n, x, fvec, &user_data);
                }
                catch (int a) { throw 2; }

            }
            else if (RCI_Request == 2)
            {

                ret = djacobix(&TestFunction, &n, &m, fjac, x, &jac_eps, &user_data);

                if (ret != TR_SUCCESS) throw 3;
            }
            else if (RCI_Request >= -6 && RCI_Request <= -1) break;
            else throw 4;
        }

        // Завершение итерационного процесса 
        ret = dtrnlsp_get(&handle, ndoneIter, stopCriteria,
            resInitial, resFinal);
        if (ret != TR_SUCCESS) throw 5;

        //  Освобождение ресурсов 
        ret = dtrnlsp_delete(&handle);
        if (ret != TR_SUCCESS) throw 6;
    }
    catch (int err) { if (err) *error = err; }

    // Освобождение памяти 
    if (fvec != NULL)  delete[] fvec;
    if (fjac != NULL)  delete[] fjac;

}