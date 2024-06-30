// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO;
using Microsoft.EntityFrameworkCore;
using VirtualRadar.Database.EntityFramework.StandingData.Entities;

namespace VirtualRadar.Database.EntityFramework.StandingData
{
    class StandingDataContext(IFileSystem _FileSystem, IWorkingFolder _WorkingFolder) : DbContext
    {
        public DbSet<AircraftTypeNoEnumsView> AircraftTypeNoEnumsViews { get; set; }

        public DbSet<Airport> Airports { get; set; }

        public DbSet<CodeBlock> CodeBlocks { get; set; }

        public DbSet<Country> Countries { get; set; }

        public DbSet<DatabaseVersion> DatabaseVersions { get; set; }

        public DbSet<Operator> Operators { get; set; }

        public string FileName => _FileSystem.Combine(
            _WorkingFolder.Folder,
            "StandingData.sqb"
        );

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            var fileName = FileName;

            if(!_FileSystem.FileExists(fileName)) {
                throw new FileNotFoundException($"{fileName} does not exist");
            }

            optionsBuilder.UseSqlite($"Data Source={fileName}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AircraftTypeNoEnumsView>()
                .ToTable("AircraftTypeNoEnumsView")
                .HasKey(
                    nameof(AircraftTypeNoEnumsView.AircraftTypeId),
                    nameof(AircraftTypeNoEnumsView.ModelId),
                    nameof(AircraftTypeNoEnumsView.ManufacturerId)
                );

            modelBuilder.Entity<Airport>()
                .ToTable("Airport");

            modelBuilder.Entity<CodeBlock>()
                .ToTable("CodeBlock")
                .HasNoKey();

            var country = modelBuilder.Entity<Country>()
                .ToTable("Country");
            country
                .HasMany<Airport>()
                .WithOne(airport => airport.Country)
                .HasForeignKey(airport => airport.CountryId)
                .IsRequired(true);
            country
                .HasMany<CodeBlock>()
                .WithOne(codeBlock => codeBlock.Country)
                .HasForeignKey(codeBlock => codeBlock.CountryId)
                .IsRequired(true);

            modelBuilder.Entity<DatabaseVersion>()
                .ToTable("DatabaseVersion")
                .HasNoKey();

            modelBuilder.Entity<Operator>()
                .ToTable("Operator");
        }
    }
}
