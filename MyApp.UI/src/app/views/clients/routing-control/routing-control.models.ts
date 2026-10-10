export interface RoutingRun {
  id: number; leadId: number; verticalCode: string; status: string;
  winnerPlatformCode?: string; winnerClientId?: number; winningBidAmount?: number;
  auctionStartedOn?: string; auctionClosesOn?: string; winnerLockedOn?: string;
  completedOn?: string; postCompletedOn?: string; thumbtackEligibility?: string;
  thumbtackSearchId?: string; errorMessage?: string; reconciledOn?: string;
  reconciliationOutcome?: string; reconciliationReference?: string;
}
export interface RoutingAttempt {
  id: number; platformCode: string; status: string; isWinner: boolean;
  offeredBidAmount?: number; bidReceivedOn?: string; bidExpiresOn?: string;
  responseDurationMilliseconds?: number; hasPostReference: boolean;
  postAttempts: number; errorMessage?: string; pingRequestAudit?: string;
  pingResponseAudit?: string; postRequestAudit?: string; postResponseAudit?: string;
}
export interface RunDetail { run: RoutingRun; attempts: RoutingAttempt[] }
export interface NetworxReadiness {
  items: { vertical: string; option: string; leadTypeIds: number[]; ready: boolean; reason?: string }[];
  note: string;
}
