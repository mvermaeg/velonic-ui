using System.Collections.Concurrent;

namespace MyApp.Api.Services.ExternalDeliveries.Mili
{
    public sealed class MiliRateLimiter
    {
        private sealed class VerticalHistory
        {
            public object SyncRoot { get; } = new();

            public List<DateTimeOffset> PingTimesUtc { get; } = new();
        }

        private readonly ConcurrentDictionary<string, VerticalHistory>
            _histories = new(
                StringComparer.OrdinalIgnoreCase);

        public bool TryAcquire(
            string vertical,
            out string? reason)
        {
            reason = null;

            var now = DateTimeOffset.UtcNow;

            var history =
                _histories.GetOrAdd(
                    vertical,
                    _ => new VerticalHistory());

            lock (history.SyncRoot)
            {
                history.PingTimesUtc.RemoveAll(
                    x => x < now.AddDays(-1));

                var lastMinute =
                    history.PingTimesUtc.Count(
                        x => x >= now.AddMinutes(-1));

                if (lastMinute >= 1)
                {
                    reason =
                        $"MILI rate limit reached for {vertical}: maximum 1 ping per minute.";

                    return false;
                }

                var lastHour =
                    history.PingTimesUtc.Count(
                        x => x >= now.AddHours(-1));

                if (lastHour >= 45)
                {
                    reason =
                        $"MILI rate limit reached for {vertical}: maximum 45 pings per hour.";

                    return false;
                }

                var lastDay =
                    history.PingTimesUtc.Count(
                        x => x >= now.AddDays(-1));

                if (lastDay >= 1250)
                {
                    reason =
                        $"MILI rate limit reached for {vertical}: maximum 1250 pings per day.";

                    return false;
                }

                history.PingTimesUtc.Add(now);

                return true;
            }
        }
    }
}