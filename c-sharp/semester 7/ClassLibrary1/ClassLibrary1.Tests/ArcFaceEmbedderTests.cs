using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using ClassLibrary1;

namespace ClassLibrary1.Tests
{
    public class ArcFaceEmbedderTests
    {
        private static string ModelPath => Path.Combine("TestData", "arcfaceresnet100-8.onnx");
        private static string Face1Path => Path.Combine("TestData", "face1.png");
        private static string Face2Path => Path.Combine("TestData", "face2.png");
        private static string DifImgPath => Path.Combine("TestData", "dif_img.png");
        private static string R1Path => Path.Combine("TestData", "img.png");
        private static string R2Path => Path.Combine("TestData", "rotated_img.png");

        public ArcFaceEmbedderTests()
        {
            ArcFaceEmbedder.Initialize(ModelPath);
        }
        [Fact]
        public async Task FaceSimilarity_RotatedImage()
        {
            var a = await ArcFaceEmbedder.GetEmbeddingFromFileAsync(R1Path);
            var b = await ArcFaceEmbedder.GetEmbeddingFromFileAsync(R2Path);

            var same = ArcFaceEmbedder.Similarity(a, b);
            Assert.InRange(same, 0.7f, 1.001f);
        }

        [Fact]
        public void Initialize_WithEmptyPath_Throws()
        {
            Assert.Throws<ArgumentException>(() => ArcFaceEmbedder.Initialize(""));
        }

        [Fact]
        public void Initialize_SamePathTwice_DoesNotThrow_AndKeepsSession()
        {
            ArcFaceEmbedder.Initialize(ModelPath);
            var s1 = ArcFaceEmbedder.PeekSession();
            ArcFaceEmbedder.Initialize(ModelPath);
            var s2 = ArcFaceEmbedder.PeekSession();
            Assert.Same(s1, s2);
        }


        [Fact]
        public void Session_IsSingleton()
        {
            var s1 = ArcFaceEmbedder.PeekSession();
            var s2 = ArcFaceEmbedder.PeekSession();
            Assert.Same(s1, s2);
        }
        [Fact]
        public async Task MultipleConcurrentCalls_ShareSameModelSingleton()
        {
            ArcFaceEmbedder.Initialize(ModelPath);
            var sessionBefore = ArcFaceEmbedder.PeekSession();

            var t1 = ArcFaceEmbedder.GetEmbeddingFromFileAsync(Face1Path);
            var t2 = ArcFaceEmbedder.GetEmbeddingFromFileAsync(Face1Path);
            var embeddings = await Task.WhenAll(t1, t2);

            Assert.Equal(embeddings[0].Length, embeddings[1].Length);
            var cos = ArcFaceEmbedder.CosineSimilarity(embeddings[0], embeddings[1]);
            Assert.InRange(cos, 0.999f, 1.001f);

            var sessionAfter = ArcFaceEmbedder.PeekSession();
            Assert.Same(sessionBefore, sessionAfter);
        }


        [Fact]
        public async Task GetEmbedding_FromFile_ReturnsUnitVector_NoNaNs()
        {
            var emb = await ArcFaceEmbedder.GetEmbeddingFromFileAsync(Face1Path);
            Assert.NotNull(emb);
            Assert.NotEmpty(emb);
            Assert.All(emb, x => Assert.False(float.IsNaN(x) || float.IsInfinity(x)));

            var self = ArcFaceEmbedder.Similarity(emb, emb);
            Assert.InRange(self, 0.999f, 1.001f);
        }


        [Fact]
        public async Task GetEmbedding_NonExistingFile_Throws()
        {
            var missing = Path.Combine(Path.GetTempPath(), $"no_such_{Guid.NewGuid():N}.png");
            var ex = await Assert.ThrowsAsync<FileNotFoundException>(
                () => ArcFaceEmbedder.GetEmbeddingFromFileAsync(missing));
            Assert.Contains("could not find", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ThreadSafety_ManyConcurrentCalls_SameResult()
        {
            var tasks = Enumerable.Range(0, 8)
                                  .Select(_ => ArcFaceEmbedder.GetEmbeddingFromFileAsync(Face1Path))
                                  .ToArray();

            var res = await Task.WhenAll(tasks);
            Assert.Equal(8, res.Length);

            for (int i = 1; i < res.Length; i++)
            {
                var sim = ArcFaceEmbedder.Similarity(res[0], res[i]);
                Assert.InRange(sim, 0.97f, 1.001f);
            }
        }

        [Fact]
        public async Task FaceSimilarity_SelfVsOther_OrderIsCorrect()
        {
            var a = await ArcFaceEmbedder.GetEmbeddingFromFileAsync(Face1Path);
            var b = await ArcFaceEmbedder.GetEmbeddingFromFileAsync(Face2Path);

            var self = ArcFaceEmbedder.Similarity(a, a);
            var ab = ArcFaceEmbedder.Similarity(a, b);

            Assert.InRange(self, 0.999f, 1.001f);
            Assert.True(self >= ab);
        }

        [Fact]
        public async Task FaceSimilarity_SamePersonVSOther()
        {
            var a = await ArcFaceEmbedder.GetEmbeddingFromFileAsync(Face1Path);
            var b = await ArcFaceEmbedder.GetEmbeddingFromFileAsync(Face2Path);
            var c = await ArcFaceEmbedder.GetEmbeddingFromFileAsync(DifImgPath);

            var same = ArcFaceEmbedder.Similarity(a, b);
            var diff = ArcFaceEmbedder.Similarity(a, c);
            Assert.True(same >= diff);
        }

        [Fact]
        public void DisposeSession_AllowsReinitialize()
        {
            ArcFaceEmbedder.DisposeSession();
            Assert.Null(ArcFaceEmbedder.PeekSession());

            ArcFaceEmbedder.Initialize(ModelPath);
            Assert.NotNull(ArcFaceEmbedder.PeekSession());
        }
    }
}
