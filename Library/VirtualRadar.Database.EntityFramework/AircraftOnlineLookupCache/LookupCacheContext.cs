﻿// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Microsoft.EntityFrameworkCore;
using VirtualRadar.Database.EntityFramework.AircraftOnlineLookupCache.Entities;

namespace VirtualRadar.Database.EntityFramework.AircraftOnlineLookupCache
{
    class LookupCacheContext(
        IFileSystem _FileSystem,
        IWorkingFolder _WorkingFolder
    ) : DbContext
    {
        public DbSet<AircraftDetail> AircraftDetails { get; set; }

        public DbSet<DatabaseVersion> DatabaseVersions { get; set; }

        public string FileName => _FileSystem.Combine(
            _WorkingFolder.Folder,
            "AircraftOnlineLookupCache.sqb"
        );

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            var fileName = FileName;
            optionsBuilder.UseSqlite($"Data Source={fileName}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var aircraftDetail = modelBuilder.Entity<AircraftDetail>()
                .ToTable("AircraftDetail");
            aircraftDetail
                .Property(entity => entity.Icao)
                .UseCollation("NOCASE");
            aircraftDetail
                .HasIndex(entity => entity.Icao)
                .IsUnique();
            aircraftDetail
                .Property(entity => entity.Registration)
                .UseCollation("NOCASE");
            aircraftDetail
                .Property(entity => entity.ModelIcao)
                .UseCollation("NOCASE");
            aircraftDetail
                .Property(entity => entity.OperatorIcao)
                .UseCollation("NOCASE");

            modelBuilder.Entity<DatabaseVersion>()
                .ToTable("DatabaseVersion");
        }

        public bool EnsureCreated()
        {
            var result = Database.EnsureCreated();
            if(result) {
                try {
                    Database.ExecuteSqlRaw("PRAGMA journal_mode = 'persist'");
                } catch {
                    _FileSystem.DeleteFile(FileName);
                    throw;
                }
            }

            return result;
        }
    }
}
