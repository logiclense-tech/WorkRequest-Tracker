CREATE TABLE dbo.WorkRequests
(
    Id            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WorkRequests PRIMARY KEY,
    Title         NVARCHAR(150) NOT NULL,
    ClientName    NVARCHAR(150) NOT NULL,
    Description   NVARCHAR(2000) NOT NULL,
    Priority      NVARCHAR(10) NOT NULL,
    Status        NVARCHAR(20) NOT NULL CONSTRAINT DF_WorkRequests_Status DEFAULT ('New'),
    DueDate       DATETIME2 NOT NULL,
    CreatedAtUtc  DATETIME2 NOT NULL CONSTRAINT DF_WorkRequests_Created DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc  DATETIME2 NOT NULL CONSTRAINT DF_WorkRequests_Updated DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT CK_WorkRequests_Priority CHECK (Priority IN ('Low','Medium','High')),
    CONSTRAINT CK_WorkRequests_Status CHECK (Status IN ('New','InProgress','Blocked','Completed'))
);

CREATE TABLE dbo.WorkRequestNotes
(
    Id             INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WorkRequestNotes PRIMARY KEY,
    WorkRequestId  INT NOT NULL,
    [Text]         NVARCHAR(1000) NOT NULL,
    CreatedAtUtc   DATETIME2 NOT NULL CONSTRAINT DF_WorkRequestNotes_Created DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_WorkRequestNotes_WorkRequests FOREIGN KEY (WorkRequestId)
        REFERENCES dbo.WorkRequests(Id) ON DELETE CASCADE
);

-- Supports status filtering plus newest-first ordering for the list endpoint.
CREATE INDEX IX_WorkRequests_Status_CreatedAtUtc
    ON dbo.WorkRequests(Status, CreatedAtUtc DESC);

-- Supports client-name lookups and can help title/client search, depending on query plan.
CREATE INDEX IX_WorkRequests_ClientName ON dbo.WorkRequests(ClientName);

-- Supports title prefix/search patterns; a leading-wildcard Contains search may not fully use this index.
CREATE INDEX IX_WorkRequests_Title ON dbo.WorkRequests(Title);

-- Supports retrieving notes for a specific work request.
CREATE INDEX IX_WorkRequestNotes_WorkRequestId_CreatedAtUtc
    ON dbo.WorkRequestNotes(WorkRequestId, CreatedAtUtc DESC);
