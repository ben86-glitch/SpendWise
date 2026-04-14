IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Transactions] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [TransactionCategory] int NOT NULL,
    [Date] datetime2 NOT NULL,
    [BillingCycle] int NULL,
    [TransactionType] nvarchar(13) NOT NULL,
    CONSTRAINT [PK_Transactions] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260410064052_InitialCreate', N'10.0.5');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Transactions] ADD [PaymentMethod] int NOT NULL DEFAULT 0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260414063022_AddPaymentMethodToTransactions', N'10.0.5');

COMMIT;
GO

