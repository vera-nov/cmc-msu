import numpy as np


class Preprocessor:

    def __init__(self):
        pass

    def fit(self, X, Y=None):
        pass

    def transform(self, X):
        pass

    def fit_transform(self, X, Y=None):
        pass


class MyOneHotEncoder(Preprocessor):

    def __init__(self, dtype=np.float64):
        super(Preprocessor).__init__()
        self.dtype = dtype
        self.unique_values = None

    def fit(self, X, Y=None):
        """
        param X: training objects, pandas-dataframe, shape [n_objects, n_features]
        param Y: unused
        """
        self.unique_values = [sorted(X[col].unique()) for col in X.columns]

    def transform(self, X):
        """
        param X: objects to transform, pandas-dataframe, shape [n_objects, n_features]
        returns: transformed objects, numpy-array, shape [n_objects, |f1| + |f2| + ...]
        """
        encoded_columns = []
        for i, col in enumerate(X.columns):
            unique_vals = self.unique_values[i]
            val_to_index = {val: idx for idx, val in enumerate(unique_vals)}
            encoded_col = np.zeros((X.shape[0], len(unique_vals)), dtype=self.dtype)
            for row_idx, value in enumerate(X[col]):
                encoded_col[row_idx, val_to_index[value]] = 1
            encoded_columns.append(encoded_col)

        return np.hstack(encoded_columns)

    def fit_transform(self, X, Y=None):
        self.fit(X)
        return self.transform(X)

    def get_params(self, deep=True):
        return {"dtype": self.dtype}


class SimpleCounterEncoder:

    def __init__(self, dtype=np.float64):
        self.dtype = dtype
        self.unique_values = None
        self.encoded_values = None

    def fit(self, X, Y):
        """
        param X: training objects, pandas-dataframe, shape [n_objects, n_features]
        param Y: target for training objects, pandas-series, shape [n_objects,]
        """
        n = len(Y)
        self.unique_values = [sorted(X[col].unique()) for col in X.columns]
        self.encoded_values = [{} for _ in X.columns]
        for i, col in enumerate(X.columns):
            for feature in self.unique_values[i]:
                mask = X[col] == feature
                successes = np.mean(Y[mask]) if mask.any() else 0.0
                counters = np.sum(mask) / n
                self.encoded_values[i][feature] = [successes, counters]

    def transform(self, X, a=1e-5, b=1e-5):
        """
        param X: objects to transform, pandas-dataframe, shape [n_objects, n_features]
        param a: constant for counters, float
        param b: constant for counters, float
        returns: transformed objects, numpy-array, shape [n_objects, 3 * n_features]
        """
        encoded_columns = []
        for i, col in enumerate(X.columns):
            encoded_col = np.zeros((X.shape[0], 3), dtype=self.dtype)
            for row_idx, val in enumerate(X[col]):
                if val in self.encoded_values[i]:
                    successes, counters = self.encoded_values[i][val]
                else:
                    successes, counters = 0.0, 0.0
                encoded_col[row_idx, 0] = successes
                encoded_col[row_idx, 1] = counters
                encoded_col[row_idx, 2] = (successes + a) / (counters + b)
            encoded_columns.append(encoded_col)

        return np.hstack(encoded_columns)

    def fit_transform(self, X, Y, a=1e-5, b=1e-5):
        self.fit(X, Y)
        return self.transform(X, a, b)

    def get_params(self, deep=True):
        return {"dtype": self.dtype}


def group_k_fold(size, n_splits=3, seed=1):
    idx = np.arange(size)
    np.random.seed(seed)
    idx = np.random.permutation(idx)
    n_ = size // n_splits
    for i in range(n_splits - 1):
        yield idx[i * n_: (i + 1) * n_], np.hstack((idx[:i * n_], idx[(i + 1) * n_:]))
    yield idx[(n_splits - 1) * n_:], idx[:(n_splits - 1) * n_]


class FoldCounters:

    def __init__(self, n_folds=3, dtype=np.float64):
        self.dtype = dtype
        self.n_folds = n_folds

    def fit(self, X, Y, seed=1):
        """
        param X: training objects, pandas-dataframe, shape [n_objects, n_features]
        param Y: target for training objects, pandas-series, shape [n_objects,]
        param seed: random seed, int
        """
        self.folds = [i for i in group_k_fold(X.shape[0], self.n_folds, seed)]
        self.coder_model = [SimpleCounterEncoder() for i in range(self.n_folds)]
        for i, sub_X in enumerate(self.folds):
            self.coder_model[i].fit(X.iloc[sub_X[1]], Y.iloc[sub_X[1]])

    def transform(self, X, a=1e-5, b=1e-5):
        """
        param X: objects to transform, pandas-dataframe, shape [n_objects, n_features]
        param a: constant for counters, float
        param b: constant for counters, float
        returns: transformed objects, numpy-array, shape [n_objects, 3]
        """
        transform_X = np.zeros((X.shape[0], 3 * X.shape[1]))
        for i, sub_X in enumerate(self.folds):
            transform_X[sub_X[0]] = self.coder_model[i].transform(X.iloc[sub_X[0]], a, b)
        return transform_X

    def fit_transform(self, X, Y, a=1e-5, b=1e-5):
        self.fit(X, Y)
        return self.transform(X, a, b)


def weights(x, y):
    """
    param x: training set of one feature, numpy-array, shape [n_objects,]
    param y: target for training objects, numpy-array, shape [n_objects,]
    returns: optimal weights, numpy-array, shape [|x unique values|,]
    """
    lst = np.zeros(len(np.unique(x)), dtype=np.float64)
    for i, j in enumerate(np.unique(x)):
        if len(y[x == j]) == sum(y[x == j]):
            lst[i] = 1
        elif sum(y[x == j]) == 0:
            lst[i] = 0
        else:
            lst[i] = sum(y[x == j]) / (len(y[x == j]))
    return lst
