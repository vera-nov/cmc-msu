using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using ClassLibrary1;

namespace ArcFaceDemo
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string modelPath = args.Length > 0 ? args[0] : "arcfaceresnet100-8.onnx";
            string face1Path = args.Length > 1 ? args[1] : "face1.png";
            string face2Path = args.Length > 2 ? args[2] : "face2.png";

            using (var session = new InferenceSession(modelPath))
            {
                Console.WriteLine("Predicting contents of image...");
                foreach (var kv in session.InputMetadata)
                    Console.WriteLine($"{kv.Key}: {MetadataToString(kv.Value)}");
                foreach (var kv in session.OutputMetadata)
                    Console.WriteLine($"{kv.Key}: {MetadataToString(kv.Value)}");
            }

            ArcFaceEmbedder.Initialize(modelPath);

            var embeddings1 = await ArcFaceEmbedder.GetEmbeddingFromFileAsync(face1Path);
            var embeddings2 = await ArcFaceEmbedder.GetEmbeddingFromFileAsync(face2Path);

            float dist = EuclideanDistance(embeddings1, embeddings2);
            Console.WriteLine($"Distance =  {dist * dist}");
            Console.WriteLine($"Similarity =  {ArcFaceEmbedder.CosineSimilarity(embeddings1, embeddings2)}");
        }

        static string MetadataToString(NodeMetadata metadata)
            => $"{metadata.ElementType}[{string.Join(",", metadata.Dimensions.Select(d => d.ToString()))}]";

        static float EuclideanDistance(float[] v1, float[] v2)
        {
            if (v1.Length != v2.Length) throw new ArgumentException("Vector sizes differ.");
            double sum = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                double d = (double)v1[i] - v2[i];
                sum += d * d;
            }
            return (float)Math.Sqrt(sum);
        }
    }
}
