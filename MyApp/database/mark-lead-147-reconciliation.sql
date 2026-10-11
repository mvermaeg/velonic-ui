-- Preserves the NETWORX winner and post history; does not resend the lead.
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF DB_NAME() <> N'VelonicDB' THROW 51000, 'Unexpected database.', 1;
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @lock int, @now datetime2=SYSUTCDATETIME();
    EXEC @lock=sys.sp_getapplock @Resource=N'Homeyy.ExternalLead.147', @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=0;
    IF @lock<0 THROW 51001, 'Lead 147 is being processed.', 1;
    IF NOT EXISTS(SELECT 1 FROM VelonicDBUser.LeadStatusHistory WHERE LeadId=147 AND Remarks=N'Reconciliation required 147-20261011: NETWORX HTTP 400; detailed post result unavailable. No resend authorized without provider outcome evidence.')
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM VelonicDBUser.LeadRoutingRuns r JOIN VelonicDBUser.LeadRoutingAttempts a ON a.Id=r.WinnerAttemptId
            WHERE r.Id=31 AND r.LeadId=147 AND r.RoutingMode='Auction' AND r.Status='Failed' AND r.WinnerPlatformCode='NETWORX'
            AND r.PostCompletedOn IS NULL AND r.ReconciledOn IS NULL AND a.IsWinner=1 AND a.PostAttempts=1 AND a.Status='PostFailed' AND a.HttpStatusCode=400)
            THROW 51002, 'Lead 147 ownership or post outcome changed.', 1;
        IF EXISTS(SELECT 1 FROM VelonicDBUser.ExternalLeadDeliveries WHERE LeadId=147 AND (DeliveredOn IS NOT NULL OR Status='Delivered'))
           OR EXISTS(SELECT 1 FROM VelonicDBUser.LeadDeliveries WHERE LeadId=147)
            THROW 51003, 'Lead 147 has delivery evidence; do not change its status.', 1;
        UPDATE VelonicDBUser.LeadRoutingRuns SET Status='ReconciliationRequired',NextAttemptOn=NULL,UpdatedOn=@now,
            ErrorMessage=N'NETWORX HTTP 400 post acceptance unconfirmed; detailed reason unavailable. Provider reconciliation required before any resend.' WHERE Id=31;
        INSERT VelonicDBUser.LeadStatusHistory(LeadId,OldStatus,NewStatus,ChangedOn,Remarks)
        SELECT Id,LeadStatus,LeadStatus,@now,N'Reconciliation required 147-20261011: NETWORX HTTP 400; detailed post result unavailable. No resend authorized without provider outcome evidence.' FROM VelonicDBUser.Leads WHERE Id=147;
    END;
    COMMIT TRANSACTION;
    SELECT LeadId,Id RunId,Status,WinnerPlatformCode,PostCompletedOn FROM VelonicDBUser.LeadRoutingRuns WHERE Id=31;
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
