using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyApp.Api.Data;
using MyApp.Api.Data.Seeders;
using MyApp.Api.Models.Identity;
using MyApp.Api.Services.Bidding;
using MyApp.Api.Services.Deliveries;
using MyApp.Api.Services.EmailValidation;
using MyApp.Api.Services.ExternalDeliveries;
using MyApp.Api.Services.ExternalDeliveries.BlueInk;
using MyApp.Api.Services.ExternalDeliveries.Networx;
using MyApp.Api.Services.Fraud;
using MyApp.Api.Services.Location;
using MyApp.Api.Services.Ringba;
using System.Text;
using MyApp.Api.Services.MetaLeads;



var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT token only. Do not add Bearer manually."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.Configure<RingbaOptions>(
    builder.Configuration.GetSection(
        RingbaOptions.SectionName));

builder.Services.AddHttpClient<RingbaService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

//builder.Services.Configure<MetaLeadOptions>(
//    builder.Configuration.GetSection(
//        MetaLeadOptions.SectionName));

//builder.Services.AddHttpClient<MetaLeadService>(client =>
//{
//    client.Timeout = TimeSpan.FromSeconds(20);
//});

// META LEAD ADS
builder.Services.Configure<MetaLeadOptions>(
    builder.Configuration.GetSection("ExternalIntegrations:MetaLeadAds"));

builder.Services.AddScoped<MetaLeadService>();

builder.Services.AddHttpClient();

// DB-first main context
builder.Services.AddDbContext<MyAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity context
builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<BlueInkOptions>(
    builder.Configuration.GetSection(
        BlueInkOptions.SectionName));
builder.Services.Configure<NetworxOptions>(
    builder.Configuration.GetSection(
        NetworxOptions.SectionName));

builder.Services.AddScoped<ExternalLeadDistributionService>();

builder.Services.AddHttpClient<
    IExternalLeadProvider,
    BlueInkLeadProvider>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });
builder.Services.AddHttpClient<
    IExternalLeadProvider,
    NetworxLeadProvider>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });
builder.Services.AddHostedService<
    ExternalLeadDeliveryWorker>();


// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationIdentityDbContext>()
.AddDefaultTokenProviders();

// JWT
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new Exception("Jwt:Key is missing in appsettings.json");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// App services
builder.Services.AddScoped<LeadBiddingService>();
builder.Services.AddScoped<LeadDeliveryService>();
builder.Services.AddScoped<ILeadFraudService, LeadFraudService>();

builder.Services.AddHttpClient();
builder.Services.AddHttpClient<BouncerEmailValidationService>();
builder.Services.AddHttpClient<IpLocationService>();
builder.Services.AddHttpClient<IAddressValidationService, GoogleGeocodingAddressValidationService>();

// CORS for Angular
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAngular", policy =>
//    {
//        policy.WithOrigins(
//                "https://marketing.homeyy.com",
//                "http://marketing.homeyy.com",
//                "http://localhost:4200"
//            )
//            .AllowAnyHeader()
//            .AllowAnyMethod();
//    });
//});

var allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.RoutePrefix = string.Empty;
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyApp.Api v1");
});

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await IdentitySeeder.SeedAsync(app.Services);

app.Run();

 