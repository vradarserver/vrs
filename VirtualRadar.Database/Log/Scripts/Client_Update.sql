UPDATE [Client]
   SET [IpAddress] =        @ipAddress
      ,[ReverseDns] =       @reverseDns
      ,[ReverseDnsDate] =   @reverseDnsDate
 WHERE [Id] = @id;
