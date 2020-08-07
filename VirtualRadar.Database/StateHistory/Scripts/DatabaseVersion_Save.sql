INSERT INTO [DatabaseVersion] (
    [DatabaseVersionID]
   ,[CreatedUtc]
) VALUES (
    @DatabaseVersionID
   ,@CreatedUtc
)
ON    CONFLICT ([DatabaseVersionID])
DO    UPDATE
SET   [CreatedUtc] = @CreatedUtc;
