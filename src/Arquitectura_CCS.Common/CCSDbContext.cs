using Microsoft.EntityFrameworkCore;
using Arquitectura_CCS.Common.Models;

namespace Arquitectura_CCS.Common
{
    public class CCSDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("DefaultConnection", options =>
                {
                    options.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                    options.CommandTimeout(30);
                });
            }
        }
        public CCSDbContext(DbContextOptions<CCSDbContext> options) : base(options) { }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleTelemetry> VehicleTelemetries { get; set; }
        public DbSet<EmergencySignal> EmergencySignals { get; set; }
        public DbSet<Rule> Rules { get; set; }
        public DbSet<RuleAction> RuleActions { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // Configuración de EmergencySignal
            modelBuilder.Entity<EmergencySignal>(entity =>
            {
                entity.HasKey(es => es.EmergencyId);
                entity.HasIndex(es => es.VehicleId);
                entity.HasIndex(es => es.CreatedAt);
                entity.HasIndex(es => es.IsResolved);

                entity.Property(es => es.EmergencyType).HasConversion<int>();

                entity.HasOne(es => es.Vehicle)
                      .WithMany()
                      .HasForeignKey(es => es.VehicleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Rule
            modelBuilder.Entity<Rule>(entity =>
            {
                entity.HasKey(r => r.RuleId);
                entity.HasIndex(r => r.VehicleId);
                entity.HasIndex(r => r.IsActive);
                entity.HasIndex(r => r.RuleType);

                entity.Property(r => r.RuleType).HasConversion<int>();

                entity.HasOne(r => r.Vehicle)
                      .WithMany(v => v.Rules)
                      .HasForeignKey(r => r.VehicleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de RuleAction
            modelBuilder.Entity<RuleAction>(entity =>
            {
                entity.HasKey(ra => ra.ActionId);
                entity.HasIndex(ra => ra.RuleId);

                entity.Property(ra => ra.ActionType).HasConversion<int>();

                entity.HasOne(ra => ra.Rule)
                      .WithMany(r => r.Actions)
                      .HasForeignKey(ra => ra.RuleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Notification
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.NotificationId);
                entity.HasIndex(n => n.VehicleId);
                entity.HasIndex(n => n.CreatedAt);
                entity.HasIndex(n => n.IsSent);
            });

            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasKey(v => v.VehicleId);
                entity.Property(v => v.VehicleId).HasMaxLength(50);
                entity.Property(v => v.LicensePlate).IsRequired().HasMaxLength(20);
                entity.Property(v => v.Model).HasMaxLength(50);
                entity.Property(v => v.Brand).HasMaxLength(50);
            });

            // Configuración para VehicleTelemetry
            modelBuilder.Entity<VehicleTelemetry>(entity =>
            {
                entity.HasKey(t => t.TelemetryId);

                // Relación con Vehicle
                entity.HasOne(t => t.Vehicle)
                      .WithMany(v => v.TelemetryHistory)
                      .HasForeignKey(t => t.VehicleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}