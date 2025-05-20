#include <iostream>
#include <vector>
#include <cmath>
#include <algorithm>
#include <fstream>
#include <iomanip>
using namespace std;

void write_to_file(const string& filename, const vector<double>& x, const vector<double>& y) {
    ofstream outfile(filename);
    if (!outfile.is_open()) {
        cerr << "Error opening file: " << filename << endl;
        return;
    }
    outfile << "x,y\n";
    for (size_t i = 0; i < x.size(); ++i) {
        outfile << x[i] << "," << y[i] << "\n";
    }
    outfile.close();
}

class HeatEquationSolver {
private:
    const double L = M_PI;
    const double kappa0 = 1.0;
    const double g0 = 1.0;
    const double g1 = 1.0;
    int n;
    std::vector<double> x;
    std::vector<double> h;
    bool non_uniform_grid = false;

    double phi(double t) const {
        return L * (exp(t) - 1.0) / (exp(1.0) - 1.0);
    }

    /*double p(double x) const {
        if (x == M_PI/3.0) return 1.0 + sin(x)/8.0 + sin(x)/6.0;
        else if (x == 2.0*M_PI/3.0) return 1.0 + sin(x)/4.0 + sin(x)/6.0;
        if (x < M_PI/3.0) return 1.0 + 0.25*sin(x);
        else if (x < 2.0*M_PI/3.0) return 1.0 + sin(x)/3.0;
        else return 1.0 + 0.5*sin(x);
    }

    double q(double x) const {
        if (x == M_PI/3.0 || x == 2.0*M_PI/3.0) return 1.5;
        else if (x < M_PI/3.0) return 1.0;
        else if (x < 2.0*M_PI/3.0) return 2.0;
        else return 1.0;
    } */

    double p(double x) const {
        return 1 + sin(x)/3.0;
    }

    double q(double x) {
        const double pi = M_PI;
        const double a = pi/3.0;
        const double b = 2.0*pi/3.0;
        const double k = 20.0;

        const double transition1 = 1.0/(1.0 + exp(-k*(x - a)));
        const double transition2 = 1.0/(1.0 + exp(-k*(x - b)));
        double y = 1.0 + transition1 - transition2;

        return y;
    }


    double f(double x) const {
        return 1.0 - cos(2.0*x);
    }

public:
    HeatEquationSolver(int num_points, bool _non_uniform_grid = false) : n(num_points) {
        non_uniform_grid = _non_uniform_grid;
        x.resize(n);
        h.resize(n);
        if (non_uniform_grid == false) {
            int block = (n + 2) / 3;
            double step = M_PI / 3 / (block - 1);
            for (int i = 0; i < block; ++i) {
                x[i] = step * i;
            }
            for (int i = 1; i < block; ++i) {
                x[block - 1 + i] = M_PI/3 + step * i;
            }
            for (int i = 1; i < block; ++i) {
                x[2 * block - 2 + i] = 2 * M_PI/3 + step * i;
            }
        } /* else {
            for (int i = 0; i < n; ++i) {
                double t = static_cast<double>(i) / (n - 1);
                x[i] = phi(t);
            }
        } */
        else {
            const double t_pi3 = log(1.0 + (M_E - 1.0) / 3.0);
            const double t_2pi3 = log(1.0 + 2.0 * (M_E - 1.0) / 3.0);
            if (n < 7) {
                cout << "Invalid amount of grid points" << endl;
                exit(1);
            } else {
                int points_between = n / 3;
                int counter = 0;
                for (int i = 0; i < points_between; ++i) {
                    double t = t_pi3 * static_cast<double>(i) / points_between;
                    x[counter] = phi(t);
                    counter++;
                }
                for (int i = 0; i < points_between; ++i) {
                    double t = t_pi3 + (t_2pi3 - t_pi3) * static_cast<double>(i) / points_between;
                    x[counter] = phi(t);
                    counter++;
                }
                for (int i = 0; i < points_between; ++i) {
                    double t = t_2pi3 + (1.0 - t_2pi3) * static_cast<double>(i) / points_between;
                    x[counter] = phi(t);
                    counter++;
                }
                x.back() = M_PI;
            }
        }
        h[0] = 0;
        for (int i = 1; i < n; ++i) {
            h[i] = x[i] - x[i-1];
        }
    }

    std::vector<double> solve() {
        std::vector<std::vector<double>> A(n, std::vector<double>(n, 0.0));
        std::vector<double> b(n, 0.0);
        for (int i = 1; i < n-1; ++i) {
            double p_im12 = p((x[i-1] + x[i]) / 2.0);
            double p_ip12 = p((x[i] + x[i+1]) / 2.0);
            double q_i = q(x[i]);
            double h_i = h[i];
            double h_ip1 = h[i+1];
            double h_avg = (h_i + h_ip1) / 2.0;

            A[i][i-1] = -p_im12 / h_i;
            A[i][i] = p_ip12 / h_ip1 + p_im12 / h_i + q_i * h_avg;
            A[i][i+1] = -p_ip12 / h_ip1;
            b[i] = f(x[i]) * h_avg;
        }

        double p_12 = p((x[0] + x[1]) / 2.0);
        double q_0 = q(x[0]);
        A[0][0] = p_12 / h[1] + q_0 * h[1] / 2.0 + kappa0;
        A[0][1] = -p_12 / h[1];
        b[0] = f(x[0]) * h[1] / 2.0 + g0;

        A[n-1][n-1] = 1.0;
        b[n-1] = g1;

        return thomas_algorithm(A, b);
    }
    const std::vector<double>& get_grid() const { return x; }

private:
    std::vector<double> thomas_algorithm(const std::vector<std::vector<double>>& A,
                                        const std::vector<double>& b) {
        int n = A.size();
        std::vector<double> alpha(n, 0.0);
        std::vector<double> beta(n, 0.0);
        std::vector<double> x(n, 0.0);
        double y = A[0][0];

        alpha[0] = -A[0][1] / y;
        beta[0] = b[0] / y;
        for (int i = 1; i < n - 1; i++) {
            y = A[i][i] + A[i][i - 1] * alpha[i - 1];
            alpha[i] = -A[i][i + 1] / y;
            beta[i] = (b[i] - A[i][i - 1] * beta[i - 1]) / y;
        }
        x[n - 1] = (b[n - 1] - A[n - 1][n - 2] * beta[n - 2]) / (A[n - 1][n - 1] + A[n - 1][n - 2] * alpha[n - 2]);
        for (int i = n - 2; i >= 0; i--) {
            x[i] = alpha[i] * x[i + 1] + beta[i];
        }

        return x;
    }
};

int main()
{
    // 7, 28, 106 точек
    HeatEquationSolver solver7(7);
    HeatEquationSolver solver28(28);
    HeatEquationSolver solver106(106);

    vector<double> res7 = solver7.solve();
    vector<double> res28 = solver28.solve();
    vector<double> res106 = solver106.solve();

    vector<double> grid7 = solver7.get_grid();
    vector<double> grid28 = solver28.get_grid();
    vector<double> grid106 = solver106.get_grid();

    HeatEquationSolver solver7NU(7, true);
    HeatEquationSolver solver28NU(28, true);
    HeatEquationSolver solver106NU(106, true);

    vector<double> res7NU = solver7NU.solve();
    vector<double> res28NU = solver28NU.solve();
    vector<double> res106NU = solver106NU.solve();

    vector<double> grid7NU = solver7NU.get_grid();
    vector<double> grid28NU = solver28NU.get_grid();
    vector<double> grid106NU = solver106NU.get_grid();

    write_to_file("uniform_grid7.csv", grid7, res7);
    write_to_file("uniform_grid28.csv", grid28, res28);
    write_to_file("uniform_grid106.csv", grid106, res106);

    write_to_file("nonuniform_grid7.csv", grid7NU, res7NU);
    write_to_file("nonuniform_grid28.csv", grid28NU, res28NU);
    write_to_file("nonuniform_grid106.csv", grid106NU, res106NU);

    cout << "Uniform grid" << endl;
    cout << setw(10) << "Nodes N"
         << setw(15) << "Nodes 2N-1"
         << setw(20) << "Max Error"
         << endl;
    cout << string(45, '-') << endl;

    cout << scientific << setprecision(6);

    for (int i = 7; i < 1000; i += 36) {
        int N = i;
        HeatEquationSolver solver1(N);
        HeatEquationSolver solver2(2*N - 1);

        vector<double> a = solver1.solve();
        vector<double> b = solver2.solve();
        vector<double> x1 = solver1.get_grid();
        vector<double> x2 = solver2.get_grid();

        double max1 = 0;
        for (int i = 0, j = 0; i < N && j < 2*N-1; ++i, j += 2) {
            if (fabs(x1[i] - x2[j]) < 1e-10) {
                double diff = fabs(a[i] - b[j]);
                if (diff > max1) max1 = diff;
            }
        }
        cout << setw(10) << N
             << setw(15) << 2*N-1
             << setw(20) << max1
             << endl;
    }

    cout << "Non-uniform grid" << endl;
    cout << setw(10) << "Nodes N"
         << setw(15) << "Nodes 2N-1"
         << setw(20) << "Max Error"
         << endl;
    cout << string(45, '-') << endl;

    cout << scientific << setprecision(6);

    for (int i = 7; i < 1000; i += 36) {
        int N = i;
        HeatEquationSolver solver1(N, true);
        HeatEquationSolver solver2(2*N - 1, true);

        vector<double> a = solver1.solve();
        vector<double> b = solver2.solve();
        vector<double> x1 = solver1.get_grid();
        vector<double> x2 = solver2.get_grid();

        double max1 = 0;
        for (int i = 0, j = 0; i < N && j < 2*N-1; ++i, j += 2) {
            if (fabs(x1[i] - x2[j]) < 1e-10) {
                double diff = fabs(a[i] - b[j]);
                if (diff > max1) max1 = diff;
            }
        }
        cout << setw(10) << N
             << setw(15) << 2*N-1
             << setw(20) << max1
             << endl;
    }
    cout << "Uniform grid" << endl;
    cout << setw(10) << "Nodes N"
         << setw(25) << "Convergence order"
         << endl;
    cout << string(35, '-') << endl;

    cout << fixed << setprecision(6);

    for (int N = 7; N < 100; N += 3) {
        HeatEquationSolver solver1(N);
        HeatEquationSolver solver2(2*N-1);
        HeatEquationSolver solver3(4*N-3);

        vector<double> a = solver1.solve();
        vector<double> b = solver2.solve();
        vector<double> c = solver3.solve();

        vector<double> x1 = solver1.get_grid();
        vector<double> x2 = solver2.get_grid();
        vector<double> x3 = solver3.get_grid();

        double max1 = 0;
        double max2 = 0;
        for (int i = 0, j = 0; i < N && j < 2*N-1; ++i, j += 2) {
            if (fabs(x1[i] - x2[j]) < 1e-10) {
                double diff = fabs(a[i] - b[j]);
                if (diff > max1) max1 = diff;
            }
        }
        for (int i = 0, j = 0; i < 2*N-1 && j < 4*N-3; i += 1, j += 2) {
            if (fabs(x2[i] - x3[j]) < 1e-10) {
                double diff = fabs(b[i] - c[j]);
                if (diff > max2) max2 = diff;
            }
        }

        double convergence_order = (max1 < 1e-12 || max2 < 1e-12) ? 0 : log2(max1/max2);

        cout << setw(10) << N
             << setw(25) << convergence_order
             << endl;
    }

    cout << "Non-uniform grid" << endl;
    cout << setw(10) << "Nodes N"
         << setw(25) << "Convergence order"
         << endl;
    cout << string(35, '-') << endl;

    cout << fixed << setprecision(6);

    vector<int> test_nodes = {7, 19, 28, 32, 41};

    for (int N = 7; N < 100; N += 3) {
        HeatEquationSolver solver1(N, true);
        HeatEquationSolver solver2(2*N-1, true);
        HeatEquationSolver solver3(4*N-3, true);

        vector<double> a = solver1.solve();
        vector<double> b = solver2.solve();
        vector<double> c = solver3.solve();

        vector<double> x1 = solver1.get_grid();
        vector<double> x2 = solver2.get_grid();
        vector<double> x3 = solver3.get_grid();

        double max1 = 0;
        double max2 = 0;
        for (int i = 0, j = 0; i < N && j < 2*N-1; ++i, j += 2) {
            if (fabs(x1[i] - x2[j]) < 1e-10) {
                double diff = fabs(a[i] - b[j]);
                if (diff > max1) max1 = diff;
            }
        }
        for (int i = 0, j = 0; i < 2*N-1 && j < 4*N-3; i += 1, j += 2) {
            if (fabs(x2[i] - x3[j]) < 1e-10) {
                double diff = fabs(b[i] - c[j]);
                if (diff > max2) max2 = diff;
            }
        }

        double convergence_order = (max1 < 1e-12 || max2 < 1e-12) ? 0 : log2(max1/max2);

        cout << setw(10) << N
             << setw(25) << convergence_order
             << endl;
    }

    return 0;
}
