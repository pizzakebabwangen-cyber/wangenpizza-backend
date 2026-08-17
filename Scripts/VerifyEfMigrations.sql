/*
  Use this AFTER dotnet ef database update (or app startup Migrate) to verify schema history.

  Expected (from repo): last row in __EFMigrationsHistory should match the newest migration in the
  WangenPizza\Migrations folder — e.g. migration ending in _testDB (timestamp 20250927012508) or newer
  if you add migrations later.

  If this table is missing: migrations never ran on this database (or not EF-managed).
  If the last MigrationId is OLD but the app is NEW: pending migrations may still exist → run update again.
*/

-- USE [your_catalog_name];
-- GO

IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
BEGIN
    SELECT N'__EFMigrationsHistory missing — database may not be under EF migrations yet.' AS Result;
END
ELSE
BEGIN
    SELECT COUNT(*) AS AppliedMigrationCount FROM dbo.__EFMigrationsHistory;

    SELECT TOP 30 MigrationId, ProductVersion
    FROM dbo.__EFMigrationsHistory
    ORDER BY MigrationId DESC;
END
