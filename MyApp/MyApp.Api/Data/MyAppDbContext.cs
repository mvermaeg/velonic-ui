using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data.Entities;

namespace MyApp.Api.Data;

public partial class MyAppDbContext : DbContext
{
    public MyAppDbContext()
    {
    }

    public MyAppDbContext(DbContextOptions<MyAppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Affiliate> Affiliates { get; set; }

    public virtual DbSet<AffiliateClick> AffiliateClicks { get; set; }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<ClientBiddingSetting> ClientBiddingSettings { get; set; }

    public virtual DbSet<LandingPage> LandingPages { get; set; }

    public virtual DbSet<Lead> Leads { get; set; }

    public virtual DbSet<LeadAttempt> LeadAttempts { get; set; }

    public virtual DbSet<LeadBiddingResult> LeadBiddingResults { get; set; }

    public virtual DbSet<LeadCampaign> LeadCampaigns { get; set; }

    public virtual DbSet<LeadDelivery> LeadDeliveries { get; set; }

    public virtual DbSet<LeadForm> LeadForms { get; set; }

    public virtual DbSet<LeadRawPayload> LeadRawPayloads { get; set; }

    public virtual DbSet<LeadRejection> LeadRejections { get; set; }

    public virtual DbSet<LeadReturn> LeadReturns { get; set; }

    public virtual DbSet<LeadSource> LeadSources { get; set; }

    public virtual DbSet<LeadStatusHistory> LeadStatusHistories { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<Site> Sites { get; set; }

    public virtual DbSet<WebsiteTemplate> WebsiteTemplates { get; set; }

    public virtual DbSet<WebsiteTheme> WebsiteThemes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=103.133.214.224,1436;Database=VelonicDB;User Id=VelonicDBUser;Password=Vapt@1247;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("VelonicDBUser");

        modelBuilder.Entity<Affiliate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Affiliat__3214EC074B47458B");

            entity.Property(e => e.AffiliateName).HasMaxLength(300);
            entity.Property(e => e.ApiKey).HasMaxLength(200);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<AffiliateClick>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Affiliat__3214EC078226C482");

            entity.Property(e => e.AffiliateName).HasMaxLength(300);
            entity.Property(e => e.ApiKey).HasMaxLength(200);
            entity.Property(e => e.CampaignName).HasMaxLength(250);
            entity.Property(e => e.ClickUuid).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IpAddress).HasMaxLength(100);
            entity.Property(e => e.PageName).HasMaxLength(250);
            entity.Property(e => e.SourceName).HasMaxLength(250);
            entity.Property(e => e.SubId).HasMaxLength(250);
            entity.Property(e => e.Url).HasMaxLength(1000);
            entity.Property(e => e.UserAgent).HasMaxLength(1000);
        });

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Clients__3214EC0725EEBFCF");

            entity.Property(e => e.AcceptsWebLeads).HasDefaultValue(true);
            entity.Property(e => e.AccountManagerUserId).HasMaxLength(450);
            entity.Property(e => e.AccountStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Active");
            entity.Property(e => e.ClientName).HasMaxLength(250);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsBuyer).HasDefaultValue(true);
            entity.Property(e => e.ReturnAgreement).HasMaxLength(100);
            entity.Property(e => e.SubAccountManagerUserId).HasMaxLength(450);
            entity.Property(e => e.Tier).HasMaxLength(50);
            entity.Property(e => e.Timezone).HasMaxLength(100);
        });

        modelBuilder.Entity<ClientBiddingSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ClientBi__3214EC07FCC29501");

            entity.Property(e => e.BidAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LeadType).HasMaxLength(100);
            entity.Property(e => e.Postcode).HasMaxLength(50);
            entity.Property(e => e.State).HasMaxLength(100);
        });

        modelBuilder.Entity<LandingPage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LandingP__3214EC072F79AA5F");

            entity.Property(e => e.AffiliateName).HasMaxLength(300);
            entity.Property(e => e.CampaignName).HasMaxLength(300);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CtaText).HasMaxLength(100);
            entity.Property(e => e.HeroSubtitle).HasMaxLength(500);
            entity.Property(e => e.HeroTitle).HasMaxLength(300);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PageName).HasMaxLength(200);
            entity.Property(e => e.PageSlug).HasMaxLength(150);
            entity.Property(e => e.PageTitle).HasMaxLength(250);
        });

        modelBuilder.Entity<Lead>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Leads__3214EC07BCE2D0CC");

            entity.HasIndex(e => e.AffiliateName, "IX_Leads_AffiliateName");

            entity.HasIndex(e => e.AssignedToUserId, "IX_Leads_AssignedToUserId");

            entity.HasIndex(e => e.CampaignName, "IX_Leads_CampaignName");

            entity.HasIndex(e => e.CreatedAt, "IX_Leads_CreatedAt");

            entity.HasIndex(e => e.Email, "IX_Leads_Email");

            entity.HasIndex(e => e.Phone, "IX_Leads_Phone");

            entity.HasIndex(e => e.LeadStatus, "IX_Leads_Status");

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.AffiliateName).HasMaxLength(300);
            entity.Property(e => e.AffiliateSubId).HasMaxLength(250);
            entity.Property(e => e.CampaignName).HasMaxLength(300);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreatedByUserId).HasMaxLength(450);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.IpAddress).HasMaxLength(100);
            entity.Property(e => e.LeadStatus)
                .HasMaxLength(50)
                .HasDefaultValue("New");
            entity.Property(e => e.PageName).HasMaxLength(300);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Postcode).HasMaxLength(50);
            entity.Property(e => e.Profit).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ReceivedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Sales).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Source).HasMaxLength(100);
            entity.Property(e => e.SourceReference).HasMaxLength(200);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.UpdatedByUserId).HasMaxLength(450);
        });

        modelBuilder.Entity<LeadAttempt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LeadAtte__3214EC07A783793F");

            entity.Property(e => e.AttemptType).HasMaxLength(50);
            entity.Property(e => e.AttemptedByUserId).HasMaxLength(450);
            entity.Property(e => e.AttemptedOn).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Result).HasMaxLength(100);
        });

        modelBuilder.Entity<LeadBiddingResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LeadBidd__3214EC07988138AE");

            entity.Property(e => e.BidAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.MatchReason).HasMaxLength(500);
        });

        modelBuilder.Entity<LeadCampaign>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LeadCamp__3214EC0711452C94");

            entity.Property(e => e.CampaignName).HasMaxLength(300);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ExternalCampaignId).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<LeadDelivery>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LeadDeli__3214EC075BC218BB");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DeliveryStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.DeliveryType)
                .HasMaxLength(50)
                .HasDefaultValue("API");
        });

        modelBuilder.Entity<LeadForm>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LeadForm__3214EC070EE20173");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ExternalFormId).HasMaxLength(200);
            entity.Property(e => e.FormName).HasMaxLength(300);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PageName).HasMaxLength(300);
        });

        modelBuilder.Entity<LeadRawPayload>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LeadRawP__3214EC07D69E5038");

            entity.Property(e => e.ExternalLeadId).HasMaxLength(200);
            entity.Property(e => e.ReceivedOn).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.SourceName).HasMaxLength(100);
        });

        modelBuilder.Entity<LeadRejection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LeadReje__3214EC0755567C2A");

            entity.Property(e => e.CreatedByUserId).HasMaxLength(450);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RejectionReason).HasMaxLength(300);
            entity.Property(e => e.RejectionSource).HasMaxLength(100);
        });

        modelBuilder.Entity<LeadReturn>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LeadRetu__3214EC0741F0F0BB");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ReturnReason).HasMaxLength(300);
            entity.Property(e => e.ReturnStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.ReviewedByUserId).HasMaxLength(450);
        });

        modelBuilder.Entity<LeadSource>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LeadSour__3214EC077190FA50");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SourceName).HasMaxLength(100);
            entity.Property(e => e.SourceType).HasMaxLength(50);
        });

        modelBuilder.Entity<LeadStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LeadStat__3214EC072C518B46");

            entity.ToTable("LeadStatusHistory");

            entity.Property(e => e.ChangedByUserId).HasMaxLength(450);
            entity.Property(e => e.ChangedOn).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.NewStatus).HasMaxLength(50);
            entity.Property(e => e.OldStatus).HasMaxLength(50);
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Organiza__3214EC0781DFB462");

            entity.Property(e => e.ContactEmail).HasMaxLength(200);
            entity.Property(e => e.ContactPhone).HasMaxLength(50);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.OrganizationName).HasMaxLength(200);
        });

        modelBuilder.Entity<Site>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Sites__3214EC0771223939");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DomainName).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SiteName).HasMaxLength(200);
            entity.Property(e => e.Slug).HasMaxLength(150);
            entity.Property(e => e.ThemeKey)
                .HasMaxLength(50)
                .HasDefaultValue("theme-1");

            entity.HasOne(d => d.WebsiteTemplate).WithMany(p => p.Sites)
                .HasForeignKey(d => d.WebsiteTemplateId)
                .HasConstraintName("FK_Sites_WebsiteTemplates");

            entity.HasOne(d => d.WebsiteTheme).WithMany(p => p.Sites)
                .HasForeignKey(d => d.WebsiteThemeId)
                .HasConstraintName("FK_Sites_WebsiteThemes");
        });

        modelBuilder.Entity<WebsiteTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__WebsiteT__3214EC070A433417");

            entity.Property(e => e.BuildFolder).HasMaxLength(500);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PreviewImage).HasMaxLength(500);
            entity.Property(e => e.SourceFolder).HasMaxLength(500);
            entity.Property(e => e.TemplateKey).HasMaxLength(100);
            entity.Property(e => e.TemplateName).HasMaxLength(200);
            entity.Property(e => e.Version).HasMaxLength(50);
        });

        modelBuilder.Entity<WebsiteTheme>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__WebsiteT__3214EC073D93BD14");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FontFamily).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.PrimaryColor).HasMaxLength(20);
            entity.Property(e => e.SecondaryColor).HasMaxLength(20);
            entity.Property(e => e.ThemeKey).HasMaxLength(100);
            entity.Property(e => e.ThemeName).HasMaxLength(200);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
