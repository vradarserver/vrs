IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Aircraft_GetOrCreate')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Aircraft_GetOrCreate] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Aircraft_GetOrCreate]
    @Created        BIT OUTPUT
   ,@ModeS          VARCHAR(6)
   ,@LocalNow       DATETIME = NULL
   ,@ModeSCountry   NVARCHAR(24) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @LocalNow = ISNULL(@LocalNow, GETDATE());

    INSERT INTO [BaseStation].[Aircraft] (
        [ModeS]
       ,[FirstCreated]
       ,[LastModified]
       ,[ModeSCountry]
    )
    SELECT @ModeS
          ,@LocalNow
          ,@LocalNow
          ,@ModeSCountry
    WHERE NOT EXISTS (
        SELECT 1
        FROM   [BaseStation].[Aircraft]
        WHERE  [ModeS] = @ModeS
    );
    SET @Created = CASE WHEN @@ROWCOUNT = 0 THEN 0 ELSE 1 END;

    SELECT *
    FROM   [BaseStation].[Aircraft]
    WHERE  [ModeS] = @ModeS;
END;
GO
