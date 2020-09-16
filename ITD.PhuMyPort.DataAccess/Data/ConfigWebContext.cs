using ITD.PhuMyPort.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITD.PhuMyPort.DataAccess.Data
{
    public class ConfigWebContext : DbContext
    {
        public ConfigWebContext(DbContextOptions<ConfigWebContext> options) : base(options)
        {
        }

        public DbSet<Workplace> Workplaces { get; set; }
        public DbSet<Camera> Cameras { get; set; }
        public DbSet<ANPRLog> ANPRLogs { get; set; }
        public DbSet<PLC> PLCs { get; set; }
        public DbSet<Transection> Transections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Workplace>().ToTable("Workplace");
            modelBuilder.Entity<Camera>().ToTable("Camera");
            modelBuilder.Entity<ANPRLog>().ToTable("ANPRLog");
            modelBuilder.Entity<PLC>().ToTable("PLC");
            modelBuilder.Entity<Transection>().ToTable("Transection");
        }
    }
}
