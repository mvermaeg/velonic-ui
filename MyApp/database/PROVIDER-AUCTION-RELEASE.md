# Provider auction correction

No production database connection or provider submission is required to build or test this change.

## Result

- Covered service + ZIP remains owned exclusively by Thumbtack, including errors and zero results.
- Uncovered leads ping configured eligible providers concurrently. One winner is atomically locked; only that winner receives full contacts. Duplicate completion and worker restart do not reopen a delivered lead.
- BLUEINK's production gateway fetches only TrustedForm/Jornaya proof from the server for its documented ping contract. Missing or invalid proof blocks that provider's ping. Certificates and identifiers remain redacted in audit payloads.
- Mili waits for a shared database quota slot without holding its reservation lock while waiting. The auction deadline cancels the wait. A quota slot that becomes available after the 90-second auction is not sent later, because doing so would bypass exclusive winner selection. Reservations survive restart and ambiguous sends are not replayed.
- Modernize is no longer excluded by name. Its existing monetary ping/post adapter participates when configured. Real leads cannot be sent to staging; a production tag ID and provider approval are required. TrustedForm proof is required for winner posting. Unknown post outcomes require reconciliation, never a second provider.

## Deployment

1. Obtain Modernize's approved production tag ID and account/service activation. Configure `ExternalIntegrations:Modernize:ProductionTagId` on the server. Preserve credentials outside source control; do not use the staging tag for production.
2. Preserve live DefaultConnection and provider credentials. Run with ASPNETCORE_ENVIRONMENT=Production so appsettings.Production.json supplies BLUEINK roofing mappings and the persistent key-ring path. Create the key-ring folder and grant the API identity read/write access.
3. Publish MyApp.Api in Release and replace the API application files using the normal deployment procedure; restart its application pool. Publish the Homeyy.com production build as well: both public roofing wizards now collect the home type for installation. No new SQL schema migration is required. Existing auction columns, including PingDispatchedOn, must already be deployed.
4. Review startup readiness logs and the first eligible new lead's sanitized attempts. Positive bids are not guaranteed. Missing required answers, consent proof, provider configuration, coverage, quotas, or a no-buyer response can legitimately prevent bids.
5. Do not replay delivered, unmatched, disqualified, or unknown-outcome leads without provider reconciliation and an audited release decision.

## Remaining provider information

The supplied NETWORX CSV confirms the 20-task list but still lacks explicit Tar installation/repair/replacement and Natural Slate installation mappings. Unsupported options remain blocked; no task IDs are guessed. Both public roofing wizards preserve “Install New Roof” and ask whether the home is newly constructed or existing. BLUEINK uses the answer to select the documented value; historical ambiguous installation answers remain blocked. Modernize also uses the home type. Mili campaign activation and live acceptance require provider confirmation or observation of a genuine eligible lead after deployment; local configuration alone does not establish acceptance.
