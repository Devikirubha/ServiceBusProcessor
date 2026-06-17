-- create_serilog_logs.sql
-- Creates a Serilog Logs table compatible with Serilog.Sinks.MSSqlServer defaults
-- Run as a DB admin/owner. Adjust schema or column types if you use custom ColumnOptions.

CREATE TABLE [dbo].[Logs] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Message] NVARCHAR(MAX) NULL,
    [MessageTemplate] NVARCHAR(MAX) NULL,
    [Level] NVARCHAR(128) NULL,
    [TimeStamp] DATETIMEOFFSET NOT NULL,
    [Exception] NVARCHAR(MAX) NULL,
    [Properties] NVARCHAR(MAX) NULL,
    [LogEvent] VARBINARY(MAX) NULL
);

CREATE INDEX IX_Logs_TimeStamp ON [dbo].[Logs]([TimeStamp]);

-- If you add custom columns via ColumnOptions (e.g. CorrelationId), add them here:
-- ALTER TABLE [dbo].[Logs] ADD [CorrelationId] NVARCHAR(50) NULL;
