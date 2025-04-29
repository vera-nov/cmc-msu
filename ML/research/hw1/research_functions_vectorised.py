import numpy as np


def are_multisets_equal(x: np.ndarray, y: np.ndarray) -> bool:
    return np.array_equal(np.sort(x), np.sort(y))


def max_prod_mod_3(x: np.ndarray) -> int:
    prod = x[1:] * x[:-1]
    mask = (prod % 3 == 0)
    return -1 if prod[mask].size == 0 else np.max(prod[mask])


def convert_image(image: np.ndarray, weights: np.ndarray) -> np.ndarray:
    return np.sum(image * weights, axis=2)


def rle_scalar(x: np.ndarray, y: np.ndarray) -> int:
    x_vec = np.repeat(x[..., 0], x[..., 1])
    y_vec = np.repeat(y[..., 0], y[..., 1])
    return int(np.dot(x_vec, y_vec)) if x_vec.shape[0] == y_vec.shape[0] else -1


def cosine_distance(X: np.ndarray, Y: np.ndarray) -> np.ndarray:
    norm_X = np.linalg.norm(X, axis=1)
    norm_Y = np.linalg.norm(Y, axis=1)
    norm = np.outer(norm_X, norm_Y)
    dot_prod = X.dot(Y.T)
    return np.where(norm == 0, 1, dot_prod / norm)
    