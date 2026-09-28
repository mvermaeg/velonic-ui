using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data.Entities;

namespace MyApp.Api.Data;

public partial class MyAppDbContext
{
    public DbSet<ThumbtackCoverageService> ThumbtackCoverageServices { get; set; }
    public DbSet<ThumbtackZipCoverage> ThumbtackZipCoverages { get; set; }
    public DbSet<ThumbtackCoverageImportBatch> ThumbtackCoverageImportBatches { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ThumbtackCoverageImportBatch>(e => {
            e.ToTable("ThumbtackCoverageImportBatches", "VelonicDBUser"); e.HasKey(x => x.Id);
            e.Property(x => x.Fingerprint).HasColumnType("char(64)"); e.HasIndex(x => x.Fingerprint).IsUnique();
            e.Property(x => x.SourceName).HasMaxLength(260);
        });
        modelBuilder.Entity<LeadRoutingRun>(e => {
            e.Property(x => x.RoutingMode).HasMaxLength(16).IsUnicode(false).HasDefaultValue("Legacy");
            e.Property(x => x.WinningBidAmount).HasColumnType("decimal(18,4)");
            e.Property(x => x.ThumbtackEligibility).HasMaxLength(1000);
            e.Property(x => x.ThumbtackSearchId).HasMaxLength(500);
            e.Property(x => x.ReconciledBy).HasMaxLength(450);
            e.Property(x => x.ReconciliationReference).HasMaxLength(100);
            e.Property(x => x.ReconciliationOutcome).HasMaxLength(20);
            e.HasIndex(x => x.WinnerAttemptId).IsUnique().HasFilter("[WinnerAttemptId] IS NOT NULL");
        });
        modelBuilder.Entity<LeadRoutingRule>().Property(x => x.IsActive).HasDefaultValue(false);
        // SQL datetime2 is UTC by convention. Preserve that kind in API JSON so browsers
        // do not interpret persisted deadlines as their own local time zone.
        var utc = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
            value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
        foreach (var type in new[] { typeof(LeadRoutingRun), typeof(LeadRoutingAttempt), typeof(LeadRoutingRule) })
            foreach (var property in modelBuilder.Entity(type).Metadata.GetProperties()
                .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
                property.SetValueConverter(utc);
        modelBuilder.Entity<LeadRoutingAttempt>(e => {
            e.Property(x => x.ExternalReferenceId).HasMaxLength(500).IsUnicode(true);
            e.Property(x => x.OfferedBidAmount).HasPrecision(18, 4);
            e.HasIndex(x => x.LeadRoutingRunId).IsUnique().HasFilter("[IsWinner] = 1");
        });
        modelBuilder.Entity<ThumbtackCoverageService>(entity =>
        {
            entity.ToTable("ThumbtackCoverageServices", "VelonicDBUser");
            entity.HasKey(x => x.ServiceCode);
            entity.Property(x => x.ServiceCode).HasMaxLength(50).IsUnicode(false);
            entity.Property(x => x.CategoryPk).HasMaxLength(100).IsUnicode(false);
        });
        modelBuilder.Entity<ThumbtackZipCoverage>(entity =>
        {
            entity.ToTable("ThumbtackZipCoverage", "VelonicDBUser");
            entity.HasKey(x => new { x.ServiceCode, x.ZipCode });
            entity.Property(x => x.ServiceCode).HasMaxLength(50).IsUnicode(false);
            entity.Property(x => x.ZipCode).HasColumnType("char(5)");
            entity.HasOne<ThumbtackCoverageService>().WithMany()
                .HasForeignKey(x => x.ServiceCode).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
