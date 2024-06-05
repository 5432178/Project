using Microsoft.EntityFrameworkCore;
using Project.Models;
using System.Collections.Generic;
namespace Project.Data

{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<AppFiles> Files { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Folder entity
            modelBuilder.Entity<Folder>()
                .Property(f => f.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Folder>()
           .HasMany(f => f.Files)
           .WithOne(f => f.ParentFolder)
           .HasForeignKey(f => f.ParentFolderId)
           .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Folder>()
                .HasMany(f => f.SubFolders)
                .WithOne(f => f.ParentFolder)
                .HasForeignKey(f => f.ParentFolderId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}

