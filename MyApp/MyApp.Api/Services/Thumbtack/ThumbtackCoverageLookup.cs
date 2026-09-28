using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;

namespace MyApp.Api.Services.Thumbtack;

public static class ThumbtackCoverageLookup
{
    // Routing ownership depends on the imported pair, even when searches are disabled.
    public static Task<bool> HasCoverageAsync(MyAppDbContext db, string serviceCode, string? rawZip, CancellationToken ct)
    {
        var zip = NormalizeZip(rawZip);
        if (zip == null) throw new InvalidOperationException("Cannot determine coverage without a valid ZIP.");
        var service = serviceCode.Trim();
        return db.ThumbtackZipCoverages.AsNoTracking()
            .AnyAsync(x => x.ServiceCode == service && x.ZipCode == zip, ct);
    }

    // The supplied file lost leading zeroes. Never parse ZIPs as numbers.
    public static string? NormalizeZip(string? value)
    {
        var zip = value?.Trim();
        return zip is { Length: 4 or 5 } && zip.All(c => c is >= '0' and <= '9')
            ? zip.PadLeft(5, '0') : null;
    }

    public static Task<bool> IsCoveredAsync(
        MyAppDbContext db, string serviceCode, string? rawZip, CancellationToken ct)
    {
        var zip = NormalizeZip(rawZip);
        if (zip == null) return Task.FromResult(false);
        var service = serviceCode.Trim();
        return (from coverage in db.ThumbtackZipCoverages.AsNoTracking()
                join category in db.ThumbtackCoverageServices.AsNoTracking()
                    on coverage.ServiceCode equals category.ServiceCode
                where category.ServiceCode == service && category.IsEnabled &&
                      coverage.ZipCode == zip && coverage.IsEnabled
                select coverage).AnyAsync(ct);
    }
}
