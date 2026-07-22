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
