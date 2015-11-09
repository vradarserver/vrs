UPDATE [SystemEvents]
   SET [TimeStamp]      = @timeStamp
      ,[App]            = @app
      ,[Msg]            = @msg
 WHERE [SystemEventsID] = @systemEventsID;
