-- Execute in normal SSMS mode against the approved VelonicDB, with API workers stopped.
-- No coverage import or connection changes. Back up first; conflicting existing data aborts safely.
USE [VelonicDB];
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF @@TRANCOUNT <> 0 THROW 51200, 'Finish the existing transaction before deploying.', 1;
BEGIN TRY
BEGIN TRANSACTION;
DECLARE @lock int;
EXEC @lock=sys.sp_getapplock @Resource='Homeyy.AuctionSchema', @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=30000;
IF @lock<0 THROW 51201, 'Auction deployment lock unavailable.', 1;
IF SCHEMA_ID('VelonicDBUser') IS NULL THROW 51202, 'Existing application schema is required.', 1;
IF OBJECT_ID('VelonicDBUser.ExternalLeadDeliveries','U') IS NULL THROW 51203, 'Existing ExternalLeadDeliveries table is required.', 1;

IF OBJECT_ID('VelonicDBUser.LeadRoutingRules','U') IS NULL
CREATE TABLE VelonicDBUser.LeadRoutingRules (
 Id bigint IDENTITY PRIMARY KEY, DestinationType varchar(30) NOT NULL,
 ClientId bigint NULL, PlatformCode varchar(50) NULL, VerticalCode varchar(100) NOT NULL,
 State varchar(100) NULL, Postcode varchar(50) NULL, BidAmount decimal(18,2) NOT NULL DEFAULT 0,
 Priority int NOT NULL DEFAULT 100, DailyCap int NULL, MonthlyCap int NULL,
 IsActive bit NOT NULL DEFAULT 0, CreatedOn datetime2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedOn datetime2 NULL
);
IF OBJECT_ID('VelonicDBUser.LeadRoutingRuns','U') IS NULL
CREATE TABLE VelonicDBUser.LeadRoutingRuns (
 Id bigint IDENTITY PRIMARY KEY, LeadId bigint NOT NULL, VerticalCode varchar(100) NOT NULL,
 Status varchar(30) NOT NULL DEFAULT 'Pending', WinnerType varchar(30) NULL,
 WinnerClientId bigint NULL, WinnerPlatformCode varchar(50) NULL, WinningBidAmount decimal(18,2) NULL,
 NextAttemptOn datetime2 NULL, CompletedOn datetime2 NULL, ErrorMessage nvarchar(1000) NULL,
 CreatedOn datetime2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedOn datetime2 NULL, RowVersion rowversion NOT NULL
);
IF OBJECT_ID('VelonicDBUser.LeadRoutingAttempts','U') IS NULL
CREATE TABLE VelonicDBUser.LeadRoutingAttempts (
 Id bigint IDENTITY PRIMARY KEY, LeadRoutingRunId bigint NOT NULL, LeadId bigint NOT NULL,
 LeadRoutingRuleId bigint NULL, DestinationType varchar(30) NOT NULL, ClientId bigint NULL,
 PlatformCode varchar(50) NULL, BidAmount decimal(18,2) NOT NULL DEFAULT 0,
 AttemptNumber int NOT NULL, Status varchar(30) NOT NULL, IsRetryable bit NOT NULL DEFAULT 0,
 HttpStatusCode int NULL, ExternalReferenceId nvarchar(500) NULL, ErrorMessage nvarchar(1000) NULL,
 CreatedOn datetime2 NOT NULL DEFAULT SYSUTCDATETIME(), CompletedOn datetime2 NULL
);

DECLARE @columns TABLE (TableName sysname, ColumnName sysname, Definition nvarchar(200));
INSERT @columns VALUES
 ('LeadRoutingRules','Id','bigint IDENTITY NOT NULL'),('LeadRoutingRules','DestinationType','varchar(30) NOT NULL'),
 ('LeadRoutingRules','ClientId','bigint NULL'),('LeadRoutingRules','PlatformCode','varchar(50) NULL'),
 ('LeadRoutingRules','VerticalCode','varchar(100) NOT NULL'),('LeadRoutingRules','State','varchar(100) NULL'),
 ('LeadRoutingRules','Postcode','varchar(50) NULL'),('LeadRoutingRules','BidAmount','decimal(18,2) NOT NULL DEFAULT 0'),
 ('LeadRoutingRules','Priority','int NOT NULL DEFAULT 100'),('LeadRoutingRules','DailyCap','int NULL'),
 ('LeadRoutingRules','MonthlyCap','int NULL'),('LeadRoutingRules','IsActive','bit NOT NULL DEFAULT 0'),
 ('LeadRoutingRules','CreatedOn','datetime2 NOT NULL DEFAULT SYSUTCDATETIME()'),('LeadRoutingRules','UpdatedOn','datetime2 NULL'),
 ('LeadRoutingRuns','Id','bigint IDENTITY NOT NULL'),('LeadRoutingRuns','LeadId','bigint NOT NULL'),
 ('LeadRoutingRuns','VerticalCode','varchar(100) NOT NULL'),('LeadRoutingRuns','Status','varchar(30) NOT NULL DEFAULT ''Pending'''),
 ('LeadRoutingRuns','WinnerType','varchar(30) NULL'),('LeadRoutingRuns','WinnerClientId','bigint NULL'),
 ('LeadRoutingRuns','WinnerPlatformCode','varchar(50) NULL'),('LeadRoutingRuns','WinningBidAmount','decimal(18,4) NULL'),
 ('LeadRoutingRuns','NextAttemptOn','datetime2 NULL'),('LeadRoutingRuns','CompletedOn','datetime2 NULL'),
 ('LeadRoutingRuns','ErrorMessage','nvarchar(1000) NULL'),('LeadRoutingRuns','CreatedOn','datetime2 NOT NULL DEFAULT SYSUTCDATETIME()'),
 ('LeadRoutingRuns','UpdatedOn','datetime2 NULL'),('LeadRoutingRuns','RowVersion','rowversion NOT NULL'),
 ('LeadRoutingAttempts','Id','bigint IDENTITY NOT NULL'),('LeadRoutingAttempts','LeadRoutingRunId','bigint NOT NULL'),
 ('LeadRoutingAttempts','LeadId','bigint NOT NULL'),('LeadRoutingAttempts','LeadRoutingRuleId','bigint NULL'),
 ('LeadRoutingAttempts','DestinationType','varchar(30) NOT NULL'),('LeadRoutingAttempts','ClientId','bigint NULL'),
 ('LeadRoutingAttempts','PlatformCode','varchar(50) NULL'),('LeadRoutingAttempts','BidAmount','decimal(18,2) NOT NULL DEFAULT 0'),
 ('LeadRoutingAttempts','AttemptNumber','int NOT NULL'),('LeadRoutingAttempts','Status','varchar(30) NOT NULL'),
 ('LeadRoutingAttempts','IsRetryable','bit NOT NULL DEFAULT 0'),('LeadRoutingAttempts','HttpStatusCode','int NULL'),
 ('LeadRoutingAttempts','ExternalReferenceId','nvarchar(500) NULL'),('LeadRoutingAttempts','ErrorMessage','nvarchar(1000) NULL'),
 ('LeadRoutingAttempts','CreatedOn','datetime2 NOT NULL DEFAULT SYSUTCDATETIME()'),('LeadRoutingAttempts','CompletedOn','datetime2 NULL'),
 ('LeadRoutingRules','EffectiveFrom','datetime2 NULL'),('LeadRoutingRules','EffectiveTo','datetime2 NULL'),
 ('LeadRoutingRuns','RoutingMode','varchar(16) NOT NULL DEFAULT ''Legacy'' WITH VALUES'),
 ('LeadRoutingRuns','AuctionStartedOn','datetime2 NULL'),('LeadRoutingRuns','AuctionClosesOn','datetime2 NULL'),
 ('LeadRoutingRuns','WinnerSelectedOn','datetime2 NULL'),('LeadRoutingRuns','WinnerLockedOn','datetime2 NULL'),
 ('LeadRoutingRuns','PostCompletedOn','datetime2 NULL'),('LeadRoutingRuns','WinnerAttemptId','bigint NULL'),
 ('LeadRoutingRuns','ThumbtackEligibility','nvarchar(1000) NULL'),('LeadRoutingRuns','ThumbtackSearchId','nvarchar(500) NULL'),
 ('LeadRoutingRuns','ThumbtackResponseJson','nvarchar(max) NULL'),
 ('LeadRoutingRuns','ThumbtackSearchStartedOn','datetime2 NULL'),
 ('LeadRoutingRuns','ReconciledOn','datetime2 NULL'),('LeadRoutingRuns','ReconciledBy','nvarchar(450) NULL'),
 ('LeadRoutingRuns','ReconciliationReference','nvarchar(100) NULL'),('LeadRoutingRuns','ReconciliationOutcome','nvarchar(20) NULL'),
 ('LeadRoutingAttempts','IsWinner','bit NOT NULL DEFAULT 0 WITH VALUES'),
 ('LeadRoutingAttempts','PingDispatchedOn','datetime2 NULL'),
 ('LeadRoutingAttempts','BidReceivedOn','datetime2 NULL'),('LeadRoutingAttempts','BidExpiresOn','datetime2 NULL'),
 ('LeadRoutingAttempts','OfferedBidAmount','decimal(18,4) NULL'),('LeadRoutingAttempts','ResponseDurationMilliseconds','bigint NULL'),
 ('LeadRoutingAttempts','PingReferenceId','nvarchar(max) NULL'),('LeadRoutingAttempts','PostContext','nvarchar(max) NULL'),
 ('LeadRoutingAttempts','PingRequestAudit','nvarchar(max) NULL'),('LeadRoutingAttempts','PingResponseAudit','nvarchar(max) NULL'),
 ('LeadRoutingAttempts','PostRequestAudit','nvarchar(max) NULL'),('LeadRoutingAttempts','PostResponseAudit','nvarchar(max) NULL'),
 ('LeadRoutingAttempts','PostStartedOn','datetime2 NULL'),('LeadRoutingAttempts','PostAttempts','int NOT NULL DEFAULT 0 WITH VALUES');
DECLARE @table sysname, @column sysname, @definition nvarchar(200), @sql nvarchar(max);
-- Safe widening from earlier empty/partial versions; never truncate populated data.
IF COL_LENGTH('VelonicDBUser.LeadRoutingRuns','WinningBidAmount') IS NOT NULL
 ALTER TABLE VelonicDBUser.LeadRoutingRuns ALTER COLUMN WinningBidAmount decimal(18,4) NULL;
IF COL_LENGTH('VelonicDBUser.LeadRoutingRules','State') BETWEEN 1 AND 99 AND EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('VelonicDBUser.LeadRoutingRules') AND name='State' AND system_type_id=167)
 ALTER TABLE VelonicDBUser.LeadRoutingRules ALTER COLUMN State varchar(100) NULL;
IF COL_LENGTH('VelonicDBUser.LeadRoutingRules','Postcode') BETWEEN 1 AND 49 AND EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('VelonicDBUser.LeadRoutingRules') AND name='Postcode' AND system_type_id=167)
 ALTER TABLE VelonicDBUser.LeadRoutingRules ALTER COLUMN Postcode varchar(50) NULL;
IF COL_LENGTH('VelonicDBUser.LeadRoutingAttempts','ExternalReferenceId') BETWEEN 1 AND 999
 ALTER TABLE VelonicDBUser.LeadRoutingAttempts ALTER COLUMN ExternalReferenceId nvarchar(500) NULL;
DECLARE changes CURSOR LOCAL FAST_FORWARD FOR SELECT TableName,ColumnName,Definition FROM @columns;
OPEN changes;
FETCH NEXT FROM changes INTO @table,@column,@definition;
WHILE @@FETCH_STATUS=0
BEGIN
 IF COL_LENGTH('VelonicDBUser.'+@table,@column) IS NULL
 BEGIN
  IF @column IN ('Id','DestinationType','ClientId','PlatformCode','VerticalCode','State','Postcode','BidAmount','Priority',
    'DailyCap','MonthlyCap','IsActive','CreatedOn','UpdatedOn','LeadId','Status','WinnerType','WinnerClientId',
    'WinnerPlatformCode','WinningBidAmount','NextAttemptOn','CompletedOn','ErrorMessage','RowVersion',
    'LeadRoutingRunId','LeadRoutingRuleId','AttemptNumber','IsRetryable','HttpStatusCode','ExternalReferenceId')
  BEGIN
   DECLARE @populated bit;
   SET @sql=N'SELECT @hasRows=CASE WHEN EXISTS(SELECT 1 FROM VelonicDBUser.'+QUOTENAME(@table)+N') THEN 1 ELSE 0 END;';
   EXEC sys.sp_executesql @sql,N'@hasRows bit OUTPUT',@hasRows=@populated OUTPUT;
   IF @populated=1 THROW 51211, 'Populated partial legacy schema requires review; baseline values will not be invented. No data deleted.', 1;
  END;
  SET @sql=N'ALTER TABLE VelonicDBUser.'+QUOTENAME(@table)+N' ADD '+QUOTENAME(@column)+N' '+@definition+N';';
  EXEC sys.sp_executesql @sql;
 END;
 DECLARE @actual nvarchar(200), @nullable bit;
 SELECT @actual=CASE TYPE_NAME(c.system_type_id)
   WHEN 'timestamp' THEN 'rowversion'
   WHEN 'varchar' THEN 'varchar('+CASE WHEN c.max_length=-1 THEN 'max' ELSE CONVERT(varchar(10),c.max_length) END+')'
   WHEN 'nvarchar' THEN 'nvarchar('+CASE WHEN c.max_length=-1 THEN 'max' ELSE CONVERT(varchar(10),c.max_length/2) END+')'
   WHEN 'decimal' THEN 'decimal('+CONVERT(varchar(10),c.precision)+','+CONVERT(varchar(10),c.scale)+')'
   ELSE TYPE_NAME(c.system_type_id) END, @nullable=c.is_nullable
 FROM sys.columns c WHERE c.object_id=OBJECT_ID('VelonicDBUser.'+@table) AND c.name=@column;
 IF @actual<>LEFT(@definition,CHARINDEX(' ',@definition)-1) OR
    @nullable<>CASE WHEN @definition LIKE '%NOT NULL%' THEN 0 ELSE 1 END
 BEGIN
  DECLARE @mismatch nvarchar(2048)='Incompatible existing column: '+@table+'.'+@column+'. No data was deleted; review schema drift.';
  THROW 51206, @mismatch, 1;
 END;
 IF @column='Id' AND COLUMNPROPERTY(OBJECT_ID('VelonicDBUser.'+@table),@column,'IsIdentity')<>1
  THROW 51207, 'Routing Id must be an identity column. Existing data retained.', 1;
 IF @actual='datetime2' AND EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('VelonicDBUser.'+@table) AND name=@column AND scale<>7)
  THROW 51208, 'Routing timestamps must use datetime2(7). Existing data retained.', 1;
 FETCH NEXT FROM changes INTO @table,@column,@definition;
END;
CLOSE changes;
DEALLOCATE changes;
-- Empty partially created tables can be repaired; incompatible populated tables fail transactionally.
IF NOT EXISTS(SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID('VelonicDBUser.LeadRoutingRules') AND type='PK')
 EXEC(N'ALTER TABLE VelonicDBUser.LeadRoutingRules ADD PRIMARY KEY(Id);');
IF NOT EXISTS(SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID('VelonicDBUser.LeadRoutingRuns') AND type='PK')
 EXEC(N'ALTER TABLE VelonicDBUser.LeadRoutingRuns ADD PRIMARY KEY(Id);');
IF NOT EXISTS(SELECT 1 FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID('VelonicDBUser.LeadRoutingAttempts') AND type='PK')
 EXEC(N'ALTER TABLE VelonicDBUser.LeadRoutingAttempts ADD PRIMARY KEY(Id);');
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID('VelonicDBUser.LeadRoutingRuns') AND referenced_object_id=OBJECT_ID('VelonicDBUser.Leads'))
 EXEC(N'ALTER TABLE VelonicDBUser.LeadRoutingRuns WITH CHECK ADD FOREIGN KEY(LeadId) REFERENCES VelonicDBUser.Leads(Id);');
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID('VelonicDBUser.LeadRoutingAttempts') AND referenced_object_id=OBJECT_ID('VelonicDBUser.LeadRoutingRuns'))
 EXEC(N'ALTER TABLE VelonicDBUser.LeadRoutingAttempts WITH CHECK ADD FOREIGN KEY(LeadRoutingRunId) REFERENCES VelonicDBUser.LeadRoutingRuns(Id);');
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID('VelonicDBUser.LeadRoutingAttempts') AND referenced_object_id=OBJECT_ID('VelonicDBUser.LeadRoutingRules'))
 EXEC(N'ALTER TABLE VelonicDBUser.LeadRoutingAttempts WITH CHECK ADD FOREIGN KEY(LeadRoutingRuleId) REFERENCES VelonicDBUser.LeadRoutingRules(Id);');
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID('VelonicDBUser.LeadRoutingAttempts') AND referenced_object_id=OBJECT_ID('VelonicDBUser.Leads'))
 EXEC(N'ALTER TABLE VelonicDBUser.LeadRoutingAttempts WITH CHECK ADD FOREIGN KEY(LeadId) REFERENCES VelonicDBUser.Leads(Id);');
ALTER TABLE VelonicDBUser.LeadRoutingRuns ALTER COLUMN WinningBidAmount decimal(18,4) NULL;
-- Existing rules are preserved; only the default for newly inserted rules changes.
DECLARE @oldDefault sysname;
SELECT @oldDefault=d.name FROM sys.default_constraints d JOIN sys.columns c ON c.object_id=d.parent_object_id AND c.column_id=d.parent_column_id
 WHERE d.parent_object_id=OBJECT_ID('VelonicDBUser.LeadRoutingRules') AND c.name='IsActive';
IF @oldDefault IS NOT NULL
BEGIN
 SET @sql=N'ALTER TABLE VelonicDBUser.LeadRoutingRules DROP CONSTRAINT '+QUOTENAME(@oldDefault)+';';
 EXEC sys.sp_executesql @sql;
END;
ALTER TABLE VelonicDBUser.LeadRoutingRules ADD DEFAULT 0 FOR IsActive;

EXEC(N'IF EXISTS(SELECT 1 FROM VelonicDBUser.LeadRoutingRuns GROUP BY LeadId HAVING COUNT(*)>1)
 THROW 51204, ''Duplicate routing runs require review; no data deleted.'', 1;');
IF EXISTS(SELECT 1 FROM VelonicDBUser.ExternalLeadDeliveries GROUP BY LeadId,PlatformCode HAVING COUNT(*)>1)
 THROW 51205, 'Duplicate external deliveries require review; no data deleted.', 1;
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('VelonicDBUser.LeadRoutingRuns') AND name='UX_LeadRoutingRuns_LeadId')
 EXEC(N'CREATE UNIQUE INDEX UX_LeadRoutingRuns_LeadId ON VelonicDBUser.LeadRoutingRuns(LeadId);');
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('VelonicDBUser.ExternalLeadDeliveries') AND name='UX_Auction_LeadProvider')
 CREATE UNIQUE INDEX UX_Auction_LeadProvider ON VelonicDBUser.ExternalLeadDeliveries(LeadId,PlatformCode);
-- Dynamic statements compile after the additive columns exist.
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('VelonicDBUser.LeadRoutingAttempts') AND name='UX_Auction_OneWinner')
 EXEC(N'CREATE UNIQUE INDEX UX_Auction_OneWinner ON VelonicDBUser.LeadRoutingAttempts(LeadRoutingRunId) WHERE IsWinner=1;');
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('VelonicDBUser.LeadRoutingRuns') AND name='UX_Auction_WinnerAttempt')
 EXEC(N'CREATE UNIQUE INDEX UX_Auction_WinnerAttempt ON VelonicDBUser.LeadRoutingRuns(WinnerAttemptId) WHERE WinnerAttemptId IS NOT NULL;');
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('VelonicDBUser.LeadRoutingRuns') AND name='IX_Auction_Work')
 EXEC(N'CREATE INDEX IX_Auction_Work ON VelonicDBUser.LeadRoutingRuns(RoutingMode,Status,NextAttemptOn);');
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('VelonicDBUser.LeadRoutingAttempts') AND name='IX_Auction_RuleReservations')
 EXEC(N'CREATE INDEX IX_Auction_RuleReservations ON VelonicDBUser.LeadRoutingAttempts(LeadRoutingRuleId,IsWinner,CreatedOn);');
IF OBJECT_ID('VelonicDBUser.FK_Auction_WinnerAttempt','F') IS NULL
 EXEC(N'ALTER TABLE VelonicDBUser.LeadRoutingRuns WITH CHECK ADD CONSTRAINT FK_Auction_WinnerAttempt FOREIGN KEY(WinnerAttemptId) REFERENCES VelonicDBUser.LeadRoutingAttempts(Id);');
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('VelonicDBUser.LeadRoutingAttempts') AND name='UX_Auction_RunAttempt')
 EXEC(N'CREATE UNIQUE INDEX UX_Auction_RunAttempt ON VelonicDBUser.LeadRoutingAttempts(LeadRoutingRunId,Id);');
IF OBJECT_ID('VelonicDBUser.FK_Auction_WinnerSameRun','F') IS NULL
 EXEC(N'ALTER TABLE VelonicDBUser.LeadRoutingRuns WITH CHECK ADD CONSTRAINT FK_Auction_WinnerSameRun FOREIGN KEY(Id,WinnerAttemptId) REFERENCES VelonicDBUser.LeadRoutingAttempts(LeadRoutingRunId,Id);');
IF EXISTS(SELECT 1 FROM (VALUES
 ('LeadRoutingRuns','UX_LeadRoutingRuns_LeadId'),('LeadRoutingRuns','UX_Auction_WinnerAttempt'),
 ('LeadRoutingAttempts','UX_Auction_OneWinner'),('ExternalLeadDeliveries','UX_Auction_LeadProvider')) required(TableName,IndexName)
 WHERE NOT EXISTS(SELECT 1 FROM sys.indexes i WHERE i.object_id=OBJECT_ID('VelonicDBUser.'+required.TableName)
 AND i.name=required.IndexName AND i.is_unique=1 AND i.is_disabled=0))
 THROW 51209, 'An existing named safety index is disabled or not unique. No data deleted.', 1;
DECLARE @requiredIndexes TABLE(TableName sysname,IndexName sysname,FirstColumn sysname,SecondColumn sysname NULL,FilterText varchar(100) NULL);
INSERT @requiredIndexes VALUES
 ('LeadRoutingRuns','UX_LeadRoutingRuns_LeadId','LeadId',NULL,NULL),
 ('LeadRoutingRuns','UX_Auction_WinnerAttempt','WinnerAttemptId',NULL,'WinnerAttemptIdISNOTNULL'),
 ('LeadRoutingAttempts','UX_Auction_OneWinner','LeadRoutingRunId',NULL,'IsWinner=1'),
 ('ExternalLeadDeliveries','UX_Auction_LeadProvider','LeadId','PlatformCode',NULL);
IF EXISTS(SELECT 1 FROM @requiredIndexes r JOIN sys.indexes i ON i.object_id=OBJECT_ID('VelonicDBUser.'+r.TableName) AND i.name=r.IndexName
 WHERE INDEX_COL('VelonicDBUser.'+r.TableName,i.index_id,1)<>r.FirstColumn OR
 ISNULL(INDEX_COL('VelonicDBUser.'+r.TableName,i.index_id,2),'')<>ISNULL(r.SecondColumn,'') OR
 INDEX_COL('VelonicDBUser.'+r.TableName,i.index_id,3) IS NOT NULL OR
 REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ISNULL(i.filter_definition,''),'[',''),']',''),'(',''),')',''),' ','')<>ISNULL(r.FilterText,''))
 THROW 51210, 'An existing named safety index has incompatible keys or filter. No data deleted.', 1;
ALTER TABLE VelonicDBUser.LeadRoutingRuns WITH CHECK CHECK CONSTRAINT FK_Auction_WinnerAttempt;
ALTER TABLE VelonicDBUser.LeadRoutingRuns WITH CHECK CHECK CONSTRAINT FK_Auction_WinnerSameRun;
COMMIT TRANSACTION;
SELECT 'PASS: auction schema deployed; coverage untouched' AS Result;
END TRY
BEGIN CATCH
 IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
 THROW;
END CATCH;
