// Copyright © 2023 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using VirtualRadar.Interface.KineticData;
using VirtualRadar.Interface.Options;

namespace VirtualRadar.Database.SQLite.KineticData
{
    class BaseStationContext : DbContext
    {
        private readonly string _FileName;
        private readonly bool _WriteEnabled;
        private readonly BaseStationDatabaseOptions _BaseStationDatabaseOptions;

        public DbSet<KineticAircraft> Aircraft { get; set; }

        public DbSet<KineticDBHistory> DBHistories { get; set; }

        public DbSet<KineticDBInfo> DBInfos { get; set; }

        public DbSet<KineticFlight> Flights { get; set; }

        public DbSet<KineticLocation> Locations { get; set; }

        public DbSet<KineticSession> Sessions { get; set; }

        public DbSet<KineticSystemEvent> SystemEvents { get; set; }

        public event EventHandler<EntityStateChangedEventArgs> EntityStateChanged;

        protected virtual void OnEntityStateChaned(EntityStateChangedEventArgs args) => EntityStateChanged?.Invoke(this, args);

        public BaseStationContext(
            string fileName,
            bool writeEnabled,
            BaseStationDatabaseOptions baseStationDatabaseOptions
        ) : base()
        {
            _FileName = fileName;
            _WriteEnabled = writeEnabled;
            _BaseStationDatabaseOptions = baseStationDatabaseOptions;

            ChangeTracker.StateChanged += (_, args) => OnEntityStateChaned(args);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            var connectionStringBuilder = new SqliteConnectionStringBuilder() {
                DataSource = _FileName,
                Pooling = false,
                Mode = _WriteEnabled ? SqliteOpenMode.ReadWrite : SqliteOpenMode.ReadOnly,
            };

            optionsBuilder.UseSqlite(connectionStringBuilder.ConnectionString);

            if(_BaseStationDatabaseOptions.ShowDatabaseDiagnosticsInDebugConsole) {
                optionsBuilder.EnableSensitiveDataLogging(true);
                optionsBuilder.LogTo(message => System.Diagnostics.Debug.WriteLine(message));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<KineticAircraft>(entity => {
                entity.ToTable("Aircraft");
                entity.HasKey(r => r.AircraftID);
                entity.Property(r => r.FirstCreated).HasConversion(new ISO8601Converter());
                entity.Property(r => r.LastModified).HasConversion(new ISO8601Converter());

                entity.HasMany<KineticFlight>()
                      .WithOne(flight => flight.Aircraft)
                      .HasForeignKey(nameof(KineticFlight.AircraftID))
                      .IsRequired(true);
            });

            modelBuilder.Entity<KineticDBHistory>(entity => {
                entity.ToTable("DBHistory");
                entity.HasKey(r => r.DBHistoryID);
                entity.Property(r => r.TimeStamp).HasConversion(new ISO8601Converter());
            });

            modelBuilder.Entity<KineticDBInfo>(entity => {
                entity.ToTable("DBInfo");
                entity.HasNoKey();
            });

            modelBuilder.Entity<KineticFlight>(entity => {
                entity.ToTable("Flights");
                entity.HasKey(r => r.FlightID);
                entity.Property(r => r.StartTime).HasConversion(new ISO8601Converter());
                entity.Property(r => r.EndTime).HasConversion(new ISO8601Converter());
            });

            modelBuilder.Entity<KineticLocation>(entity => {
                entity.ToTable("Locations");
                entity.HasKey(r => r.LocationID);
            });

            modelBuilder.Entity<KineticSession>(entity => {
                entity.ToTable("Sessions");
                entity.HasKey(r => r.SessionID);
                entity.Property(r => r.StartTime).HasConversion(new ISO8601Converter());
                entity.Property(r => r.EndTime).HasConversion(new ISO8601Converter());
            });

            modelBuilder.Entity<KineticSystemEvent>(entity => {
                entity.ToTable("SystemEvents");
                entity.HasKey(r => r.SystemEventsID);
                entity.Property(r => r.TimeStamp).HasConversion(new ISO8601Converter());
            });
        }
    }
}
