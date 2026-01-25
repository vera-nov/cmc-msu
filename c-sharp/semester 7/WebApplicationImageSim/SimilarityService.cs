using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WebApplicationImageSim.Data;
using Microsoft.EntityFrameworkCore;

namespace WebApplicationImageSim
{
    public class SimilarityService
    {
        private readonly string _modelPath;
        private static bool _initialized = false;
        private const int ArtificialDelayMs = 2000;

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
        public async Task<(float similarity, float distance)> GetOrComputeFromBytesAsync(
            string imageName1,
            string imageName2,
            byte[] imageBytes1,
            byte[] imageBytes2)
        {
            if (string.Compare(imageName1, imageName2, StringComparison.Ordinal) > 0)
            {
                (imageName1, imageName2) = (imageName2, imageName1);
                (imageBytes1, imageBytes2) = (imageBytes2, imageBytes1);
            }

            await using var db = new AppDbContext();
            await db.Database.EnsureCreatedAsync();
            var existing = await db.ImagePairResults
                .FirstOrDefaultAsync(p => p.Image1Name == imageName1 && p.Image2Name == imageName2);

            if (existing != null)
            {
                return (existing.Similarity, existing.Distance);
            }

            EnsureModelInitialized();

            var embedTask1 = ArcFaceEmbedder.GetEmbeddingAsync(imageBytes1);
            var embedTask2 = ArcFaceEmbedder.GetEmbeddingAsync(imageBytes2);

            await Task.WhenAll(embedTask1, embedTask2);

            var embedding1 = embedTask1.Result;
            var embedding2 = embedTask2.Result;

            var similarity = ArcFaceEmbedder.CosineSimilarity(embedding1, embedding2);
            var distance = 1.0f - similarity;

            await Task.Delay(ArtificialDelayMs);

            var entity = new ImagePairResult
            {
                Image1Name = imageName1,
                Image2Name = imageName2,
                Similarity = similarity,
                Distance = distance
            };

            db.ImagePairResults.Add(entity);
            await db.SaveChangesAsync();
            await db.Database.ExecuteSqlRawAsync("PRAGMA wal_checkpoint(TRUNCATE);");

            return (similarity, distance);
        }

        public async Task ClearDatabaseAsync()
        {
            await using var db = new AppDbContext();
            await db.Database.EnsureCreatedAsync();

            db.ImagePairResults.RemoveRange(db.ImagePairResults);
            await db.SaveChangesAsync();
            await db.Database.ExecuteSqlRawAsync("PRAGMA wal_checkpoint(TRUNCATE);");

        }

        public async Task<List<ImagePairResult>> GetResultsAsync()
        {
            await using var db = new AppDbContext();
            await db.Database.EnsureCreatedAsync();

            return await db.ImagePairResults
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

    }
}
