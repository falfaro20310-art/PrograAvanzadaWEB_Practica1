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

DROP PROCEDURE IF EXISTS [dbo].[spCreateConsultation];
GO
CREATE PROCEDURE [dbo].[spCreateConsultation]
    @PatientUserId INT,
    @Title         NVARCHAR(150),
    @Description   NVARCHAR(500),
    @MeasureId     INT
AS
BEGIN

    INSERT INTO [dbo].[Consultation] (PatientUserId, Title, Description, MeasureId, StatusId, CreatedAt)
    VALUES (@PatientUserId, @Title, @Description, @MeasureId, 1, SYSUTCDATETIME())

    SELECT CAST(SCOPE_IDENTITY() AS INT)

END
GO

DROP PROCEDURE IF EXISTS [dbo].[spTakeConsultation];
GO
CREATE PROCEDURE [dbo].[spTakeConsultation]
    @ConsultationId INT,
    @DoctorUserId   INT
AS
BEGIN

    UPDATE  [dbo].[Consultation]
    SET     DoctorUserId = @DoctorUserId,
            StatusId     = 2
    WHERE   ConsultationId = @ConsultationId
        AND DoctorUserId IS NULL
        AND StatusId = 1

END
GO

DROP PROCEDURE IF EXISTS [dbo].[spGetConsultations];
GO
CREATE PROCEDURE [dbo].[spGetConsultations]
    @UserId INT,
    @RoleId INT
AS
BEGIN

    IF (@RoleId = 1)
    BEGIN
        -- Paciente: sus propias consultas, con el nombre del doctor asignado
        SELECT  C.ConsultationId,
                C.Title,
                C.Description,
                C.StatusId,
                S.Name 'StatusName',
                ISNULL(DP.Name + ' ' + DP.FirstName, 'Sin asignar') 'InterlocutorName',
                C.MeasureId,
                HIT.Name 'MeasureIndicator',
                M.Value 'MeasureValue',
                HIT.Unit 'MeasureUnit',
                M.MeasureDate 'MeasureDate',
                M.IsAbnormal 'MeasureIsAbnormal'
        FROM    [dbo].[Consultation] C
        INNER JOIN [dbo].[ConsultationStatus] S ON C.StatusId = S.StatusId
        LEFT JOIN [dbo].[Profile] DP ON C.DoctorUserId = DP.UserId
        LEFT JOIN [dbo].[UserHealthIndicatorMeasure] M ON C.MeasureId = M.MeasureId
        LEFT JOIN [dbo].[HealthIndicatorType] HIT ON M.IndicatorTypeId = HIT.IndicatorTypeId
        WHERE   C.PatientUserId = @UserId
        ORDER BY C.CreatedAt DESC
    END
    ELSE
    BEGIN
        -- Doctor: consultas abiertas sin asignar o asignadas a el
        SELECT  C.ConsultationId,
                C.Title,
                C.Description,
                C.StatusId,
                S.Name 'StatusName',
                PP.Name + ' ' + PP.FirstName 'InterlocutorName',
                C.MeasureId,
                HIT.Name 'MeasureIndicator',
                M.Value 'MeasureValue',
                HIT.Unit 'MeasureUnit',
                M.MeasureDate 'MeasureDate',
                M.IsAbnormal 'MeasureIsAbnormal'
        FROM    [dbo].[Consultation] C
        INNER JOIN [dbo].[ConsultationStatus] S ON C.StatusId = S.StatusId
        INNER JOIN [dbo].[Profile] PP ON C.PatientUserId = PP.UserId
        LEFT JOIN [dbo].[UserHealthIndicatorMeasure] M ON C.MeasureId = M.MeasureId
        LEFT JOIN [dbo].[HealthIndicatorType] HIT ON M.IndicatorTypeId = HIT.IndicatorTypeId
        WHERE   (C.DoctorUserId IS NULL AND C.StatusId = 1)
            OR  C.DoctorUserId = @UserId
        ORDER BY C.CreatedAt DESC
    END

END
GO

DROP PROCEDURE IF EXISTS [dbo].[spGetMessages];
GO
CREATE PROCEDURE [dbo].[spGetMessages]
    @ConsultationId INT
AS
BEGIN

    SELECT  M.MessageId,
            M.ConsultationId,
            M.SenderUserId,
            M.Content,
            M.SentAt,
            SP.Name + ' ' + SP.FirstName 'SenderName'
    FROM    [dbo].[Message] M
    INNER JOIN [dbo].[Profile] SP ON M.SenderUserId = SP.UserId
    WHERE   M.ConsultationId = @ConsultationId
    ORDER BY M.SentAt

END
GO

DROP PROCEDURE IF EXISTS [dbo].[spRegisterMessage];
GO
CREATE PROCEDURE [dbo].[spRegisterMessage]
    @ConsultationId INT,
    @SenderUserId   INT,
    @Content        NVARCHAR(2000)
AS
BEGIN

    INSERT INTO [dbo].[Message] (ConsultationId, SenderUserId, Content, SentAt)
    VALUES (@ConsultationId, @SenderUserId, @Content, SYSUTCDATETIME())

    SELECT CAST(SCOPE_IDENTITY() AS INT)

END
GO

DROP PROCEDURE IF EXISTS [dbo].[spValidateConsultationAccess];
GO
CREATE PROCEDURE [dbo].[spValidateConsultationAccess]
    @ConsultationId INT,
    @UserId         INT
AS
BEGIN

    SELECT  COUNT(1)
    FROM    [dbo].[Consultation]
    WHERE   ConsultationId = @ConsultationId
        AND (PatientUserId = @UserId OR DoctorUserId = @UserId)

END
GO

DROP PROCEDURE IF EXISTS [dbo].[spCloseConsultation];
GO
CREATE PROCEDURE [dbo].[spCloseConsultation]
    @ConsultationId INT,
    @UserId         INT
AS
BEGIN

    UPDATE  [dbo].[Consultation]
    SET     StatusId = 3,
            ClosedAt = SYSUTCDATETIME()
    WHERE   ConsultationId = @ConsultationId
        AND (PatientUserId = @UserId OR DoctorUserId = @UserId)
        AND StatusId <> 3

END
GO
