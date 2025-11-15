using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace ImageSimilarityApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<ImagePairResult> ImagePairResults { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "similarity.db");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ImagePairResult>()
                .HasIndex(p => new { p.Image1Name, p.Image2Name })
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
