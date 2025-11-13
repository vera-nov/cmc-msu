using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("ClassLibrary1.Tests")]

namespace ClassLibrary1;

public static class ArcFaceEmbedder
{
    private const string DefaultInputName = "data";
    private const string DefaultOutputName = "fc1";
    private const int ModelW = 112;
    private const int ModelH = 112;

    private static readonly object _initLock = new();
    private static string? _modelPath;
    private static InferenceSession? _session;

    private static readonly SemaphoreSlim _inferGate = new(1, 1);

    public static void Initialize(string modelPath)
    {
        if (string.IsNullOrWhiteSpace(modelPath))
            throw new ArgumentException("Model path must be a non-empty string.", nameof(modelPath));
        if (!File.Exists(modelPath))
            throw new FileNotFoundException("ONNX model file not found.", modelPath);
        _inferGate.Wait();
        try
        {
            lock (_initLock)
            {
                if (_session == null || !string.Equals(_modelPath, modelPath, StringComparison.Ordinal))
                {
                    var newSession = new InferenceSession(modelPath);
                    var old = _session;
                    _session = newSession;
                    _modelPath = modelPath;
                    old?.Dispose();
                }
            }
        }
        finally
        {
            _inferGate.Release();
        }
    }

    public static void DisposeSession()
    {
        _inferGate.Wait();
        try
        {
            lock (_initLock)
            {
                _session?.Dispose();
                _session = null;
                _modelPath = null;
            }
        }
        finally
        {
            _inferGate.Release();
        }
    }

    internal static InferenceSession? PeekSession() => _session;

    private static InferenceSession GetSessionOrThrow()
    {
        var s = _session;
        if (s is null)
            throw new InvalidOperationException("ArcFaceEmbedder is not initialized. Call Initialize(modelPath) first.");
        return s;
    }

    public static async Task<float[]> GetEmbeddingFromFileAsync(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            throw new ArgumentException("Image path must be provided.", nameof(imagePath));
        if (!File.Exists(imagePath))
            throw new FileNotFoundException("Could not find image file.", imagePath);

        await _inferGate.WaitAsync();
        try
        {
            using var image = Image.Load<Rgb24>(imagePath);
            using var resizedImage = image.Clone(ctx =>
            {
                ctx.Resize(new ResizeOptions
                {
                    Size = new Size(ModelW, ModelH),
                    Mode = ResizeMode.Stretch
                });
            });

            return await Task.Run(() =>
            {
                var session = GetSessionOrThrow();

                var tensor = CreateCHWTensor(resizedImage);
                using var results = session.Run(new[]
                {
                NamedOnnxValue.CreateFromTensor(DefaultInputName, tensor)
            });

                var embedding = ExtractOutputVector(results, DefaultOutputName);
                return NormalizeL2InPlace(embedding);
            });
        }
        finally
        {
            _inferGate.Release();
        }
    }
    public static async Task<float[]> GetEmbeddingAsync(byte[] imageBytes)
    {
        if (imageBytes == null || imageBytes.Length == 0)
            throw new ArgumentException("Image bytes must be provided.", nameof(imageBytes));

        await _inferGate.WaitAsync();
        try
        {
            using var image = Image.Load<Rgb24>(imageBytes);
            using var resizedImage = image.Clone(ctx =>
            {
                ctx.Resize(new ResizeOptions
                {
                    Size = new Size(ModelW, ModelH),
                    Mode = ResizeMode.Stretch
                });
            });

            return await Task.Run(() =>
            {
                var session = GetSessionOrThrow();
                var tensor = CreateCHWTensor(resizedImage);
                using var results = session.Run(new[]
                {
                NamedOnnxValue.CreateFromTensor(DefaultInputName, tensor)
            });
                var embedding = ExtractOutputVector(results, DefaultOutputName);
                return NormalizeL2InPlace(embedding);
            });
        }
        finally
        {
            _inferGate.Release();
        }
    }



    public static float Similarity(float[] a, float[] b)
    {
        if (a == null || b == null) throw new ArgumentNullException(a == null ? nameof(a) : nameof(b));
        if (a.Length != b.Length) throw new ArgumentException("Vectors must have the same length.");

        double dot = 0.0;
        for (int i = 0; i < a.Length; i++)
            dot += a[i] * b[i];
        return (float)dot;
    }

    public static float CosineSimilarity(float[] a, float[] b)
    {
        if (a == null || b == null)
            throw new ArgumentNullException(a == null ? nameof(a) : nameof(b));
        if (a.Length != b.Length)
            throw new ArgumentException("Vectors must have the same length.");

        double dot = 0.0, na = 0.0, nb = 0.0;
        for (int i = 0; i < a.Length; i++)
        {
            double ai = a[i], bi = b[i];
            dot += ai * bi;
            na += ai * ai;
            nb += bi * bi;
        }
        if (na <= 0.0 || nb <= 0.0) return 0f;
        return (float)(dot / Math.Sqrt(na * nb));
    }

    private static DenseTensor<float> CreateCHWTensor(Image<Rgb24> image)
    {
        int w = image.Width, h = image.Height;
        var t = new DenseTensor<float>(new[] { 1, 3, h, w });

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < h; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < w; x++)
                {
                    var px = row[x];
                    t[0, 0, y, x] = px.R;
                    t[0, 1, y, x] = px.G;
                    t[0, 2, y, x] = px.B;
                }
            }
        });

        return t;
    }

    private static float[] ExtractOutputVector(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> outputs, string name)
    {
        foreach (var item in outputs)
        {
            if (item != null && string.Equals(item.Name, name, StringComparison.Ordinal))
                return item.AsEnumerable<float>().ToArray();
        }
        throw new InvalidOperationException($"Output tensor '{name}' was not found.");
    }

    private static float[] NormalizeL2InPlace(float[] v)
    {
        double sumSq = 0.0;
        for (int i = 0; i < v.Length; i++) sumSq += v[i] * v[i];
        var len = Math.Sqrt(sumSq);
        if (len <= 0.0) return v;

        float inv = (float)(1.0 / len);
        for (int i = 0; i < v.Length; i++) v[i] *= inv;
        return v;
    }
}
