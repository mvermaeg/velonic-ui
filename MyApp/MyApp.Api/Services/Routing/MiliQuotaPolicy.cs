namespace MyApp.Api.Services.Routing;

public static class MiliQuotaPolicy
{
    public static TimeSpan Wait(IReadOnlyCollection<DateTime> reservations, DateTime now)
    {
        var ready = now;
        foreach (var (window, cap) in new[] { (TimeSpan.FromMinutes(1), 1), (TimeSpan.FromHours(1), 45), (TimeSpan.FromDays(1), 1250) })
        {
            var active = reservations.Where(x => x >= now - window).OrderBy(x => x).ToArray();
            if (active.Length >= cap)
            {
                var next = active[active.Length - cap] + window + TimeSpan.FromMilliseconds(100);
                if (next > ready) ready = next;
            }
        }
        return ready - now;
    }
}
