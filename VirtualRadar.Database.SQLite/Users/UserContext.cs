using Microsoft.EntityFrameworkCore;
using VirtualRadar.Interface;

namespace VirtualRadar.Database.SQLite.Users
{
    class UserContext : DbContext
    {
        public string FullPath { get; }

        public DbSet<User> Users { get; set; }

        public UserContext(IFileSystemProvider fileSystemProvider) : base()
        {
            FullPath = Path.Combine(
                fileSystemProvider.ConfigurationFolder,
                "Users.sqb"
            );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity => {
                entity.ToTable("User");
                entity.Property(e => e.Enabled).HasColumnType("BIT");
                entity.Property(e => e.CreatedUtc).HasColumnType("DATETIME");
                entity.Property(e => e.UpdatedUtc).HasColumnType("DATETIME");

                entity.HasIndex(e => e.LoginName, "idx_User_LoginName").IsUnique();
            });
        }
    }
}
