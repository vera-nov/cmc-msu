#include <iostream>
#include <vector>
#include <iomanip>
#include <cmath>

using namespace std;

class RungeKuttaSolver
{
    double lambda;
    double end_time;
    vector<double> tau;

public:
    RungeKuttaSolver() : lambda(0), tau({0}), end_time(0) {}

    RungeKuttaSolver(double l, double time, const vector<double>& steps)
        : lambda(l), tau(steps), end_time(time) {}

    double computeExactSolution(double t)
    {
        return exp(lambda * t);
    }

    double computeNumericalSolution(int n, double step_size)
    {
        double z = lambda * step_size;
        return pow(1 + z + z*z/2 + z*z*z/6, n);
    }

    void analyzeConvergence(double t)
    {
        double step = tau[0];
        int n = int(t / step);

        double error_tau = abs(computeExactSolution(t) - computeNumericalSolution(n, step));
        double error_tau2 = abs(computeExactSolution(t) - computeNumericalSolution(2*n, step/2));
        double convergence_order = log2(error_tau / error_tau2);

        cout << fixed << setprecision(2)
             << "Convergence order for lambda = " << lambda
             << " at t = " << t
             << " with time steps: " << step << " and " << step/2
             << " is " << convergence_order << endl;
    }

    void printResults()
    {
        cout << "Solution of Cauchy problem on interval [0, "
             << int(end_time) << "], lambda = " << lambda << endl;

        for (double step : tau) {
            int step_count = 0;
            double current_time = 0;

            cout << "\nTime step = " << step << "\n"
                 << left << setw(9) << "Time"
                 << setw(18) << "Exact Solution"
                 << setw(21) << "Numerical Solution"
                 << "Error" << endl;

            while (current_time <= end_time) {
                double exact = computeExactSolution(current_time);
                double numerical = computeNumericalSolution(step_count, step);

                cout << fixed << setprecision(2) << left << setw(9) << current_time
                     << fixed << setprecision(10) << left << setw(18) << exact
                     << setw(21) << numerical
                     << abs(exact - numerical) << endl;

                step_count++;
                current_time += step;
            }
        }
    }
};

int main()
{
    vector<double> steps = {0.5, 0.2, 0.1};
    RungeKuttaSolver solver(-1.0, 3.0, steps);

    solver.printResults();
    solver.analyzeConvergence(1.0);

    vector<double> steps2 = {1.0, 0.5, 0.2};
    RungeKuttaSolver solver2(-5.0, 3.0, steps2);
    solver2.printResults();

    return 0;
}
