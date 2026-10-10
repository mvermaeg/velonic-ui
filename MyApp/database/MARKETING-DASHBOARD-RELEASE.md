# Live marketing dashboard

The `/dashboard` route belongs to `D:\Development\Velonic\MyApp.UI`, not the public Homeyy.com site.

## Data and meaning

The page requests `GET https://auth.homeyy.com/api/reports/dashboard` using the existing authenticated HTTP pipeline. The API requires the Admin or SuperAdmin role and returns aggregate counts and non-contact routing metadata. It never returns provider payloads, credentials or contact details. No routing or lead-submission behavior changes.

- Test and deleted leads are excluded from all metrics.
- Today and chart hours use UTC. The chart contains 24 hourly buckets, including the current partial hour.
- Delivered today counts distinct leads with an accepted internal or external delivery record. Historical duplicate platform delivery records count once in the global total; each provider's total counts its own distinct leads.
- Provider deliveries, Thumbtack ownership and no-bid counts use the last 30 UTC calendar days, including today. Active routing and attention counts cover all current records. The latest 20 routing runs are shown without contact data.
- Delivery acceptance does not establish provider matching, payable revenue or bank payment. Confirmed earnings, acquisition costs and profit are unavailable because no reconciled earnings/cost ledger exists in the inspected application schema. Existing lead Sales/Profit fields and auction bids are deliberately not treated as verified financial data. NETWORX portal earnings cannot be automatically obtained from a screenshot.
- Refresh reads the API again. Failure clears stale figures and shows an error rather than fabricated zeros; an empty successful report displays zeros and empty states.

## Publish

1. Publish MyApp.Api in Release using the existing deployment process and preserve live server configuration and credentials.
2. Build MyApp.UI with `npm run build -- --configuration production` and publish the contents of `D:\Development\Velonic\MyApp.UI\dist\velonic\browser` to the existing marketing site. Preserve its SPA rewrite and hosting configuration.
3. Sign in as Admin/SuperAdmin, open `https://marketing.homeyy.com/#/dashboard`, and confirm the dashboard request returns 200. The existing API CORS policy must permit the marketing origin. If it does not, add the exact origin to the existing policy; do not make CORS unrestricted.
4. Confirm the counts against authenticated operational reports and that failed API requests show the error state. Financial metrics remain explicitly unavailable until an authoritative provider reconciliation and cost source is implemented.

No SQL migration or production database connection is needed to implement or test this report. Both API and marketing UI deployment are required; publishing only the public Homeyy.com site will not update this dashboard.
