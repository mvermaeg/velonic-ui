# Thumbtack-only coverage policy

The imported service + ZIP pair owns routing. `HasCoverageAsync` checks the
coverage row independently of search enablement, capacity or returned businesses.
An existing pair remains Thumbtack-only even when its search is disabled.

Before searching, the router saves `WinnerPlatformCode = THUMBTACK` and
`WinnerType = Recommendations` under the existing per-lead execution lock.
`ThumbtackSelected` includes successful searches with zero businesses.
`ThumbtackFailed` retains the exclusive owner, error and completion timestamp for
administration. Search failures, timeouts, capacity/configuration blocks and
ambiguous interrupted searches never start an external auction. Unknown coverage
fails closed before either remote route. Legacy `FallThrough` configuration cannot
override this policy.

Only a confirmed absent coverage pair enters the existing auction (90 seconds by
default, configurable). Winner locking, contact-free pings, winner-only posting
and durable retry rules remain unchanged. Terminal Thumbtack failures are not
automatically retried: administrators must reconcile an ambiguous remote outcome
before considering a controlled retry. Do not reset ownership to route elsewhere.

## Release

- Publish the corrected MyApp.Api Release build before the public UI. Preserve
  the deployment's configuration, secrets, data-protection keys and existing
  `ExternalLeadAuction:Enabled` configuration. Do not replace appsettings with
  local development settings.
- No SQL change or migration is required for this correction when the existing
  coverage and auction schema is already installed. Existing string status and
  ownership/error columns hold the policy state. The existing auction SQL file
  is retained as the offline test suite's schema reference, not a new migration.
- Publish the contents of `D:\Development\Velonic\Homeyy.com\dist` for the
  public Angular site after the API update. The API URL is
  `https://auth.homeyy.com/api`.
- This change does not recall leads already sent by the previous policy.

## Local validation (no database or live-provider calls)

From MyApp: `dotnet run --project tests/ExternalAuctionChecks -c Release` and
`dotnet build MyApp.Api/MyApp.Api.csproj -c Release`.

From Homeyy.com: `npm test -- --watch=false --browsers=ChromeHeadless --progress=false`
and `npm run build -- --configuration production`.

Public UI reads the existing saved-result endpoint using lead ID and UUID. It
does not select providers, start auctions, or resubmit leads while polling.
