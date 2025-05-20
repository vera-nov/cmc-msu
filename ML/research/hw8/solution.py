import numpy as np
from sklearn.base import BaseEstimator
from sklearn.cluster import KMeans


class KMeansClassifier(BaseEstimator):
    def __init__(self, n_clusters):
        super().__init__()
        self.n_clusters = n_clusters
        self.kmeans = KMeans(n_clusters=n_clusters, random_state=0)
        self.cluster_to_class_mapping = None

    def fit(self, data, labels):
        self.kmeans.fit(data)
        cluster_labels = self.kmeans.labels_
        self.cluster_to_class_mapping, _ = self._best_fit_classification(cluster_labels, labels)
        return self

    def predict(self, data):
        cluster_labels = self.kmeans.predict(data)
        return self.cluster_to_class_mapping[cluster_labels]

    def _best_fit_classification(self, cluster_labels, true_labels):
        labeled_mask = (true_labels != -1)
        labeled_clusters = cluster_labels[labeled_mask]
        labeled_classes = true_labels[labeled_mask]

        unique_classes, class_counts = np.unique(labeled_classes, return_counts=True)
        default_class = unique_classes[np.argmax(class_counts)]

        mapping = np.full(self.n_clusters, default_class, dtype=np.int64)

        for cluster in range(self.n_clusters):
            mask = (cluster_labels == cluster) & labeled_mask
            cluster_classes = true_labels[mask]

            if len(cluster_classes) > 0:
                unique, counts = np.unique(cluster_classes, return_counts=True)
                mapping[cluster] = unique[np.argmax(counts)]

        predicted_labels = mapping[cluster_labels]
        return mapping, predicted_labels
