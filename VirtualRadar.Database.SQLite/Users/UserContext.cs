using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VirtualRadar.Interface.Options;

namespace VirtualRadar.Database.SQLite.Users
{
    class UserContext : DbContext
    {
        private EnvironmentOptions _EnvironmentOptions;
        private UserManagerOptions _UserManagerOptions;

        public DbSet<User> Users { get; set; }

        public UserContext(IOptions<EnvironmentOptions> environmentOptions, IOptions<UserManagerOptions> userManagerOptions) : base()
        {
            _EnvironmentOptions = environmentOptions.Value;
            _UserManagerOptions = userManagerOptions.Value;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            var dataSource = Path.Combine(
                _EnvironmentOptions.WorkingFolder,
                "Users.sqb"
            );
            optionsBuilder.UseSqlite($"Data Source = {dataSource}");

            if(_UserManagerOptions.ShowDatabaseDiagnosticsInDebugConsole) {
                optionsBuilder.LogTo(message => System.Diagnostics.Debug.WriteLine(message));
            }
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
