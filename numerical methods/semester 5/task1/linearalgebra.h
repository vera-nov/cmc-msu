std::vector<std::vector<double>> transpose(const std::vector<std::vector<double>>& A)
{
    int rows = A.size();
    if (rows == 0) return {};
    int cols = A[0].size();

    std::vector<std::vector<double>> T(cols, std::vector<double>(rows, 0.0));
    for (int i = 0; i < rows; i++) {
        for (int j = 0; j < cols; j++) {
            T[j][i] = A[i][j];
        }
    }
    return T;
}

std::vector<std::vector<double>> matrix_multiply(const std::vector<std::vector<double>>& A,
                                                 const std::vector<std::vector<double>>& B)
{
    double tol = 10e-7;
    int rowsA = A.size();
    int colsA = rowsA > 0 ? A[0].size() : 0;
    int rowsB = B.size();
    int colsB = rowsB > 0 ? B[0].size() : 0;

    std::vector<std::vector<double>> C(rowsA, std::vector<double>(colsB, 0.0));
    for (int i = 0; i < rowsA; i++) {
        for (int j = 0; j < colsB; j++) {
            for (int k = 0; k < colsA; k++) {
                C[i][j] += A[i][k] * B[k][j];
            }
            if (std::abs(C[i][j]) < tol) {
                C[i][j] = 0;
            }
        }
    }
    return C;
}

double norm(const std::vector<double>& vec)
{
    double sum = 0.0;
    for (const auto& value : vec) {
        sum += value * value;
    }
    return std::sqrt(sum);
}

double dot_product(const std::vector<double>& vecA, const std::vector<double>& vecB)
{
    double product = 0.0;

    for (size_t i = 0; i < vecA.size(); ++i) {
        product += vecA[i] * vecB[i];
    }

    return product;
}

std::vector<double> projection(const std::vector<double>& u,
                               const std::vector<double>& a)
{
    double dot_u_a = dot_product(u, a);
    double dot_u_u = dot_product(u, u);

    std::vector<double> proj(u.size());
    double scale = dot_u_a / dot_u_u;
    for (size_t i = 0; i < u.size(); ++i) {
        proj[i] = scale * u[i];
    }

    return proj;
}

std::vector<double> scalar_multiply(const std::vector<double>& vec, double scalar)
{
    std::vector<double> result(vec.size());
    for (size_t i = 0; i < vec.size(); ++i) {
        result[i] = vec[i] * scalar;
    }
    return result;
}

std::vector<double> vector_sum(const std::vector<double>& vecA, const std::vector<double>& vecB)
{
    std::vector<double> result(vecA.size());
    for (size_t i = 0; i < vecA.size(); ++i) {
        result[i] = vecA[i] + vecB[i];
    }
    return result;
}

void print_matrix(const std::vector<std::vector<double>>& A)
{
    for (const auto& row : A) {
        for (const auto& val : row) {
            std::cout << val << " ";
        }
        std::cout << std::endl;
    }
}

void print_vector(const std::vector<double>& vec)
{
    for (const auto& elem : vec) {
        std::cout << elem << " ";
    }
    std::cout << std:: endl;
}

double matrix_norm(const std::vector<std::vector<double>>& A)
{
    double maxNorm = 0.0;
    for (size_t i = 0; i < A.size(); ++i) {
        double rowSum = 0.0;
        for (size_t j = 0; j < A[i].size(); ++j) {
            rowSum += std::abs(A[i][j]);
        }
        if (rowSum > maxNorm) {
            maxNorm = rowSum;
        }
    }

    return maxNorm;
}

std::vector<std::vector<double>> matrix_subtract(const std::vector<std::vector<double>>& A, const std::vector<std::vector<double>>& B)
{
    int rows = A.size();
    if (rows == 0) return {};
    int cols = A[0].size();

    std::vector<std::vector<double>> T(rows, std::vector<double>(cols, 0.0));
    for (int i = 0; i < rows; i++) {
        for (int j = 0; j < cols; j++) {
            T[i][j] = A[i][j] - B[i][j];
        }
    }
    return T;
}

std::vector<double> gauss_for_column(int k,
                                     const std::vector<std::vector<double>>& A,
                                     std::vector<std::vector<double>>& R)
{
    int n = A.size();
    std::vector<double> col(A[k]);
    for (int i = 0; i < n; i++){
        for(int j = 0; j < i; j++){
            col[i] -= col[j] * R[j][i];
        }
        col[i] /= R[i][i];
    }
    return col;

}

std::vector<std::vector<double>> generate_x(int n) {
    std::mt19937 gen(static_cast<unsigned int>(std::time(0)));
    std::uniform_real_distribution<double> dist(-1.0, 1.0);

    std::vector<std::vector<double>> x(n, std::vector<double>(1));


    for (int i = 0; i < n; ++i) {
        x[i][0] = dist(gen);
    }

    return x;
}