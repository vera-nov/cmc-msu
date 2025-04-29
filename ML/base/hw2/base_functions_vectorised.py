import numpy as np


def get_part_of_array(X: np.ndarray) -> np.ndarray:
    return X[::4, 120:500:5]


def sum_non_neg_diag(X: np.ndarray) -> int:
    diag = X.diagonal()
    diag = diag[diag >= 0]
    return -1 if diag.size == 0 else np.sum(diag)


def replace_values(X: np.ndarray) -> np.ndarray:
    res = np.copy(X)
    column_mean = np.mean(X, axis=0)
    mask = (res > 1.5 * column_mean) | (res < 0.25 * column_mean)
    res[mask] = -1
    return res