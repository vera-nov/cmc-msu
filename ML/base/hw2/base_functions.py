from typing import List
from copy import deepcopy


def get_part_of_array(X: List[List[float]]) -> List[List[float]]:
    res = []
    line = 0
    for i in range(0, len(X), 4):
        res.append([])
        for j in range(120, 500, 5):
            res[line].append(X[i][j])
        line += 1
    return res


def sum_non_neg_diag(X: List[List[int]]) -> int:
    only_neg = True
    res = 0
    for i in range(min(len(X), len(X[0]))):
        if X[i][i] >= 0:
            if only_neg == True:
                only_neg = False
            res += X[i][i]
    return res if only_neg == False else -1

def replace_values(X: List[List[float]]) -> List[List[float]]:
    res = deepcopy(X)
    for i in range(len(X[0])):
        mean = 0.0
        for j in range(len(X)):
            mean += X[j][i]
        mean /= len(X)
        for j in range(len(X)):
            if (res[j][i] < 0.25 * mean) or (res[j][i] > 1.5 * mean):
                res[j][i] = -1
    return res
