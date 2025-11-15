using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ImageSimilarityApp.Data;
using Microsoft.EntityFrameworkCore;

namespace ImageSimilarityApp.Services
{
    public class SimilarityService
    {
        private readonly string _modelPath;
        private static bool _initialized = false;
        private const int ArtificialDelayMs = 3000;

        public SimilarityService(string modelPath)
        {
            _modelPath = modelPath ?? throw new ArgumentNullException(nameof(modelPath));
        }

        private void EnsureModelInitialized()
        {
            if (_initialized) return;

            ArcFaceEmbedder.Initialize(_modelPath);
            _initialized = true;
        }

        public async Task<(float similarity, float distance)> GetOrComputeAsync(
            string imagePath1,
            string imagePath2,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(imagePath1))
                throw new ArgumentException("Path must not be empty.", nameof(imagePath1));
            if (string.IsNullOrWhiteSpace(imagePath2))
                throw new ArgumentException("Path must not be empty.", nameof(imagePath2));

            var name1 = Path.GetFileName(imagePath1);
            var name2 = Path.GetFileName(imagePath2);

            if (string.Compare(name1, name2, StringComparison.Ordinal) > 0)
            {
                (name1, name2) = (name2, name1);
                (imagePath1, imagePath2) = (imagePath2, imagePath1);
            }

            await using var db = new AppDbContext();
            await db.Database.EnsureCreatedAsync(cancellationToken);

            var existing = await db.ImagePairResults
                .FirstOrDefaultAsync(p => p.Image1Name == name1 && p.Image2Name == name2,
                                     cancellationToken);

            if (existing != null)
            {
                return (existing.Similarity, existing.Distance);
            }

            EnsureModelInitialized();

            var embedTask1 = ArcFaceEmbedder.GetEmbeddingFromFileAsync(imagePath1);
            var embedTask2 = ArcFaceEmbedder.GetEmbeddingFromFileAsync(imagePath2);

            await Task.WhenAll(embedTask1, embedTask2);

            var embedding1 = embedTask1.Result;
            var embedding2 = embedTask2.Result;

            var similarity = ArcFaceEmbedder.CosineSimilarity(embedding1, embedding2);
            var distance = 1.0f - similarity;

            await Task.Delay(ArtificialDelayMs, cancellationToken);

            var entity = new ImagePairResult
            {
                Image1Name = name1,
                Image2Name = name2,
                Similarity = similarity,
                Distance = distance
            };

            db.ImagePairResults.Add(entity);
            await db.SaveChangesAsync(cancellationToken);

            return (similarity, distance);
        }

        public async Task ClearDatabaseAsync(CancellationToken cancellationToken = default)
        {
            await using var db = new AppDbContext();
            await db.Database.EnsureCreatedAsync(cancellationToken);

            db.ImagePairResults.RemoveRange(db.ImagePairResults);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
