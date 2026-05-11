IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PersonCache' AND xtype='U')
BEGIN
    CREATE TABLE PersonCache (
        Id             INT            IDENTITY(1,1) PRIMARY KEY,
        DocumentNumber NVARCHAR(20)   NOT NULL,
        FirstName      NVARCHAR(100)  NOT NULL,
        LastName       NVARCHAR(100)  NOT NULL,
        BirthDate      DATE           NULL,
        Address        NVARCHAR(255)  NULL,
        Locality       NVARCHAR(100)  NULL,
        Phone          NVARCHAR(50)   NULL,
        Email          NVARCHAR(255)  NULL,
        LastQueriedAt  DATETIME2      NOT NULL,
        CreatedAt      DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt      DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT UQ_PersonCache_DocumentNumber UNIQUE (DocumentNumber)
    );
END
