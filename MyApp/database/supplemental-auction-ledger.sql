-- Additive ledger for the explicitly approved 2026-10-11 unmatched-lead round.
-- Original routing runs, winner locks and NETWORX deliveries are unchanged.
SET XACT_ABORT ON;
IF DB_NAME()<>N'VelonicDB' THROW 51000,'Unexpected database.',1;
BEGIN TRY
 BEGIN TRANSACTION;
 IF OBJECT_ID(N'VelonicDBUser.LeadSupplementalAuctions',N'U') IS NULL
 CREATE TABLE VelonicDBUser.LeadSupplementalAuctions(
   LeadId bigint NOT NULL CONSTRAINT PK_LeadSupplementalAuctions PRIMARY KEY,
   BatchCode varchar(50) NOT NULL,
   OriginalRunId bigint NOT NULL,
   Status varchar(40) NOT NULL,
   WinnerPlatformCode varchar(50) NULL,
   WinningBidAmount decimal(18,4) NULL,
   WinnerLockedOn datetime2 NULL,
   PostStartedOn datetime2 NULL,
   PostCompletedOn datetime2 NULL,
   PostAttempts int NOT NULL CONSTRAINT DF_Supplemental_PostAttempts DEFAULT(0),
   ExternalReferenceId nvarchar(200) NULL,
   HttpStatusCode int NULL,
   ErrorCategory nvarchar(500) NULL,
   OffersJson nvarchar(max) NOT NULL CONSTRAINT DF_Supplemental_Offers DEFAULT(N'[]'),
   CreatedOn datetime2 NOT NULL,
   UpdatedOn datetime2 NOT NULL,
   CONSTRAINT FK_Supplemental_Lead FOREIGN KEY(LeadId) REFERENCES VelonicDBUser.Leads(Id),
   CONSTRAINT FK_Supplemental_Run FOREIGN KEY(OriginalRunId) REFERENCES VelonicDBUser.LeadRoutingRuns(Id),
   CONSTRAINT CK_Supplemental_OnePost CHECK(PostAttempts BETWEEN 0 AND 1),
   CONSTRAINT CK_Supplemental_Platform CHECK(WinnerPlatformCode IS NULL OR WinnerPlatformCode IN('BLUEINK','Mili')),
   CONSTRAINT CK_Supplemental_Batch CHECK(BatchCode='unmatched-20261011' AND LeadId IN(131,133,134,135,136,137,138,143,145,148))
 );
 COMMIT TRANSACTION;
END TRY
BEGIN CATCH
 IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
 THROW;
END CATCH;
