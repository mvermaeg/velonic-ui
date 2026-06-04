using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class PostalCodeLocation
{
    public long Id { get; set; }

    public string Country { get; set; } = null!;

    public string PostalCode { get; set; } = null!;

    public string? City { get; set; }

    public string? StateCode { get; set; }

    public string? StateName { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool IsActive { get; set; }
}
