USE [vitalapp_];
GO

DROP PROCEDURE IF EXISTS [dbo].[spRegisterUser];
GO

CREATE PROCEDURE [dbo].[spRegisterUser]
    @Email      NVARCHAR(200),
    @Password   NVARCHAR(500),
    @IdCard     NVARCHAR(50),
    @Name       NVARCHAR(250),
    @FirstName  NVARCHAR(250),
    @LastName   NVARCHAR(250)
AS
BEGIN

    IF NOT EXISTS (SELECT 1 FROM [dbo].[User] WHERE Email = @Email)
        AND NOT EXISTS (SELECT 1 FROM [dbo].[Profile] WHERE IdCard = @IdCard)
    BEGIN

        DECLARE @UserId INT

        INSERT INTO [dbo].[User] (Email, Password, IsActive, CreatedAt)
        VALUES (@Email, @Password, 1, SYSUTCDATETIME())

        SET @UserId = SCOPE_IDENTITY()

        INSERT INTO [dbo].[Profile] (UserId, IdCard, Name, FirstName, LastName, CreatedAt)
        VALUES (@UserId, @IdCard, @Name, @FirstName, @LastName, SYSUTCDATETIME())

    END

END
GO

DROP PROCEDURE IF EXISTS [dbo].[spLoginUser];
GO

CREATE PROCEDURE [dbo].[spLoginUser]
    @Email NVARCHAR(200)
AS
BEGIN

    SELECT  U.UserId,
            U.Email,
            U.Password,
            U.IsActive,
            U.RoleId,
            R.Name 'RoleName',
            P.IdCard,
            P.Name,
            P.FirstName,
            P.LastName
    FROM    [dbo].[User] U
    INNER JOIN [dbo].[Profile] P ON U.UserId = P.UserId
    INNER JOIN [dbo].[Role] R ON U.RoleId = R.RoleId
    WHERE   U.Email = @Email
        AND U.IsActive = 1
        AND U.DeletedAt IS NULL

END
GO

DROP PROCEDURE IF EXISTS [dbo].[spValidateEmail];
GO

CREATE PROCEDURE [dbo].[spValidateEmail]
    @Email NVARCHAR(200)
AS
BEGIN

    SELECT  U.UserId,
            U.Email,
            U.IsActive,
            P.IdCard,
            P.Name,
            P.FirstName,
            P.LastName
    FROM    [dbo].[User] U
    INNER JOIN [dbo].[Profile] P ON U.UserId = P.UserId
    WHERE   U.Email = @Email
        AND U.IsActive = 1
        AND U.DeletedAt IS NULL

END
GO

DROP PROCEDURE IF EXISTS [dbo].[spUpdatePassword];
GO

CREATE PROCEDURE [dbo].[spUpdatePassword]
    @UserId   INT,
    @Password NVARCHAR(500)
AS
BEGIN

    UPDATE  [dbo].[User]
    SET     Password  = @Password,
            UpdatedAt = SYSUTCDATETIME()
    WHERE   UserId = @UserId

END
GO

DROP PROCEDURE IF EXISTS [dbo].[spGetProfile];
GO

CREATE PROCEDURE [dbo].[spGetProfile]
    @UserId INT
AS
BEGIN

    SELECT  U.UserId,
            U.Email,
            P.ProfileId,
            P.IdCard,
            P.Name,
            P.FirstName,
            P.LastName,
            P.BirthDate,
            P.Gender,
            P.Height,
            P.Weight
    FROM    [dbo].[User] U
    INNER JOIN [dbo].[Profile] P ON U.UserId = P.UserId
    WHERE   U.UserId = @UserId
        AND U.DeletedAt IS NULL

END
GO

DROP PROCEDURE IF EXISTS [dbo].[spUpdateProfile];
GO

CREATE PROCEDURE [dbo].[spUpdateProfile]
    @UserId     INT,
    @Name       NVARCHAR(250),
    @FirstName  NVARCHAR(250),
    @LastName   NVARCHAR(250),
    @BirthDate  DATETIME2,
    @Gender     NVARCHAR(50),
    @Height     DECIMAL(5,2),
    @Weight     DECIMAL(5,2)
AS
BEGIN

    UPDATE  [dbo].[Profile]
    SET     Name      = @Name,
            FirstName = @FirstName,
            LastName  = @LastName,
            BirthDate = @BirthDate,
            Gender    = @Gender,
            Height    = @Height,
            Weight    = @Weight,
            UpdatedAt = SYSUTCDATETIME()
    WHERE   UserId = @UserId

END
GO

DROP PROCEDURE IF EXISTS [dbo].[spGetMedicalConditions];
GO

CREATE PROCEDURE [dbo].[spGetMedicalConditions]
    @UserId INT
AS
BEGIN

    SELECT  MedicalConditionId,
            UserId,
            Name,
            Description,
            DiagnosticDate
    FROM    [dbo].[MedicalCondition]
    WHERE   UserId = @UserId
        AND DeletedAt IS NULL
    ORDER BY DiagnosticDate DESC

END
GO

DROP PROCEDURE IF EXISTS [dbo].[spRegisterMedicalCondition];
GO

CREATE PROCEDURE [dbo].[spRegisterMedicalCondition]
    @UserId         INT,
    @Name           NVARCHAR(250),
    @Description    NVARCHAR(250),
    @DiagnosticDate DATETIME2
AS
BEGIN

    INSERT INTO [dbo].[MedicalCondition] (UserId, Name, Description, DiagnosticDate, CreatedAt)
    VALUES (@UserId, @Name, @Description, @DiagnosticDate, SYSUTCDATETIME())

END
GO

DROP PROCEDURE IF EXISTS [dbo].[spDeleteMedicalCondition];
GO

CREATE PROCEDURE [dbo].[spDeleteMedicalCondition]
    @MedicalConditionId INT,
    @UserId             INT
AS
BEGIN

    UPDATE  [dbo].[MedicalCondition]
    SET     DeletedAt = SYSUTCDATETIME()
    WHERE   MedicalConditionId = @MedicalConditionId
        AND UserId = @UserId

END
GO

DROP PROCEDURE IF EXISTS [dbo].[spGetAllPatients];
GO

CREATE PROCEDURE [dbo].[spGetAllPatients]
AS
BEGIN

    SELECT  U.UserId,
            U.Email,
            U.IsActive,
            U.RoleId,
            R.Name 'RoleName',
            P.IdCard,
            P.Name,
            P.FirstName,
            P.LastName
    FROM    [dbo].[User] U
    INNER JOIN [dbo].[Profile] P ON U.UserId = P.UserId
    INNER JOIN [dbo].[Role] R ON U.RoleId = R.RoleId
    WHERE   U.DeletedAt IS NULL
        AND U.RoleId = 1
    ORDER BY P.Name, P.FirstName

END
GO

DROP PROCEDURE IF EXISTS [dbo].[spUpdateUserRole];
GO

CREATE PROCEDURE [dbo].[spUpdateUserRole]
    @UserId INT,
    @RoleId INT
AS
BEGIN

    UPDATE  [dbo].[User]
    SET     RoleId    = @RoleId,
            UpdatedAt = SYSUTCDATETIME()
    WHERE   UserId = @UserId

END
GO
