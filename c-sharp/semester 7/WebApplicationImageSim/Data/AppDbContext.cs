using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace WebApplicationImageSim.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<ImagePairResult> ImagePairResults { get; set; } = null!;

        public static readonly string DbPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "similarity.db");

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"Data Source={DbPath}");
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
