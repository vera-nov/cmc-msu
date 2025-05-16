#include <iostream>
#include <fstream>
#include <sstream>
#include <vector>
#include <stdexcept>
#include <string>
#include <cmath>
#include <chrono>
#include <random>
#include "linearalgebra.h"

void gramSchmidt(const std::vector<std::vector<double>>& A,
                 std::vector<std::vector<double>>& Q,
                 std::vector<std::vector<double>>& R,
                 int n)
{
    Q[0] = A[0];
    for (int k = 1; k < n; k++) {
            Q[k] = A[k];
        for(int i = 0; i < k; i++) {
            Q[k] = vector_sum(Q[k], scalar_multiply(projection(Q[i], A[k]), -1));
        }
    }
    for (int k = 0; k < n; k++) {
        Q[k] = scalar_multiply(Q[k], 1 / norm(Q[k]));
    }
    R = matrix_multiply(Q, transpose(A));
}

void householder(const std::vector<std::vector<double>>& A,
                 std::vector<std::vector<double>>& Q,
                 std::vector<std::vector<double>>& R,
                 int n)
{
    R = std::vector<std::vector<double>>(A);
    double sign;
    double refl_length;
    double refl_val;
    double norm;
    double norm_sqrt;
    std::vector<double> scal(n, 0);
    std::vector<double> refl_coeff(n, 0);
    std::vector<double> w(n,0);

    for(int j = 0; j < n - 1; j++) {
        sign = (R[j][j] >= 0.0) ? 1.0 : -1.0;
        norm_sqrt = 0;

        for(int i = j; i < n; i++) {
            norm_sqrt += R[i][j] * R[i][j];
        }

        norm = sqrt(norm_sqrt);
        refl_length = sign * norm;
        refl_val = R[j][j] + refl_length;
        norm_sqrt = norm_sqrt + refl_val * refl_val - R[j][j] * R[j][j];
        norm = sqrt(norm_sqrt);
        w[j] = refl_val / norm;
        refl_coeff[j] = 2.0 * w[j];

        for(int i = j + 1; i < n; i++) {
            w[i] = R[i][j] / norm;
            refl_coeff[i] = 2.0 * w[i];
        }
        sign = (R[j][j] >= 0.0) ? -1.0 : 1.0;

        for(int p = j; p < n; p++){
            scal[p] = 0;
            for(int k = j; k < n; k++) {
                scal[p] += R[k][p] * w[k];
            }
        }

        for(int p = j; p < n; p++) {
            for(int k = j; k < n; k++) {
                R[p][k] = sign * (R[p][k] - refl_coeff[p] * scal[k]);
            }
        }
    }

    for(int i = 0; i < n; i++) {
        Q[i] = gauss_for_column(i, A, R);
    }
}

std::vector<std::vector<double>> solve_system(std::vector<std::vector<double>>& R,
                                              std::vector<std::vector<double>>& f,
                                              int n)
{
    std::vector<std::vector<double>> x(n, std::vector<double>(1));
    for(int i = n - 1; i >= 0; i--) {
        x[i][0] = f[i][0];
        for (int j = i + 1; j < n; j++) {
            x[i][0] -= R[i][j] * x[j][0];
        }
        x[i][0] /= R[i][i];
    }
    return x;
}

int main()
{
    // std::string filename = "test.txt";
    std::string filename = "SLAU_var_7.csv";
    std::ifstream file(filename);

    std::vector<std::vector<double>> A;
    std::string line;

    while (std::getline(file, line)) {
        std::stringstream ss(line);
        std::string value;
        std::vector<double> row;

        while (std::getline(ss, value, ',')) {
            row.push_back(std::stod(value));
        }

        A.push_back(row);
    }

    file.close();

    int n = A.size();

    std::vector<std::vector<double>> A_transpose = transpose(A);

    std::vector<std::vector<double>> Q(n, std::vector<double>(n, 0.0));
    std::vector<std::vector<double>> R(n, std::vector<double>(n, 0.0));

    // Gram-Schmidt
    std::cout << "Gram-Schmidt" << std::endl;
    auto start = std::chrono::high_resolution_clock::now();
    gramSchmidt(A_transpose, Q, R, n);
    auto stop = std::chrono::high_resolution_clock::now();

    Q = transpose(Q);
    std::cout << "Result: " << matrix_norm(matrix_subtract(matrix_multiply(Q, R), A))<< std::endl;
    auto duration = std::chrono::duration_cast<std::chrono::milliseconds>(stop - start);
    std::cout << "Time taken: " << duration.count() << " milliseconds" << std::endl << std::endl;

    // Householder
    std::cout << "Householder" << std::endl;
    start = std::chrono::high_resolution_clock::now();
    householder(A, Q, R, n);
    stop = std::chrono::high_resolution_clock::now();

    std::cout << "Result: " << matrix_norm(matrix_subtract(matrix_multiply(Q, R), A))<< std::endl;
    duration = std::chrono::duration_cast<std::chrono::milliseconds>(stop - start);
    std::cout << "Time taken: " << duration.count() << " milliseconds" << std::endl << std::endl;

    // ----------------------------------------------------------------

    std::vector<std::vector<double>> x;
    std::vector<std::vector<double>> x_sol;
    std::vector<std::vector<double>> f;
    start = std::chrono::high_resolution_clock::now();
    // решим систему несколько раз, потому что иначе время округляется до 0 мс

    for (int i = 0; i < 100; ++i) {
        x = generate_x(n);
        f = matrix_multiply(A, x);
        std::vector<std::vector<double>> Qtf = matrix_multiply(transpose(Q), f);
        x_sol = solve_system(R, Qtf, n);
    }

    stop = std::chrono::high_resolution_clock::now();

    std::cout << "Residual: " << matrix_norm(matrix_subtract(f, matrix_multiply(A, x_sol))) << std::endl;
    std::cout << "Solution error: " << matrix_norm(matrix_subtract(x, x_sol)) << std::endl;

    duration = std::chrono::duration_cast<std::chrono::milliseconds>(stop - start);
    std::cout << "Time taken: " << duration.count() / 100 << " milliseconds" << std::endl;
    return 0;
}