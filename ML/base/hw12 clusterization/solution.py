import numpy as np

import sklearn
import sklearn.metrics


def silhouette_score(features, cluster_labels):
    '''
    :param np.ndarray features: Непустой двумерный массив векторов-признаков
    :param np.ndarray cluster_labels: Непустой одномерный массив меток объектов
    :return float: Коэффициент силуэта для выборки features с метками cluster_labels
    '''
    pairwise_distances = sklearn.metrics.pairwise_distances(features)
    unique_clusters, cluster_sizes = np.unique(cluster_labels, return_counts=True)
    num_samples = len(cluster_labels)

    if len(unique_clusters) <= 1:
        return 0

    cluster_masks = np.zeros((num_samples, len(unique_clusters)), dtype=bool)
    intra_cluster_distances = np.zeros((num_samples, len(unique_clusters)))
    sample_cluster_sizes = np.zeros(num_samples)

    for idx, cluster_id in enumerate(unique_clusters):
        cluster_masks[:, idx] = cluster_labels == cluster_id
        intra_cluster_distances[:, idx] = np.sum(pairwise_distances[:, cluster_labels == cluster_id], axis=1)
        sample_cluster_sizes[cluster_labels == cluster_id] = np.sum(cluster_labels == cluster_id)

    singleton_samples = sample_cluster_sizes == 1
    s_distances = intra_cluster_distances[cluster_masks]
    s_distances[singleton_samples] = 0
    s_distances[~singleton_samples] /= (sample_cluster_sizes[~singleton_samples] - 1)

    d_distances = np.min(((intra_cluster_distances / cluster_sizes)[~cluster_masks]).reshape(num_samples, -1), axis=1)
    d_distances[singleton_samples] = 0

    silhouette_values = np.zeros(num_samples)
    max_distances = np.maximum(s_distances, d_distances)
    np.divide(d_distances - s_distances, max_distances, out=silhouette_values, where=(max_distances != 0))

    return np.mean(silhouette_values)


def bcubed_score(true_labels, predicted_labels):
    '''
    :param np.ndarray true_labels: Непустой одномерный массив меток объектов
    :param np.ndarray predicted_labels: Непустой одномерный массив меток объектов
    :return float: B-Cubed для объектов с истинными метками true_labels и предсказанными метками predicted_labels
    '''
    true_uniq, true_inv, true_count = np.unique(true_labels, return_inverse=True, return_counts=True)
    pred_uniq, pred_inv, pred_count = np.unique(predicted_labels, return_inverse=True, return_counts=True)

    true_labels[true_labels == 0] = true_uniq[-1] + 1
    predicted_labels[predicted_labels == 0] = pred_uniq[-1] + 1

    correctness = np.ones((len(true_labels), len(true_labels)))
    correctness[(true_labels / true_labels[:, None]) != 1] = 0
    correctness[(predicted_labels / predicted_labels[:, None]) != 1] = 0

    prec = np.mean(np.sum(correctness, axis=1) / pred_count[pred_inv])
    recall = np.mean(np.sum(correctness, axis=1) / true_count[true_inv])

    return 2 * prec * recall / (prec + recall)
