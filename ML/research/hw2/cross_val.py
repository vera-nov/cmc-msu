import numpy as np
import typing
from collections import defaultdict


def kfold_split(num_objects: int,
                num_folds: int) -> list[tuple[np.ndarray, np.ndarray]]:
    """Split [0, 1, ..., num_objects - 1] into equal num_folds folds
       (last fold can be longer) and returns num_folds train-val
       pairs of indexes.

    Parameters:
    num_objects: number of objects in train set
    num_folds: number of folds for cross-validation split

    Returns:
    list of length num_folds, where i-th element of list
    contains tuple of 2 numpy arrays, the 1st numpy array
    contains all indexes without i-th fold while the 2nd
    one contains i-th fold
    """
    objects = [i for i in range(num_objects)]
    fold_size = num_objects // num_folds
    res = []
    for i in range(num_folds - 1):
        arr1 = np.array(objects[:(fold_size * i)] + objects[(fold_size * (i + 1)):])
        arr2 = np.array(objects[(fold_size * i):(fold_size * (i + 1))])
        res.append(tuple([arr1, arr2]))
    res.append(tuple([np.array(objects[:fold_size * (num_folds - 1)]), np.array(objects[(num_folds - 1) * fold_size:])]))
    return res


def knn_cv_score(X: np.ndarray, y: np.ndarray, parameters: dict[str, list],
                 score_function: callable,
                 folds: list[tuple[np.ndarray, np.ndarray]],
                 knn_class: object) -> dict[str, float]:
    """Takes train data and counts cross-validation score over
    grid of parameters (all possible parameters combinations)

    Parameters:
    X: train set
    y: train labels
    parameters: dict with keys from
        {n_neighbors, metrics, weights, normalizers}, values of type list,
        parameters['normalizers'] contains tuples (normalizer, normalizer_name)

    score_function: function with input (y_true, y_predict)
        which outputs score metric
    folds: output of kfold_split
    knn_class: class of knn model to fit

    Returns:
    dict: key - tuple of (normalizer_name, n_neighbors, metric, weight),
    value - mean score over all folds
    """
    res = {}
    for n, m, w, normalizer in [
                            (n, m, w, normalizer) for n in parameters["n_neighbors"]
                            for m in parameters["metrics"]
                            for w in parameters["weights"]
                            for normalizer in parameters["normalizers"]]:
        score = 0

        for i in folds:
            X_train = X[i[0]]
            X_val = X[i[1]]
            y_train = y[i[0]]
            y_val = y[i[1]]
            if normalizer[0]:
                normalizer[0].fit(X_train)
                X_train = normalizer[0].transform(X_train)
                X_val = normalizer[0].transform(X_val)
            model = knn_class(n_neighbors=n, metric=m, weights=w)
            model.fit(X=X_train, y=y_train)
            val_res = model.predict(X_val)
            score += score_function(y_val, val_res)

        score /= len(folds)
        res[(normalizer[1], n, m, w)] = score

    return res
