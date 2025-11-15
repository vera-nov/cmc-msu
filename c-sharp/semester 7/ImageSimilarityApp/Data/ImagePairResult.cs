using System;

namespace ImageSimilarityApp.Data
{
    public class ImagePairResult
    {
        public int Id { get; set; }

        public string Image1Name { get; set; } = string.Empty;
        public string Image2Name { get; set; } = string.Empty;

        public float Similarity { get; set; }
        public float Distance { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
