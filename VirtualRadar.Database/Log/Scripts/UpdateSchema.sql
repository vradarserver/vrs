--
-- Client
--
CREATE TABLE IF NOT EXISTS [Client]
(
    [Id]                INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
   ,[IpAddress]         TEXT NOT NULL
   ,[ReverseDns]        TEXT
   ,[ReverseDnsDate]    DATETIME
);
CREATE UNIQUE INDEX IF NOT EXISTS [Idx_Client_IpAddress] ON [Client] ([IpAddress] ASC);


--
-- Session
--
CREATE TABLE IF NOT EXISTS [Session]
(
    [Id]                INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
   ,[ClientId]          INTEGER NOT NULL
   ,[StartTime]         DATETIME
   ,[EndTime]           DATETIME
   ,[CountRequests]     INTEGER
   ,[OtherBytesSent]    INTEGER
   ,[HtmlBytesSent]     INTEGER
   ,[JsonBytesSent]     INTEGER
   ,[ImageBytesSent]    INTEGER
   ,[AudioBytesSent]    INTEGER
);

CREATE INDEX IF NOT EXISTS [Idx_Session_ClientId]   ON [Session] ([ClientId] ASC);
CREATE INDEX IF NOT EXISTS [Idx_Session_StartTime]  ON [Session] ([StartTime] ASC);
CREATE INDEX IF NOT EXISTS [Idx_Session_EndTime]    ON [Session] ([EndTime] ASC);
