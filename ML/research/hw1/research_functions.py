from collections import Counter
from typing import List


def are_multisets_equal(x: List[int], y: List[int]) -> bool:
    return sorted(x) == sorted(y)


def max_prod_mod_3(x: List[int]) -> int:
    maxprod = -1
    for i in range(1, len(x)):
        if (x[i - 1] * x[i] > maxprod) and ((x[i - 1] % 3 == 0) or (x[i] % 3 == 0)):
            maxprod = x[i - 1] * x[i]
    return maxprod


def convert_image(image: List[List[List[float]]], weights: List[float]) -> List[List[float]]:
    height, width = len(image), len(image[0])
    res = []
    for i in range(height):
        res.append([])
        for j in range(width):
            added_weight = 0
            for channel, weight in zip(image[i][j], weights):
                added_weight += channel * weight
            res[i].append(added_weight)
    return res


def rle_scalar(x: List[List[int]], y:  List[List[int]]) -> int:
    def rle_to_vec(n):
        v = []
        for i in range(len(n)):
            v += [n[i][0]] * n[i][1]
        return v
    x_vector = rle_to_vec(x)
    y_vector = rle_to_vec(y)
    return -1 if (len(x_vector) != len(y_vector)) else sum([x_vector[i] * y_vector[i] for i in range(len(x_vector))])


def cosine_distance(X: List[List[float]], Y: List[List[float]]) -> List[List[float]]:
    
    def calculate_norm(n):
        return sum([i ** 2 for i in n]) ** 0.5
    def calculate_dot_product(n, m):
        return sum([n_el * m_el for n_el, m_el in zip(n, m)])
        
    res = []
    for i in range(len(X)):
        res.append([])
        for j in range(len(Y)):
            if (calculate_norm(X[i]) == 0) or (calculate_norm(Y[j]) == 0):
                res[i].append(1)
            else:
                res[i].append(calculate_dot_product(X[i], Y[j]) / (calculate_norm(X[i]) * calculate_norm(Y[j])))
    return res

    