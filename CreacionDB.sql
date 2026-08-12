USE [vitalapp_];
GO

CREATE TABLE [dbo].[Role]
(
    RoleId  INT             IDENTITY(1,1) NOT NULL,
    Name    NVARCHAR(50)    NOT NULL,
    CONSTRAINT PK_Role PRIMARY KEY (RoleId),
    CONSTRAINT UQ_Role_Name UNIQUE (Name)
);
GO

SET IDENTITY_INSERT [dbo].[Role] ON;
INSERT INTO [dbo].[Role] (RoleId, Name) VALUES (1, N'Patient'), (2, N'Doctor');
SET IDENTITY_INSERT [dbo].[Role] OFF;
GO

CREATE TABLE [dbo].[User]
(
    UserId      INT             IDENTITY(1,1) NOT NULL,
    Email       NVARCHAR(200)   NOT NULL,
    Password    NVARCHAR(500)   NOT NULL,
    IsActive    BIT             NOT NULL DEFAULT (1),
    CreatedAt   DATETIME2       NOT NULL DEFAULT (SYSUTCDATETIME()),
    UpdatedAt   DATETIME2       NULL,
    DeletedAt   DATETIME2       NULL,
    RoleId      INT             NOT NULL DEFAULT (1),
    CONSTRAINT PK_User PRIMARY KEY (UserId),
    CONSTRAINT UQ_User_Email UNIQUE (Email),
    CONSTRAINT FK_User_Role FOREIGN KEY (RoleId) REFERENCES [dbo].[Role](RoleId)
);
GO

CREATE TABLE [dbo].[Profile]
(
    ProfileId   INT             IDENTITY(1,1) NOT NULL,
    UserId      INT             NOT NULL,
    Name        NVARCHAR(250)   NOT NULL,
    FirstName   NVARCHAR(250)   NOT NULL,
    LastName    NVARCHAR(250)   NOT NULL,
    BirthDate   DATETIME2       NULL,
    Gender      NVARCHAR(50)    NULL,
    Height      DECIMAL(5,2)    NULL,
    Weight      DECIMAL(5,2)    NULL,
    CreatedAt   DATETIME2       NOT NULL DEFAULT (SYSUTCDATETIME()),
    UpdatedAt   DATETIME2       NULL,
    DeletedAt   DATETIME2       NULL,
    IdCard      NVARCHAR(50)    NULL,
    CONSTRAINT PK_Profile PRIMARY KEY (ProfileId),
    CONSTRAINT UQ_Profile_IdCard UNIQUE (IdCard),
    CONSTRAINT FK_Profile_User FOREIGN KEY (UserId) REFERENCES [dbo].[User](UserId)
);
GO

CREATE TABLE [dbo].[MedicalCondition]
(
    MedicalConditionId  INT             IDENTITY(1,1) NOT NULL,
    UserId              INT             NOT NULL,
    Name                NVARCHAR(250)   NOT NULL,
    Description         NVARCHAR(250)   NOT NULL,
    DiagnosticDate      DATETIME2       NOT NULL,
    CreatedAt           DATETIME2       NOT NULL DEFAULT (SYSUTCDATETIME()),
    UpdatedAt           DATETIME2       NULL,
    DeletedAt           DATETIME2       NULL,
    CONSTRAINT PK_MedicalCondition PRIMARY KEY (MedicalConditionId),
    CONSTRAINT FK_MedicalCondition_User FOREIGN KEY (UserId) REFERENCES [dbo].[User](UserId)
);
GO

CREATE TABLE [dbo].[HealthIndicatorType]
(
    IndicatorTypeId INT             IDENTITY(1,1) NOT NULL,
    Name            NVARCHAR(250)   NOT NULL,
    Unit            NVARCHAR(250)   NOT NULL,
    MinNormalValue  DECIMAL(6,2)    NOT NULL,
    MaxNormalValue  DECIMAL(6,2)    NOT NULL,
    Gender          BIT             NOT NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT (SYSUTCDATETIME()),
    UpdatedAt       DATETIME2       NULL,
    DeletedAt       DATETIME2       NULL,
    CONSTRAINT PK_IndicatorType PRIMARY KEY (IndicatorTypeId)
);
GO

CREATE TABLE [dbo].[UserHealthIndicatorMeasure]
(
    MeasureId       INT             IDENTITY(1,1) NOT NULL,
    UserId          INT             NOT NULL,
    IndicatorTypeId INT             NOT NULL,
    Value           DECIMAL(6,2)    NOT NULL,
    SecondaryValue  DECIMAL(6,2)    NOT NULL,
    MeasureDate     DATETIME2       NOT NULL,
    Notes           NVARCHAR(500)   NULL,
    IsAbnormal      BIT             NOT NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT (SYSUTCDATETIME()),
    UpdatedAt       DATETIME2       NULL,
    DeletedAt       DATETIME2       NULL,
    CONSTRAINT PK_Measure PRIMARY KEY (MeasureId),
    CONSTRAINT FK_Measure_User FOREIGN KEY (UserId) REFERENCES [dbo].[User](UserId),
    CONSTRAINT FK_Measure_IndicatorType FOREIGN KEY (IndicatorTypeId) REFERENCES [dbo].[HealthIndicatorType](IndicatorTypeId)
);
GO

CREATE TABLE [dbo].[Alert]
(
    AlertId     INT             IDENTITY(1,1) NOT NULL,
    UserId      INT             NOT NULL,
    MeasureId   INT             NOT NULL,
    Message     NVARCHAR(500)   NOT NULL,
    ReadedAt    DATETIME2       NULL,
    CreatedAt   DATETIME2       NOT NULL DEFAULT (SYSUTCDATETIME()),
    UpdatedAt   DATETIME2       NULL,
    DeletedAt   DATETIME2       NULL,
    CONSTRAINT PK_Alert PRIMARY KEY (AlertId),
    CONSTRAINT FK_Alert_User FOREIGN KEY (UserId) REFERENCES [dbo].[User](UserId),
    CONSTRAINT FK_Alert_Measure FOREIGN KEY (MeasureId) REFERENCES [dbo].[UserHealthIndicatorMeasure](MeasureId)
);
GO

CREATE TABLE [dbo].[Objective]
(
    ObjectiveId     INT             IDENTITY(1,1) NOT NULL,
    UserId          INT             NOT NULL,
    IndicatorTypeId INT             NOT NULL,
    Title           NVARCHAR(150)   NOT NULL,
    Description     NVARCHAR(500)   NULL,
    InitialValue    DECIMAL(6,2)    NULL,
    ObjectiveValue  DECIMAL(6,2)    NULL,
    StartDate       DATETIME2       NOT NULL,
    LimitDate       DATETIME2       NULL,
    Status          NVARCHAR(20)    NOT NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT (SYSUTCDATETIME()),
    UpdatedAt       DATETIME2       NULL,
    DeletedAt       DATETIME2       NULL,
    CONSTRAINT PK_Objective PRIMARY KEY (ObjectiveId),
    CONSTRAINT FK_Objective_User FOREIGN KEY (UserId) REFERENCES [dbo].[User](UserId),
    CONSTRAINT FK_Objective_IndicatorType FOREIGN KEY (IndicatorTypeId) REFERENCES [dbo].[HealthIndicatorType](IndicatorTypeId)
);
GO

CREATE TABLE [dbo].[ObjectiveProgress]
(
    ObjectiveProgressId INT             IDENTITY(1,1) NOT NULL,
    ObjectiveId         INT             NOT NULL,
    Date                DATETIME2       NOT NULL,
    CurrentValue        DECIMAL(6,2)    NOT NULL,
    ComplianceRate      DECIMAL(5,2)    NOT NULL,
    CreatedAt           DATETIME2       NOT NULL DEFAULT (SYSUTCDATETIME()),
    UpdatedAt           DATETIME2       NULL,
    DeletedAt           DATETIME2       NULL,
    CONSTRAINT PK_ObjectiveProgress PRIMARY KEY (ObjectiveProgressId),
    CONSTRAINT FK_ObjectiveProgress_Objective FOREIGN KEY (ObjectiveId) REFERENCES [dbo].[Objective](ObjectiveId)
);
GO

CREATE TABLE [dbo].[ErrorLog]
(
    ErrorLogId  INT             IDENTITY(1,1) NOT NULL,
    Place       NVARCHAR(250)   NOT NULL,
    ErrorType   NVARCHAR(250)   NOT NULL,
    Message     NVARCHAR(2000)  NOT NULL,
    CONSTRAINT PK_ErrorLog PRIMARY KEY (ErrorLogId)
);
GO

CREATE TABLE [dbo].[ConsultationStatus]
(
    StatusId INT             IDENTITY(1,1) NOT NULL,
    Name     NVARCHAR(50)    NOT NULL,
    CONSTRAINT PK_ConsultationStatus PRIMARY KEY (StatusId),
    CONSTRAINT UQ_ConsultationStatus_Name UNIQUE (Name)
);
GO

SET IDENTITY_INSERT [dbo].[ConsultationStatus] ON;
INSERT INTO [dbo].[ConsultationStatus] (StatusId, Name) VALUES (1, N'Open'), (2, N'InProgress'), (3, N'Closed');
SET IDENTITY_INSERT [dbo].[ConsultationStatus] OFF;
GO

CREATE TABLE [dbo].[Consultation]
(
    ConsultationId INT             IDENTITY(1,1) NOT NULL,
    PatientUserId  INT             NOT NULL,
    DoctorUserId   INT             NULL,
    MeasureId      INT             NULL,
    Title          NVARCHAR(150)   NOT NULL,
    Description    NVARCHAR(500)   NULL,
    StatusId       INT             NOT NULL DEFAULT (1),
    CreatedAt      DATETIME2       NOT NULL DEFAULT (SYSUTCDATETIME()),
    ClosedAt       DATETIME2       NULL,
    CONSTRAINT PK_Consultation PRIMARY KEY (ConsultationId),
    CONSTRAINT FK_Consultation_Patient FOREIGN KEY (PatientUserId) REFERENCES [dbo].[User](UserId),
    CONSTRAINT FK_Consultation_Doctor FOREIGN KEY (DoctorUserId) REFERENCES [dbo].[User](UserId),
    CONSTRAINT FK_Consultation_Measure FOREIGN KEY (MeasureId) REFERENCES [dbo].[UserHealthIndicatorMeasure](MeasureId),
    CONSTRAINT FK_Consultation_Status FOREIGN KEY (StatusId) REFERENCES [dbo].[ConsultationStatus](StatusId)
);
GO

CREATE TABLE [dbo].[Message]
(
    MessageId      INT             IDENTITY(1,1) NOT NULL,
    ConsultationId INT             NOT NULL,
    SenderUserId   INT             NOT NULL,
    Content        NVARCHAR(2000)  NOT NULL,
    SentAt         DATETIME2       NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Message PRIMARY KEY (MessageId),
    CONSTRAINT FK_Message_Consultation FOREIGN KEY (ConsultationId) REFERENCES [dbo].[Consultation](ConsultationId),
    CONSTRAINT FK_Message_Sender FOREIGN KEY (SenderUserId) REFERENCES [dbo].[User](UserId)
);
GO
