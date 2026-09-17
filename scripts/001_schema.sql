IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'TbtbChallenge')
BEGIN
    CREATE DATABASE TbtbChallenge;
END
GO

USE TbtbChallenge;
GO

IF OBJECT_ID(N'dbo.Patients', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Patients
    (
        Id INT IDENTITY(1, 1) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        DocumentType NVARCHAR(20) NOT NULL,
        DocumentNumber NVARCHAR(50) NOT NULL,
        Phone NVARCHAR(30) NOT NULL,
        Email NVARCHAR(200) NULL,
        City NVARCHAR(100) NOT NULL,
        TreatmentStartDate DATE NOT NULL,
        Status NVARCHAR(20) NOT NULL
            CONSTRAINT DF_Patients_Status DEFAULT (N'activo')
            CONSTRAINT CK_Patients_Status CHECK (Status IN (N'activo', N'inactivo')),
        StatusChangedAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL
            CONSTRAINT DF_Patients_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_Patients PRIMARY KEY (Id),
        CONSTRAINT UQ_Patients_Document UNIQUE (DocumentType, DocumentNumber)
    );
END
GO

IF OBJECT_ID(N'dbo.Gestors', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Gestors
    (
        Id INT IDENTITY(1, 1) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Email NVARCHAR(200) NULL,
        CONSTRAINT PK_Gestors PRIMARY KEY (Id)
    );
END
GO

IF OBJECT_ID(N'dbo.Contacts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Contacts
    (
        Id INT IDENTITY(1, 1) NOT NULL,
        PatientId INT NOT NULL,
        GestorId INT NOT NULL,
        ContactDate DATE NOT NULL,
        Channel NVARCHAR(20) NOT NULL
            CONSTRAINT CK_Contacts_Channel CHECK (Channel IN (N'llamada', N'whatsapp', N'correo')),
        Result NVARCHAR(30) NOT NULL
            CONSTRAINT CK_Contacts_Result CHECK (Result IN (N'contestado', N'no contesta', N'buzón', N'número equivocado', N'reagendado', N'otro')),
        Notes NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL
            CONSTRAINT DF_Contacts_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_Contacts PRIMARY KEY (Id),
        CONSTRAINT FK_Contacts_Patients FOREIGN KEY (PatientId) REFERENCES dbo.Patients (Id),
        CONSTRAINT FK_Contacts_Gestors FOREIGN KEY (GestorId) REFERENCES dbo.Gestors (Id)
    );
END
GO

IF OBJECT_ID(N'dbo.ContactAmendments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ContactAmendments
    (
        Id INT IDENTITY(1, 1) NOT NULL,
        ContactId INT NOT NULL,
        Reason NVARCHAR(300) NOT NULL,
        AmendedByGestorId INT NOT NULL,
        OldChannel NVARCHAR(20) NULL,
        NewChannel NVARCHAR(20) NULL,
        OldResult NVARCHAR(30) NULL,
        NewResult NVARCHAR(30) NULL,
        OldContactDate DATE NULL,
        NewContactDate DATE NULL,
        OldNotes NVARCHAR(500) NULL,
        NewNotes NVARCHAR(500) NULL,
        AmendedAt DATETIME2 NOT NULL
            CONSTRAINT DF_ContactAmendments_AmendedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_ContactAmendments PRIMARY KEY (Id),
        CONSTRAINT FK_ContactAmendments_Contacts FOREIGN KEY (ContactId) REFERENCES dbo.Contacts (Id),
        CONSTRAINT FK_ContactAmendments_Gestors FOREIGN KEY (AmendedByGestorId) REFERENCES dbo.Gestors (Id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Contacts_ContactDate_GestorId')
    CREATE INDEX IX_Contacts_ContactDate_GestorId ON dbo.Contacts (ContactDate, GestorId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Contacts_PatientId_ContactDate')
    CREATE INDEX IX_Contacts_PatientId_ContactDate ON dbo.Contacts (PatientId, ContactDate);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ContactAmendments_ContactId')
    CREATE INDEX IX_ContactAmendments_ContactId ON dbo.ContactAmendments (ContactId);
GO
