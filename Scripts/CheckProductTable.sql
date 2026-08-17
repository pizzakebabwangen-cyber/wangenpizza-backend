/*
  Menüprodukte = admin UI name. Entity Framework maps to table: dbo.Product

  BEFORE RUNNING:
  1) Connect SSMS / Azure Data Studio to the SAME server + database your LIVE site uses.
  2) Repo appsettings.json (key "DefultConnection") currently points to:
        Data Source = SQL8020.site4now.net
        Initial Catalog = db_aac3f3_wangen2024
     Production often overrides this in hosting panel or appsettings.Production.json — VERIFY.
  3) If the script shows 0 rows here but you expected data, you may be on the wrong catalog
     or the data was removed on this instance (see notes at bottom).
*/

-- Optional: uncomment and set to your actual production database name
-- USE [db_aac3f3_wangen2024];
-- GO

SET NOCOUNT ON;

/* 1) Does the Product table exist? */
IF OBJECT_ID(N'dbo.Product', N'U') IS NULL
BEGIN
    SELECT N'ERROR: dbo.Product does not exist on this database.' AS Message;
    RETURN;
END

/* 2) Row count (main check) */
SELECT COUNT(*) AS ProductRowCount
FROM dbo.[Product];

/* 3) Quick peek at newest rows (if any) */
SELECT TOP 25
    p.[Id],
    p.[Name],
    p.[ProductType]
FROM dbo.[Product] AS p
ORDER BY p.[Id] DESC;

/* 4) Orphan check: order lines pointing to missing products (often after deletes) */
IF OBJECT_ID(N'dbo.OrderItem', N'U') IS NOT NULL
BEGIN
    SELECT COUNT(*) AS OrderItems_WhereProductMissing
    FROM dbo.[OrderItem] AS oi
    LEFT JOIN dbo.[Product] AS p ON p.[Id] = oi.[ProductId]
    WHERE p.[Id] IS NULL;
END

/* 5) Last applied EF migrations (did schema change recently?) */
IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NOT NULL
BEGIN
    SELECT TOP 20
        [MigrationId],
        [ProductVersion]
    FROM dbo.[__EFMigrationsHistory]
    ORDER BY [MigrationId] DESC;
END
ELSE
    SELECT N'No __EFMigrationsHistory table (not using EF migrations on this DB or different schema).' AS Note;

/*
  WHAT COULD EMPTY Product (no single migration in this repo clears Product data):

  - Wrong database: app or SSMS connected to empty/staging DB instead of production.
  - Hosting override: connection string on server differs from appsettings.json in the repo.
  - Manual SQL: DELETE / TRUNCATE on Product (or restore of a DB backup taken when empty).
  - Full database restore / copy from template that had no menu rows.
  - Rare: destructive script run outside the app; EF Migrate() does not remove business rows
    unless a migration explicitly deletes them (none in current WangenPizza migrations for Product data).

  This script only INSPECTS; it does not modify data.
*/
