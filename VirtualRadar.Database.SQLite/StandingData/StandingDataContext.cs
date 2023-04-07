// Copyright © 2023 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Microsoft.EntityFrameworkCore;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Options;

namespace VirtualRadar.Database.SQLite.StandingData
{
    class StandingDataContext : DbContext
    {
        private readonly IFileSystem _FileSystem;
        private readonly EnvironmentOptions _EnvironmentOptions;
        private readonly StandingDataManagerOptions _StandingDataManagerOptions;

        public DbSet<AircraftTypeModel> AircraftTypes { get; set; }

        public DbSet<AircraftTypeModelModel> AircraftTypeModels { get; set; }

        public DbSet<AirportModel> Airports { get; set; }

        public DbSet<CodeBlockModel> CodeBlocks { get; set; }

        public DbSet<CountryModel> Countrys { get; set; }

        public DbSet<DatabaseVersionModel> DatabaseVersions { get; set; }

        public DbSet<EnginePlacementModel> EnginePlacements { get; set; }

        public DbSet<EngineTypeModel> EngineTypes { get; set; }

        public DbSet<ManufacturerModel> Manufacturers { get; set; }

        public DbSet<ModelModel> Models { get; set; }

        public DbSet<OperatorModel> Operators { get; set; }

        public DbSet<RouteModel> Routes { get; set; }

        public DbSet<RouteStopModel> RouteStops { get; set; }

        public DbSet<SpeciesModel> Species { get; set; }

        public DbSet<WakeTurbulenceModel> WakeTurbulences { get; set; }

        public StandingDataContext(
            IFileSystem fileSystem,
            EnvironmentOptions environmentOptions,
            StandingDataManagerOptions standingDataManagerOptions
        ) : base()
        {
            _FileSystem = fileSystem;
            _EnvironmentOptions = environmentOptions;
            _StandingDataManagerOptions = standingDataManagerOptions;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            var dataSource = _FileSystem.Combine(
                _EnvironmentOptions.WorkingFolder,
                "StandingData.sqb"
            );
            optionsBuilder
                .UseSqlite($"data source={dataSource}; mode=readonly;");

            if(_StandingDataManagerOptions.ShowDatabaseDiagnosticsInDebugConsole) {
                optionsBuilder.LogTo(message => System.Diagnostics.Debug.WriteLine(message));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AircraftTypeModel>(entity => {
                entity.ToTable("AircraftType");

                entity.HasOne(aircraftType => aircraftType.WakeTurbulence)
                    .WithMany()
                    .HasForeignKey(aircraftType => aircraftType.WakeTurbulenceId)
                    .IsRequired();

                entity.HasOne(aircraftType => aircraftType.Species)
                    .WithMany()
                    .HasForeignKey(model => model.SpeciesId)
                    .IsRequired();

                entity.HasOne(aircraftType => aircraftType.EngineType)
                    .WithMany()
                    .HasForeignKey(aircraftType => aircraftType.EngineTypeId)
                    .IsRequired();

                entity.HasOne(aircraftType => aircraftType.EnginePlacement)
                    .WithMany()
                    .HasForeignKey(aircraftType => aircraftType.EnginePlacementId)
                    .IsRequired();
            });

            modelBuilder.Entity<AircraftTypeModelModel>(entity => {
                entity.ToTable("AircraftTypeModel");

                entity.HasOne(aircraftTypeModel => aircraftTypeModel.AircraftType)
                    .WithMany(aircraftType => aircraftType.AircraftTypeModels)
                    .HasForeignKey(aircraftTypeModel => aircraftTypeModel.AircraftTypeId)
                    .IsRequired();

                entity.HasOne(aircraftTypeModel => aircraftTypeModel.Model)
                    .WithMany()
                    .HasForeignKey(aircraftTypeModel => aircraftTypeModel.ModelId)
                    .IsRequired();
            });

            modelBuilder.Entity<AirportModel>(entity => {
                entity.ToTable("Airport")
                    .HasOne(model => model.Country)
                    .WithMany()
                    .IsRequired()
                    .HasForeignKey(model => model.CountryId);
            });

            modelBuilder.Entity<CodeBlockModel>(entity => {
                entity.ToTable("CodeBlock")
                    .HasOne(codeBlock => codeBlock.Country)
                    .WithMany()
                    .HasForeignKey(codeBlock => codeBlock.CountryId);
            });

            modelBuilder.Entity<CountryModel>(entity => {
                entity.ToTable("Country");
            });

            modelBuilder.Entity<DatabaseVersionModel>(entity => {
                entity.ToTable("DatabaseVersion");
                entity.Property(r => r.Created).HasConversion(new ISO8601Converter());
                entity.HasNoKey();
            });

            modelBuilder.Entity<EnginePlacementModel>(entity => {
                entity.ToTable("EnginePlacement");
            });

            modelBuilder.Entity<EngineTypeModel>(entity => {
                entity.ToTable("EngineType");
            });

            modelBuilder.Entity<ManufacturerModel>(entity => {
                entity.ToTable("Manufacturer");
            });

            modelBuilder.Entity<ModelModel>(entity => {
                entity.ToTable("Model")
                    .HasOne(model => model.Manufacturer)
                    .WithMany()
                    .HasForeignKey(model => model.ManufacturerId);
            });

            modelBuilder.Entity<OperatorModel>(entity => {
                entity.ToTable("Operator");
            });

            modelBuilder.Entity<RouteModel>(entity => {
                entity.ToTable("Route");

                entity.HasOne(route => route.Operator)
                    .WithMany()
                    .HasForeignKey(route => route.OperatorId)
                    .IsRequired();

                entity.HasOne(route => route.FromAirport)
                    .WithMany()
                    .HasForeignKey(route => route.FromAirportId)
                    .IsRequired();

                entity.HasOne(route => route.ToAirport)
                    .WithMany()
                    .HasForeignKey(route => route.ToAirportId)
                    .IsRequired();
            });

            modelBuilder.Entity<RouteStopModel>(entity => {
                entity.ToTable("RouteStop");

                entity.HasOne(routeStop => routeStop.Route)
                    .WithMany(route => route.RouteStops)
                    .HasForeignKey(routeStop => routeStop.RouteId)
                    .IsRequired();

                entity.HasOne(routeStop => routeStop.Airport)
                    .WithMany()
                    .HasForeignKey(routeStop => routeStop.AirportId)
                    .IsRequired();
            });

            modelBuilder.Entity<SpeciesModel>(entity => {
                entity.ToTable("Species");
            });

            modelBuilder.Entity<WakeTurbulenceModel>(entity => {
                entity.ToTable("WakeTurbulence");
            });
        }
    }
}
