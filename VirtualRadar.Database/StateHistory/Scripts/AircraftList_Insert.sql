INSERT INTO [AircraftList] (
    [VrsSessionID]
   ,[IsKeyList]
   ,[ReceiverSnapshotID]
   ,[CreatedUtc]
   ,[UpdatedUtc]
) VALUES (
    @VrsSessionID
   ,@IsKeyList
   ,@ReceiverSnapshotID
   ,@CreatedUtc
   ,@UpdatedUtc
);
SELECT last_insert_rowid();
