/*
  Pizza Wangen — Production fix for admin dashboard / statistics.
  Error: Invalid column name 'AppliedGutscheinCode'

  Run against the SAME database as the live app (backup first).
  Safe to run once; skips columns that already exist.
*/

SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    RAISERROR(N'Table dbo.Orders not found. Check database / schema.', 16, 1);
    RETURN;
END

IF COL_LENGTH(N'dbo.Orders', N'AppliedGutscheinCode') IS NULL
    ALTER TABLE dbo.Orders ADD AppliedGutscheinCode NVARCHAR(MAX) NULL;

IF COL_LENGTH(N'dbo.Orders', N'GutscheinDeduction') IS NULL
    ALTER TABLE dbo.Orders ADD GutscheinDeduction DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_Orders_GutscheinDeduction DEFAULT (0);

-- So future `dotnet ef database update` does not try to add the same columns again:
IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1
        FROM dbo.__EFMigrationsHistory
        WHERE MigrationId = N'20260411005841_OrderGutscheinDeductionFields')
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (N'20260411005841_OrderGutscheinDeductionFields', N'6.0.29');

PRINT N'Done: Orders.AppliedGutscheinCode + GutscheinDeduction + migration history (if applicable).';
