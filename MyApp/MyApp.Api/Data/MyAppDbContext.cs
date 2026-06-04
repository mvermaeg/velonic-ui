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

    public virtual DbSet<LeadType> LeadTypes { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<PostalCodeLocation> PostalCodeLocations { get; set; }

    public virtual DbSet<Site> Sites { get; set; }

    public virtual DbSet<SiteDomain> SiteDomains { get; set; }

    public virtual DbSet<SiteForm> SiteForms { get; set; }

    public virtual DbSet<SiteMediaFile> SiteMediaFiles { get; set; }

    public virtual DbSet<SitePage> SitePages { get; set; }

    public virtual DbSet<SitePageSection> SitePageSections { get; set; }

    public virtual DbSet<SitePixel> SitePixels { get; set; }

    public virtual DbSet<SitePixelEvent> SitePixelEvents { get; set; }

    public virtual DbSet<SiteSetting> SiteSettings { get; set; }

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
            entity.Property(e => e.AddressValidationStatus).HasMaxLength(50);
            entity.Property(e => e.AffiliateName).HasMaxLength(300);
            entity.Property(e => e.AffiliateSubId).HasMaxLength(250);
            entity.Property(e => e.CampaignName).HasMaxLength(300);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreatedByUserId).HasMaxLength(450);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.EmailDomain).HasMaxLength(200);
            entity.Property(e => e.FingerprintHash).HasMaxLength(200);
            entity.Property(e => e.FraudLevel).HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.IpAddress).HasMaxLength(100);
            entity.Property(e => e.LeadQualityScore).HasDefaultValue(100);
            entity.Property(e => e.LeadStatus)
                .HasMaxLength(50)
                .HasDefaultValue("New");
            entity.Property(e => e.PageName).HasMaxLength(300);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.PostalAddressCheckReason).HasMaxLength(500);
            entity.Property(e => e.Postcode).HasMaxLength(50);
            entity.Property(e => e.Profit).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ReceivedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RiskCheckedOn).HasColumnType("datetime");
            entity.Property(e => e.Sales).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Source).HasMaxLength(100);
            entity.Property(e => e.SourceReference).HasMaxLength(200);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.UpdatedByUserId).HasMaxLength(450);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.VisitorCountry).HasMaxLength(100);

            entity.HasOne(d => d.LeadType).WithMany(p => p.Leads)
                .HasForeignKey(d => d.LeadTypeId)
                .HasConstraintName("FK_Leads_LeadTypes");
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

        modelBuilder.Entity<LeadType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LeadType__3214EC079887D8BB");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
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

        modelBuilder.Entity<PostalCodeLocation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PostalCo__3214EC073A080DD3");

            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Latitude).HasColumnType("decimal(10, 6)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(10, 6)");
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.StateCode).HasMaxLength(50);
            entity.Property(e => e.StateName).HasMaxLength(100);
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

        modelBuilder.Entity<SiteDomain>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SiteDoma__3214EC07246F3423");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DomainName).HasMaxLength(300);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Site).WithMany(p => p.SiteDomains)
                .HasForeignKey(d => d.SiteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SiteDomains_Sites");
        });

        modelBuilder.Entity<SiteForm>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SiteForm__3214EC07E14A0D28");

            entity.Property(e => e.CampaignName).HasMaxLength(200);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.FormKey).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SourceName).HasMaxLength(200);
            entity.Property(e => e.SubmitButtonText).HasMaxLength(100);
            entity.Property(e => e.SuccessMessage).HasMaxLength(500);

            entity.HasOne(d => d.Site).WithMany(p => p.SiteForms)
                .HasForeignKey(d => d.SiteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SiteForms_Sites");

            entity.HasOne(d => d.SitePage).WithMany(p => p.SiteForms)
                .HasForeignKey(d => d.SitePageId)
                .HasConstraintName("FK_SiteForms_SitePages");
        });

        modelBuilder.Entity<SiteMediaFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SiteMedi__3214EC07CEA9340E");

            entity.Property(e => e.AltText).HasMaxLength(300);
            entity.Property(e => e.Caption).HasMaxLength(500);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FileName).HasMaxLength(300);
            entity.Property(e => e.FileType).HasMaxLength(100);
            entity.Property(e => e.FileUrl).HasMaxLength(700);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MimeType).HasMaxLength(150);

            entity.HasOne(d => d.Site).WithMany(p => p.SiteMediaFiles)
                .HasForeignKey(d => d.SiteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SiteMediaFiles_Sites");
        });

        modelBuilder.Entity<SitePage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SitePage__3214EC07DBF335EA");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.HeroSubtitle).HasMaxLength(1000);
            entity.Property(e => e.HeroTitle).HasMaxLength(300);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MetaDescription).HasMaxLength(1000);
            entity.Property(e => e.MetaTitle).HasMaxLength(300);
            entity.Property(e => e.PageName).HasMaxLength(200);
            entity.Property(e => e.PageSlug).HasMaxLength(150);
            entity.Property(e => e.PageTitle).HasMaxLength(300);

            entity.HasOne(d => d.Site).WithMany(p => p.SitePages)
                .HasForeignKey(d => d.SiteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SitePages_Sites");
        });

        modelBuilder.Entity<SitePageSection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SitePage__3214EC07850B2076");

            entity.Property(e => e.ButtonText).HasMaxLength(200);
            entity.Property(e => e.ButtonUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SectionKey).HasMaxLength(100);
            entity.Property(e => e.SectionTitle).HasMaxLength(500);

            entity.HasOne(d => d.SitePage).WithMany(p => p.SitePageSections)
                .HasForeignKey(d => d.SitePageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SitePageSections_SitePages");
        });

        modelBuilder.Entity<SitePixel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SitePixe__3214EC07C8E74AD9");

            entity.Property(e => e.CampaignName).HasMaxLength(200);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FireOnPageSlug).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PixelName).HasMaxLength(200);
            entity.Property(e => e.PixelType).HasMaxLength(100);
            entity.Property(e => e.Placement)
                .HasMaxLength(100)
                .HasDefaultValue("head");
            entity.Property(e => e.SourceName).HasMaxLength(200);

            entity.HasOne(d => d.Site).WithMany(p => p.SitePixels)
                .HasForeignKey(d => d.SiteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SitePixels_Sites");
        });

        modelBuilder.Entity<SitePixelEvent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SitePixe__3214EC07F4315332");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EventName).HasMaxLength(100);
            entity.Property(e => e.PageSlug).HasMaxLength(150);
            entity.Property(e => e.Referrer).HasMaxLength(1000);
            entity.Property(e => e.Url).HasMaxLength(1000);
            entity.Property(e => e.UserAgent).HasMaxLength(1000);

            entity.HasOne(d => d.Site).WithMany(p => p.SitePixelEvents)
                .HasForeignKey(d => d.SiteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SitePixelEvents_Sites");

            entity.HasOne(d => d.SitePixel).WithMany(p => p.SitePixelEvents)
                .HasForeignKey(d => d.SitePixelId)
                .HasConstraintName("FK_SitePixelEvents_SitePixels");
        });

        modelBuilder.Entity<SiteSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SiteSett__3214EC074974042C");

            entity.Property(e => e.AddressLine1).HasMaxLength(500);
            entity.Property(e => e.AddressLine2).HasMaxLength(500);
            entity.Property(e => e.BusinessHours).HasMaxLength(500);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EmailAddress).HasMaxLength(200);
            entity.Property(e => e.FacebookUrl).HasMaxLength(500);
            entity.Property(e => e.InstagramUrl).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LinkedinUrl).HasMaxLength(500);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).HasMaxLength(100);
            entity.Property(e => e.TwitterUrl).HasMaxLength(500);
            entity.Property(e => e.YoutubeUrl).HasMaxLength(500);

            entity.HasOne(d => d.Site).WithMany(p => p.SiteSettings)
                .HasForeignKey(d => d.SiteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SiteSettings_Sites");
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
