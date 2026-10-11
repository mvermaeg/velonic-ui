-- One-time, user-authorized preparation. No provider HTTP calls or schema changes.
-- Run in VelonicDB only. The API worker performs the coverage check and auction.
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF DB_NAME() <> N'VelonicDB' THROW 51000, 'Unexpected database.', 1;
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @lock int, @run145 bigint, @now datetime2 = SYSUTCDATETIME();
    EXEC @lock = sys.sp_getapplock @Resource=N'Homeyy.ExternalLead.144', @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=0;
    IF @lock < 0 THROW 51001, 'Lead 144 is being processed.', 1;
    EXEC @lock = sys.sp_getapplock @Resource=N'Homeyy.ExternalLead.145', @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=0;
    IF @lock < 0 THROW 51001, 'Lead 145 is being processed.', 1;
    DECLARE @pacific datetime2 = CONVERT(datetime2, @now AT TIME ZONE 'UTC' AT TIME ZONE 'Pacific Standard Time');
    DECLARE @localOpening datetime2 = DATEADD(hour, 4, CONVERT(datetime2, CONVERT(date, @pacific)));
    IF CONVERT(time, @pacific) > '22:30:00' SET @localOpening = DATEADD(day, 1, @localOpening);
    DECLARE @next datetime2 = @now;
    IF CONVERT(time, @pacific) < '04:00:00' OR CONVERT(time, @pacific) > '22:30:00'
        SET @next = CONVERT(datetime2, @localOpening AT TIME ZONE 'Pacific Standard Time' AT TIME ZONE 'UTC');

    IF NOT EXISTS (SELECT 1 FROM VelonicDBUser.LeadStatusHistory WHERE LeadId=144 AND Remarks=N'Approved home-type correction 144-20261011: Newly constructed home.')
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM VelonicDBUser.Leads WHERE Id=144 AND Postcode='49660' AND IsTest=0 AND IsCompleted=1 AND IsDeleted=0 AND IsInvalidEmail=0 AND FraudScore<=60)
            THROW 51002, 'Lead 144 no longer meets preparation requirements.', 1;
        IF (SELECT COUNT(*) FROM VelonicDBUser.LeadRoutingRuns WHERE LeadId=144)<>1 OR
           NOT EXISTS (SELECT 1 FROM VelonicDBUser.LeadRoutingRuns WHERE Id=29 AND LeadId=144 AND RoutingMode='Auction' AND Status='Pending' AND WinnerAttemptId IS NULL AND WinnerPlatformCode IS NULL AND ThumbtackSearchStartedOn IS NULL AND PostCompletedOn IS NULL)
            THROW 51003, 'Lead 144 routing ownership changed.', 1;
        IF EXISTS (SELECT 1 FROM VelonicDBUser.ExternalLeadDeliveries WHERE LeadId=144) OR EXISTS (SELECT 1 FROM VelonicDBUser.LeadDeliveries WHERE LeadId=144) OR
           EXISTS (SELECT 1 FROM VelonicDBUser.LeadRoutingAttempts WHERE LeadId=144 AND (IsWinner=1 OR PostAttempts>0 OR Status NOT IN('Rejected','Excluded')))
            THROW 51004, 'Lead 144 has an unsafe prior attempt or delivery.', 1;
        UPDATE VelonicDBUser.Leads SET AdditionalDataJson=JSON_MODIFY(AdditionalDataJson,'$.answers.roofInstallationHomeType',N'Newly constructed home') WHERE Id=144;
        UPDATE VelonicDBUser.LeadRoutingRuns SET NextAttemptOn=@next, UpdatedOn=@now WHERE Id=29;
        INSERT VelonicDBUser.LeadStatusHistory(LeadId,OldStatus,NewStatus,ChangedOn,Remarks)
        SELECT Id,LeadStatus,LeadStatus,@now,N'Approved home-type correction 144-20261011: Newly constructed home.' FROM VelonicDBUser.Leads WHERE Id=144;
    END;

    IF NOT EXISTS (SELECT 1 FROM VelonicDBUser.LeadStatusHistory WHERE LeadId=145 AND Remarks=N'Approved retry preparation 145-20261011; awaiting updated API worker.')
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM VelonicDBUser.Leads WHERE Id=145 AND Postcode='59417' AND IsTest=0 AND IsCompleted=1 AND IsDeleted=0 AND IsInvalidEmail=0 AND FraudScore=75)
            THROW 51005, 'Lead 145 no longer matches the reviewed request.', 1;
        IF EXISTS (SELECT 1 FROM VelonicDBUser.LeadRoutingRuns WHERE LeadId=145) OR
           EXISTS (SELECT 1 FROM VelonicDBUser.LeadRoutingAttempts WHERE LeadId=145) OR
           EXISTS (SELECT 1 FROM VelonicDBUser.ExternalLeadDeliveries WHERE LeadId=145) OR
           EXISTS (SELECT 1 FROM VelonicDBUser.LeadDeliveries WHERE LeadId=145)
            THROW 51006, 'Lead 145 already has routing or delivery records.', 1;
        -- Older workers ignore this status. Only the updated worker can consume
        -- the run-specific approval without clearing or falsifying fraud scores.
        INSERT VelonicDBUser.LeadRoutingRuns(LeadId,VerticalCode,RoutingMode,Status,NextAttemptOn,CreatedOn,UpdatedOn)
        VALUES(145,'Roofing','Auction','ApprovedRetryPending',DATEADD(minute,1,@next),@now,@now);
        SET @run145=SCOPE_IDENTITY();
        INSERT VelonicDBUser.LeadStatusHistory(LeadId,OldStatus,NewStatus,ChangedOn,Remarks)
        SELECT Id,LeadStatus,LeadStatus,@now,CONCAT(N'Routing risk approval: run=',@run145,N'; user-confirmed distinct genuine request.') FROM VelonicDBUser.Leads WHERE Id=145;
        INSERT VelonicDBUser.LeadStatusHistory(LeadId,OldStatus,NewStatus,ChangedOn,Remarks)
        SELECT Id,LeadStatus,LeadStatus,@now,N'Approved retry preparation 145-20261011; awaiting updated API worker.' FROM VelonicDBUser.Leads WHERE Id=145;
    END;
    COMMIT TRANSACTION;
    SELECT LeadId,Id RunId,Status,NextAttemptOn FROM VelonicDBUser.LeadRoutingRuns WHERE LeadId IN(144,145);
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
