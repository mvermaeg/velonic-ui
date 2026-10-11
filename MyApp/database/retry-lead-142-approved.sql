-- Approved replay of a NoBid lead with no winner/post/delivery. No schema change.
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF DB_NAME()<>N'VelonicDB' THROW 51000,'Unexpected database.',1;
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @lock int,@now datetime2=SYSUTCDATETIME();
    EXEC @lock=sys.sp_getapplock @Resource=N'Homeyy.ExternalLead.142',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=0;
    IF @lock<0 THROW 51001,'Lead 142 is being processed.',1;
    IF NOT EXISTS(SELECT 1 FROM VelonicDBUser.LeadStatusHistory WHERE LeadId=142 AND Remarks=N'Approved safe retry 142-20261011: Cedar Shake task 329 corrected; no previous winner, post or delivery.')
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM VelonicDBUser.Leads WHERE Id=142 AND Postcode='96752' AND IsCompleted=1 AND IsTest=0 AND IsDeleted=0 AND IsInvalidEmail=0 AND FraudScore<=60)
            THROW 51002,'Lead 142 eligibility changed.',1;
        IF EXISTS(SELECT 1 FROM VelonicDBUser.ThumbtackZipCoverage WHERE ServiceCode='Roofing' AND ZipCode='96752' AND IsEnabled=1)
            THROW 51003,'Coverage changed; do not prepare an external replay.',1;
        IF (SELECT COUNT(*) FROM VelonicDBUser.LeadRoutingRuns WHERE LeadId=142)<>1 OR
           NOT EXISTS(SELECT 1 FROM VelonicDBUser.LeadRoutingRuns WHERE Id=27 AND LeadId=142 AND RoutingMode='Auction' AND Status='NoBid' AND WinnerAttemptId IS NULL AND WinnerPlatformCode IS NULL AND WinnerClientId IS NULL AND ThumbtackSearchStartedOn IS NULL AND PostCompletedOn IS NULL)
            THROW 51004,'Lead 142 ownership changed.',1;
        IF EXISTS(SELECT 1 FROM VelonicDBUser.ExternalLeadDeliveries WHERE LeadId=142) OR EXISTS(SELECT 1 FROM VelonicDBUser.LeadDeliveries WHERE LeadId=142) OR
           EXISTS(SELECT 1 FROM VelonicDBUser.LeadRoutingAttempts WHERE LeadId=142 AND (IsWinner=1 OR PostAttempts>0 OR Status NOT IN('Rejected','Excluded')))
            THROW 51005,'Prior post or unsafe attempt; no replay permitted.',1;
        IF (SELECT COUNT(*) FROM VelonicDBUser.ExternalPlatformTaskMappings m JOIN VelonicDBUser.Leads l ON l.LeadTypeId=m.LeadTypeId WHERE l.Id=142 AND m.PlatformCode='NETWORX' AND m.InternalOptionCode='ROOF_REPLACE_CEDAR_SHAKE' AND m.ExternalTaskId='329' AND m.IsActive=1)<>1
            THROW 51006,'Approved Cedar Shake replacement mapping missing or ambiguous.',1;
        UPDATE VelonicDBUser.LeadRoutingRuns SET Status='Pending',NextAttemptOn=@now,CompletedOn=NULL,AuctionStartedOn=NULL,AuctionClosesOn=NULL,ErrorMessage=NULL,UpdatedOn=@now WHERE Id=27;
        INSERT VelonicDBUser.LeadStatusHistory(LeadId,OldStatus,NewStatus,ChangedOn,Remarks)
        SELECT Id,LeadStatus,LeadStatus,@now,N'Approved safe retry 142-20261011: Cedar Shake task 329 corrected; no previous winner, post or delivery.' FROM VelonicDBUser.Leads WHERE Id=142;
    END;
    COMMIT TRANSACTION;
    SELECT Id RunId,LeadId,Status,NextAttemptOn FROM VelonicDBUser.LeadRoutingRuns WHERE Id=27;
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
