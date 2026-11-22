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
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122050411_InitialCreate'
)
BEGIN
    CREATE TABLE [Departments] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Location] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Departments] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122050411_InitialCreate'
)
BEGIN
    CREATE TABLE [Employees] (
        [Id] int NOT NULL IDENTITY,
        [FirstName] nvarchar(50) NOT NULL,
        [LastName] nvarchar(50) NOT NULL,
        [Email] nvarchar(254) NOT NULL,
        [Phone] nvarchar(20) NULL,
        [JobTitle] nvarchar(100) NOT NULL,
        [HireDate] date NOT NULL,
        [MonthlySalary] decimal(10,2) NOT NULL,
        [Status] nvarchar(12) NOT NULL,
        [DepartmentId] int NOT NULL,
        [ManagerId] int NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_Employees] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Employees_NotOwnManager] CHECK ([ManagerId] IS NULL OR [ManagerId] <> [Id]),
        CONSTRAINT [CK_Employees_Salary] CHECK ([MonthlySalary] > 0),
        CONSTRAINT [FK_Employees_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Employees_Employees_ManagerId] FOREIGN KEY ([ManagerId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122050411_InitialCreate'
)
BEGIN
    CREATE TABLE [Projects] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(12) NOT NULL,
        [Name] nvarchar(120) NOT NULL,
        [StartDate] date NOT NULL,
        [EndDate] date NULL,
        [DepartmentId] int NOT NULL,
        CONSTRAINT [PK_Projects] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Projects_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122050411_InitialCreate'
)
BEGIN
    CREATE TABLE [ProjectAssignments] (
        [EmployeeId] int NOT NULL,
        [ProjectId] int NOT NULL,
        [Role] nvarchar(60) NOT NULL,
        [AllocationPercent] int NOT NULL,
        CONSTRAINT [PK_ProjectAssignments] PRIMARY KEY ([EmployeeId], [ProjectId]),
        CONSTRAINT [CK_ProjectAssignments_Allocation] CHECK ([AllocationPercent] BETWEEN 1 AND 100),
        CONSTRAINT [FK_ProjectAssignments_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ProjectAssignments_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122050411_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Location', N'Name') AND [object_id] = OBJECT_ID(N'[Departments]'))
        SET IDENTITY_INSERT [Departments] ON;
    EXEC(N'INSERT INTO [Departments] ([Id], [Location], [Name])
    VALUES (1, N''Dubai'', N''Engineering''),
    (2, N''Dubai'', N''Finance''),
    (3, N''Abu Dhabi'', N''Human Resources''),
    (4, N''Sharjah'', N''Customer Support'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Location', N'Name') AND [object_id] = OBJECT_ID(N'[Departments]'))
        SET IDENTITY_INSERT [Departments] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122050411_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'DepartmentId', N'Email', N'FirstName', N'HireDate', N'JobTitle', N'LastName', N'ManagerId', N'MonthlySalary', N'Phone', N'Status', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[Employees]'))
        SET IDENTITY_INSERT [Employees] ON;
    EXEC(N'INSERT INTO [Employees] ([Id], [CreatedAtUtc], [DepartmentId], [Email], [FirstName], [HireDate], [JobTitle], [LastName], [ManagerId], [MonthlySalary], [Phone], [Status], [UpdatedAtUtc])
    VALUES (1, ''2026-01-01T00:00:00.0000000Z'', 1, N''aisha.almansoori@example.com'', N''Aisha'', ''2018-03-11'', N''Head of Engineering'', N''Al Mansoori'', NULL, 38000.0, NULL, N''Active'', ''2026-01-01T00:00:00.0000000Z''),
    (5, ''2026-01-01T00:00:00.0000000Z'', 2, N''omar.khan@example.com'', N''Omar'', ''2019-09-02'', N''Finance Manager'', N''Khan'', NULL, 30000.0, NULL, N''Active'', ''2026-01-01T00:00:00.0000000Z''),
    (7, ''2026-01-01T00:00:00.0000000Z'', 3, N''mariam.saleh@example.com'', N''Mariam'', ''2017-01-16'', N''HR Manager'', N''Saleh'', NULL, 28000.0, NULL, N''Active'', ''2026-01-01T00:00:00.0000000Z''),
    (8, ''2026-01-01T00:00:00.0000000Z'', 4, N''yusuf.rahman@example.com'', N''Yusuf'', ''2021-04-12'', N''Support Team Lead'', N''Rahman'', NULL, 16000.0, NULL, N''Active'', ''2026-01-01T00:00:00.0000000Z'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'DepartmentId', N'Email', N'FirstName', N'HireDate', N'JobTitle', N'LastName', N'ManagerId', N'MonthlySalary', N'Phone', N'Status', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[Employees]'))
        SET IDENTITY_INSERT [Employees] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122050411_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'DepartmentId', N'EndDate', N'Name', N'StartDate') AND [object_id] = OBJECT_ID(N'[Projects]'))
        SET IDENTITY_INSERT [Projects] ON;
    EXEC(N'INSERT INTO [Projects] ([Id], [Code], [DepartmentId], [EndDate], [Name], [StartDate])
    VALUES (1, N''ENG-PORTAL'', 1, NULL, N''Customer self-service portal'', ''2025-02-01''),
    (2, N''FIN-CLOSE'', 2, ''2025-09-30'', N''Faster month-end close'', ''2025-01-10''),
    (3, N''SUP-KB'', 4, NULL, N''Support knowledge base'', ''2025-03-01'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'DepartmentId', N'EndDate', N'Name', N'StartDate') AND [object_id] = OBJECT_ID(N'[Projects]'))
        SET IDENTITY_INSERT [Projects] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122050411_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'DepartmentId', N'Email', N'FirstName', N'HireDate', N'JobTitle', N'LastName', N'ManagerId', N'MonthlySalary', N'Phone', N'Status', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[Employees]'))
        SET IDENTITY_INSERT [Employees] ON;
    EXEC(N'INSERT INTO [Employees] ([Id], [CreatedAtUtc], [DepartmentId], [Email], [FirstName], [HireDate], [JobTitle], [LastName], [ManagerId], [MonthlySalary], [Phone], [Status], [UpdatedAtUtc])
    VALUES (2, ''2026-01-01T00:00:00.0000000Z'', 1, N''rahul.nair@example.com'', N''Rahul'', ''2020-07-01'', N''Senior Software Engineer'', N''Nair'', 1, 24000.0, NULL, N''Active'', ''2026-01-01T00:00:00.0000000Z''),
    (6, ''2026-01-01T00:00:00.0000000Z'', 2, N''priya.sharma@example.com'', N''Priya'', ''2022-11-07'', N''Financial Analyst'', N''Sharma'', 5, 14000.0, NULL, N''Active'', ''2026-01-01T00:00:00.0000000Z''),
    (9, ''2026-01-01T00:00:00.0000000Z'', 4, N''neha.menon@example.com'', N''Neha'', ''2025-01-06'', N''Support Agent'', N''Menon'', 8, 7500.0, NULL, N''Active'', ''2026-01-01T00:00:00.0000000Z''),
    (3, ''2026-01-01T00:00:00.0000000Z'', 1, N''sara.haddad@example.com'', N''Sara'', ''2023-02-15'', N''Software Engineer'', N''Haddad'', 2, 15500.0, NULL, N''Active'', ''2026-01-01T00:00:00.0000000Z''),
    (4, ''2026-01-01T00:00:00.0000000Z'', 1, N''karthik.iyer@example.com'', N''Karthik'', ''2024-05-20'', N''QA Engineer'', N''Iyer'', 2, 12500.0, NULL, N''Active'', ''2026-01-01T00:00:00.0000000Z'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'DepartmentId', N'Email', N'FirstName', N'HireDate', N'JobTitle', N'LastName', N'ManagerId', N'MonthlySalary', N'Phone', N'Status', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[Employees]'))
        SET IDENTITY_INSERT [Employees] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122050411_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EmployeeId', N'ProjectId', N'AllocationPercent', N'Role') AND [object_id] = OBJECT_ID(N'[ProjectAssignments]'))
        SET IDENTITY_INSERT [ProjectAssignments] ON;
    EXEC(N'INSERT INTO [ProjectAssignments] ([EmployeeId], [ProjectId], [AllocationPercent], [Role])
    VALUES (2, 1, 60, N''Tech lead''),
    (6, 2, 40, N''Analyst''),
    (9, 3, 25, N''Content writer''),
    (3, 1, 100, N''Developer''),
    (4, 1, 50, N''Tester'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EmployeeId', N'ProjectId', N'AllocationPercent', N'Role') AND [object_id] = OBJECT_ID(N'[ProjectAssignments]'))
        SET IDENTITY_INSERT [ProjectAssignments] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122050411_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Departments_Name] ON [Departments] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122050411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Employees_DepartmentId] ON [Employees] ([DepartmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122050411_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Employees_Email] ON [Employees] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122050411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Employees_LastName_FirstName] ON [Employees] ([LastName], [FirstName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122050411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Employees_ManagerId] ON [Employees] ([ManagerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122050411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ProjectAssignments_ProjectId] ON [ProjectAssignments] ([ProjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122050411_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Projects_Code] ON [Projects] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122050411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Projects_DepartmentId] ON [Projects] ([DepartmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122050411_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261122050411_InitialCreate', N'8.0.11');
END;
GO

COMMIT;
GO

