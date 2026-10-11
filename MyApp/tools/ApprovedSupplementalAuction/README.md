# Approved supplemental round

This maintenance tool implements the owner's explicit 11 October 2026 approval
for one additional recipient of the ten NETWORX-unmatched leads: 131, 133, 134,
135, 136, 137, 138, 143, 145 and 148. It never processes the matched leads 132
and 139 or disqualified lead 141. It is not an ordinary completion entry point.

The original NETWORX routing run, winner lock and accepted delivery are retained.
The separate `VelonicDBUser.LeadSupplementalAuctions` ledger has a unique lead
key, an exact batch/lead constraint and a maximum of one post. Re-running the
tool skips all existing supplemental rounds. An interrupted post requires
provider reconciliation; there is no fallback, reping or automatic post retry.
An interrupted ping round is also held rather than blindly sent again.

Only BLUEINK and Mili can participate. NETWORX is never contacted. Modernize
lacks production configuration and InsuranceTales does not support Roofing.
Existing inactive routing rules, imported Thumbtack coverage, invalid emails,
deleted/incomplete/test leads and prior non-NETWORX posts remain blockers.
The approval is scoped to these IDs; it does not disable risk checks on future
leads or falsify their fraud scores. The round records the explicit exception.

Mili uses `Shared` on both ping and post because NETWORX has already received
the lead. Ordinary initial-auction behavior is unchanged. Mili's one-per-minute,
hourly and daily quotas are reserved under its existing cross-process quota
lock before each round begins. Ping-only reservation rows are appended to the
original audit as `SupplementalQuotaReserved`; they are not winner or delivery
records. Thus the normal worker can see these quota reservations too.

Pings overlap within a 90-second auction. Highest valid bid wins; response time
breaks equal-bid ties. The winner is persisted before the post write-ahead
fence. Full contacts go only to that winner. Providers' existing production
adapters validate mappings and compliance. The additional post is not a claim
of confirmed earnings. Provider receipt, matching and payable earnings remain
distinct outcomes.

Tokens in `OffersJson` are protected using a local persistent key ring under
the tool's ignored build directory. Do not commit, publish or remove runtime
keys while a round needs reconciliation. Raw contact data and credentials are
not stored in this supplemental ledger or printed. The ordinary Leads table
is read in memory to construct the authorized winner-only post.

## Operation

Run from `D:\Development\Velonic\MyApp`. Apply
`database/supplemental-auction-ledger.sql` once to the explicitly approved
VelonicDB target; it is additive and idempotent. The tool uses the existing
local provider configuration plus Production mappings and reads the existing
stored AWS connection silently. It validates server/database and production
modes before any HTTP. No connection string is changed.

```powershell
dotnet build tools/ApprovedSupplementalAuction/ApprovedSupplementalAuction.csproj -c Release
dotnet tools/ApprovedSupplementalAuction/bin/Release/net8.0/ApprovedSupplementalAuction.dll
```

The second command is a dry run with no database or HTTP calls. Execution
requires the exact switch `--execute-approved-unmatched-20261011` and the
owner's approval already recorded in this conversation. Do not execute against
a different account or reinterpret this approval for new IDs.

Read results without tokens or contacts:

```sql
SELECT LeadId, Status, WinnerPlatformCode, WinningBidAmount,
       PostAttempts, HttpStatusCode, PostCompletedOn, ErrorCategory
FROM VelonicDBUser.LeadSupplementalAuctions
WHERE BatchCode = 'unmatched-20261011'
ORDER BY LeadId;
```

`NoBid` means no additional contact-data post. `Delivered` means the new
provider acknowledged receipt, not that it matched or paid. `PostRejected`
and `ReconciliationRequired` keep the additional winner locked. For a rejected
or unknown post, obtain the provider's definitive outcome before considering
any further action; this tool cannot send to a third platform.

This batch is executed by the local maintenance tool, not a deployed API
worker. Publishing the API is not required to complete this already-executed
round. The new public C# Shared-mode methods are used only by this explicit
maintenance flow; no default routing policy is changed.
