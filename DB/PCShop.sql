USE [master]
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'PCShop')
BEGIN
    CREATE DATABASE [PCShop]
END
GO

USE [PCShop]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'dbo.FK_dbo.tbOrderDetail_dbo.Product_productID', 'F') IS NOT NULL
    ALTER TABLE dbo.tbOrderDetail DROP CONSTRAINT [FK_dbo.tbOrderDetail_dbo.Product_productID]
GO
IF OBJECT_ID(N'dbo.FK_dbo.tbOrderDetail_dbo.tbOrder_orderID', 'F') IS NOT NULL
    ALTER TABLE dbo.tbOrderDetail DROP CONSTRAINT [FK_dbo.tbOrderDetail_dbo.tbOrder_orderID]
GO
IF OBJECT_ID(N'dbo.FK_dbo.ProductImage_dbo.Product_ProductId', 'F') IS NOT NULL
    ALTER TABLE dbo.ProductImage DROP CONSTRAINT [FK_dbo.ProductImage_dbo.Product_ProductId]
GO
IF OBJECT_ID(N'dbo.FK_dbo.Product_dbo.Category_categoryID', 'F') IS NOT NULL
    ALTER TABLE dbo.Product DROP CONSTRAINT [FK_dbo.Product_dbo.Category_categoryID]
GO
IF OBJECT_ID(N'dbo.FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId', 'F') IS NOT NULL
    ALTER TABLE dbo.AspNetUserRoles DROP CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId]
GO
IF OBJECT_ID(N'dbo.FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId', 'F') IS NOT NULL
    ALTER TABLE dbo.AspNetUserRoles DROP CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId]
GO
IF OBJECT_ID(N'dbo.FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId', 'F') IS NOT NULL
    ALTER TABLE dbo.AspNetUserLogins DROP CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId]
GO
IF OBJECT_ID(N'dbo.FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId', 'F') IS NOT NULL
    ALTER TABLE dbo.AspNetUserClaims DROP CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId]
GO

IF OBJECT_ID(N'dbo.Adv', 'U') IS NOT NULL DROP TABLE dbo.Adv
GO
IF OBJECT_ID(N'dbo.Category', 'U') IS NOT NULL DROP TABLE dbo.Category
GO
IF OBJECT_ID(N'dbo.News', 'U') IS NOT NULL DROP TABLE dbo.News
GO
IF OBJECT_ID(N'dbo.Product', 'U') IS NOT NULL DROP TABLE dbo.Product
GO
IF OBJECT_ID(N'dbo.ProductImage', 'U') IS NOT NULL DROP TABLE dbo.ProductImage
GO
IF OBJECT_ID(N'dbo.tbOrder', 'U') IS NOT NULL DROP TABLE dbo.tbOrder
GO
IF OBJECT_ID(N'dbo.tbOrderDetail', 'U') IS NOT NULL DROP TABLE dbo.tbOrderDetail
GO
IF OBJECT_ID(N'dbo.SystemSettings', 'U') IS NOT NULL DROP TABLE dbo.SystemSettings
GO
IF OBJECT_ID(N'dbo.AspNetUserRoles', 'U') IS NOT NULL DROP TABLE dbo.AspNetUserRoles
GO
IF OBJECT_ID(N'dbo.AspNetUserClaims', 'U') IS NOT NULL DROP TABLE dbo.AspNetUserClaims
GO
IF OBJECT_ID(N'dbo.AspNetUserLogins', 'U') IS NOT NULL DROP TABLE dbo.AspNetUserLogins
GO
IF OBJECT_ID(N'dbo.AspNetRoles', 'U') IS NOT NULL DROP TABLE dbo.AspNetRoles
GO
IF OBJECT_ID(N'dbo.AspNetUsers', 'U') IS NOT NULL DROP TABLE dbo.AspNetUsers
GO

CREATE TABLE dbo.AspNetRoles (
    [Id] NVARCHAR(128) NOT NULL,
    [Name] NVARCHAR(256) NOT NULL,
    CONSTRAINT [PK_dbo.AspNetRoles] PRIMARY KEY CLUSTERED ([Id] ASC)
)
GO
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex] ON dbo.AspNetRoles([Name] ASC)
GO

CREATE TABLE dbo.AspNetUsers (
    [Id] NVARCHAR(128) NOT NULL,
    [Email] NVARCHAR(256) NULL,
    [EmailConfirmed] BIT NOT NULL CONSTRAINT [DF_AspNetUsers_EmailConfirmed] DEFAULT (0),
    [PasswordHash] NVARCHAR(MAX) NULL,
    [SecurityStamp] NVARCHAR(MAX) NULL,
    [PhoneNumber] NVARCHAR(MAX) NULL,
    [PhoneNumberConfirmed] BIT NOT NULL CONSTRAINT [DF_AspNetUsers_PhoneNumberConfirmed] DEFAULT (0),
    [TwoFactorEnabled] BIT NOT NULL CONSTRAINT [DF_AspNetUsers_TwoFactorEnabled] DEFAULT (0),
    [LockoutEndDateUtc] DATETIME NULL,
    [LockoutEnabled] BIT NOT NULL CONSTRAINT [DF_AspNetUsers_LockoutEnabled] DEFAULT (0),
    [AccessFailedCount] INT NOT NULL CONSTRAINT [DF_AspNetUsers_AccessFailedCount] DEFAULT (0),
    [UserName] NVARCHAR(256) NOT NULL,
    [FullName] NVARCHAR(MAX) NULL,
    [Phone] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
)
GO
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON dbo.AspNetUsers([UserName] ASC)
GO

CREATE TABLE dbo.AspNetUserRoles (
    [UserId] NVARCHAR(128) NOT NULL,
    [RoleId] NVARCHAR(128) NOT NULL,
    CONSTRAINT [PK_dbo.AspNetUserRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
    CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES dbo.AspNetRoles ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES dbo.AspNetUsers ([Id]) ON DELETE CASCADE
)
GO
CREATE NONCLUSTERED INDEX [IX_RoleId] ON dbo.AspNetUserRoles([RoleId] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_UserId] ON dbo.AspNetUserRoles([UserId] ASC)
GO

CREATE TABLE dbo.AspNetUserClaims (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [UserId] NVARCHAR(128) NOT NULL,
    [ClaimType] NVARCHAR(MAX) NULL,
    [ClaimValue] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_dbo.AspNetUserClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES dbo.AspNetUsers ([Id]) ON DELETE CASCADE
)
GO
CREATE NONCLUSTERED INDEX [IX_UserId] ON dbo.AspNetUserClaims([UserId] ASC)
GO

CREATE TABLE dbo.AspNetUserLogins (
    [LoginProvider] NVARCHAR(128) NOT NULL,
    [ProviderKey] NVARCHAR(128) NOT NULL,
    [UserId] NVARCHAR(128) NOT NULL,
    CONSTRAINT [PK_dbo.AspNetUserLogins] PRIMARY KEY CLUSTERED ([LoginProvider] ASC, [ProviderKey] ASC, [UserId] ASC),
    CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES dbo.AspNetUsers ([Id]) ON DELETE CASCADE
)
GO
CREATE NONCLUSTERED INDEX [IX_UserId] ON dbo.AspNetUserLogins([UserId] ASC)
GO

CREATE TABLE dbo.Category (
    [id] INT IDENTITY(1,1) NOT NULL,
    [title] NVARCHAR(150) NOT NULL,
    [description] NVARCHAR(MAX) NULL,
    [alias] NVARCHAR(MAX) NULL,
    [createdDate] DATETIME NOT NULL CONSTRAINT [DF_Category_createdDate] DEFAULT (GETDATE()),
    [createdBy] NVARCHAR(MAX) NULL,
    [modifiedDate] DATETIME NOT NULL CONSTRAINT [DF_Category_modifiedDate] DEFAULT (GETDATE()),
    [modifiedBy] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_dbo.Category] PRIMARY KEY CLUSTERED ([id] ASC)
)
GO

CREATE TABLE dbo.Product (
    [id] INT IDENTITY(1,1) NOT NULL,
    [title] NVARCHAR(MAX) NOT NULL,
    [alias] NVARCHAR(MAX) NULL,
    [productCode] NVARCHAR(MAX) NOT NULL,
    [description] NVARCHAR(MAX) NULL,
    [detail] NVARCHAR(MAX) NULL,
    [image] NVARCHAR(MAX) NULL,
    [price] INT NOT NULL,
    [priceSale] INT NOT NULL CONSTRAINT [DF_Product_priceSale] DEFAULT (0),
    [quantity] INT NOT NULL CONSTRAINT [DF_Product_quantity] DEFAULT (0),
    [categoryID] INT NOT NULL,
    [isNew] BIT NOT NULL CONSTRAINT [DF_Product_isNew] DEFAULT (0),
    [isSale] BIT NOT NULL CONSTRAINT [DF_Product_isSale] DEFAULT (0),
    [createdDate] DATETIME NOT NULL CONSTRAINT [DF_Product_createdDate] DEFAULT (GETDATE()),
    [createdBy] NVARCHAR(MAX) NULL,
    [modifiedDate] DATETIME NOT NULL CONSTRAINT [DF_Product_modifiedDate] DEFAULT (GETDATE()),
    [modifiedBy] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_dbo.Product] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_dbo.Product_dbo.Category_categoryID] FOREIGN KEY ([categoryID]) REFERENCES dbo.Category ([id]) ON DELETE CASCADE
)
GO
CREATE NONCLUSTERED INDEX [IX_categoryID] ON dbo.Product([categoryID] ASC)
GO

CREATE TABLE dbo.ProductImage (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [ProductId] INT NOT NULL,
    [image] NVARCHAR(MAX) NULL,
    [isDefault] BIT NOT NULL CONSTRAINT [DF_ProductImage_isDefault] DEFAULT (0),
    CONSTRAINT [PK_dbo.ProductImage] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ProductImage_dbo.Product_ProductId] FOREIGN KEY ([ProductId]) REFERENCES dbo.Product ([id]) ON DELETE CASCADE
)
GO
CREATE NONCLUSTERED INDEX [IX_ProductId] ON dbo.ProductImage([ProductId] ASC)
GO

CREATE TABLE dbo.tbOrder (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [code] NVARCHAR(MAX) NULL,
    [customerName] NVARCHAR(MAX) NOT NULL,
    [phone] NVARCHAR(MAX) NOT NULL,
    [address] NVARCHAR(MAX) NOT NULL,
    [email] NVARCHAR(MAX) NULL,
    [totalAmount] INT NOT NULL,
    [quantity] INT NOT NULL CONSTRAINT [DF_tbOrder_quantity] DEFAULT (0),
    [paymentStatus] INT NOT NULL CONSTRAINT [DF_tbOrder_paymentStatus] DEFAULT (0),
    [paymentType] INT NOT NULL CONSTRAINT [DF_tbOrder_paymentType] DEFAULT (0),
    [createdDate] DATETIME NOT NULL CONSTRAINT [DF_tbOrder_createdDate] DEFAULT (GETDATE()),
    [createdBy] NVARCHAR(MAX) NULL,
    [modifiedDate] DATETIME NOT NULL CONSTRAINT [DF_tbOrder_modifiedDate] DEFAULT (GETDATE()),
    [modifiedBy] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_dbo.tbOrder] PRIMARY KEY CLUSTERED ([Id] ASC)
)
GO

CREATE TABLE dbo.tbOrderDetail (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [orderID] INT NOT NULL,
    [productID] INT NOT NULL,
    [price] INT NOT NULL,
    [quantity] INT NOT NULL,
    CONSTRAINT [PK_dbo.tbOrderDetail] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.tbOrderDetail_dbo.tbOrder_orderID] FOREIGN KEY ([orderID]) REFERENCES dbo.tbOrder ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.tbOrderDetail_dbo.Product_productID] FOREIGN KEY ([productID]) REFERENCES dbo.Product ([id]) ON DELETE CASCADE
)
GO
CREATE NONCLUSTERED INDEX [IX_orderID] ON dbo.tbOrderDetail([orderID] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_productID] ON dbo.tbOrderDetail([productID] ASC)
GO

CREATE TABLE dbo.News (
    [id] INT IDENTITY(1,1) NOT NULL,
    [title] NVARCHAR(MAX) NOT NULL,
    [alias] NVARCHAR(MAX) NULL,
    [description] NVARCHAR(MAX) NOT NULL,
    [detail] NVARCHAR(MAX) NOT NULL,
    [image] NVARCHAR(MAX) NOT NULL,
    [createdDate] DATETIME NOT NULL CONSTRAINT [DF_News_createdDate] DEFAULT (GETDATE()),
    [createdBy] NVARCHAR(MAX) NULL,
    [modifiedDate] DATETIME NOT NULL CONSTRAINT [DF_News_modifiedDate] DEFAULT (GETDATE()),
    [modifiedBy] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_dbo.News] PRIMARY KEY CLUSTERED ([id] ASC)
)
GO

CREATE TABLE dbo.Adv (
    [id] INT IDENTITY(1,1) NOT NULL,
    [title] NVARCHAR(MAX) NULL,
    [description] NVARCHAR(MAX) NULL,
    [image] NVARCHAR(MAX) NULL,
    [type] INT NOT NULL CONSTRAINT [DF_Adv_type] DEFAULT (0),
    [link] NVARCHAR(MAX) NULL,
    [createdDate] DATETIME NOT NULL CONSTRAINT [DF_Adv_createdDate] DEFAULT (GETDATE()),
    [createdBy] NVARCHAR(MAX) NULL,
    [modifiedDate] DATETIME NOT NULL CONSTRAINT [DF_Adv_modifiedDate] DEFAULT (GETDATE()),
    [modifiedBy] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_dbo.Adv] PRIMARY KEY CLUSTERED ([id] ASC)
)
GO

CREATE TABLE dbo.SystemSettings (
    [id] INT IDENTITY(1,1) NOT NULL,
    [settingKey] NVARCHAR(MAX) NULL,
    [settingValue] NVARCHAR(MAX) NULL,
    [settingDescription] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_dbo.SystemSettings] PRIMARY KEY CLUSTERED ([id] ASC)
)
GO

INSERT INTO dbo.AspNetRoles ([Id], [Name]) VALUES 
(N'd7593c8d-692a-4a25-8ff4-bf9c8fbe2821', N'Admin'),
(N'f4b93814-72b2-4d24-8149-a0e28e937d5c', N'Nhân viên'),
(N'906c71c4-9e9f-4318-97c4-2e92c2084c7e', N'Customer');
GO

INSERT INTO dbo.AspNetUsers (
    [Id], [UserName], [Email], [EmailConfirmed], 
    [PasswordHash], [SecurityStamp], [PhoneNumberConfirmed], 
    [TwoFactorEnabled], [LockoutEnabled], [AccessFailedCount], 
    [FullName], [Phone]
) VALUES (
    N'33ca0b1c-3b95-4fb5-9003-dfc5210c85ee',
    N'admin',
    N'admin@pcshop.vn',
    1,
    N'AEMFUiN+8dsuMhTCa3J35Hzv8wzYqwAbcw1czOA5PVq+u2ZnJtiPJSxJGisCvpnUNQ==',
    N'41df47a8-ebf1-4da2-817e-7c5bfebe7a44',
    0, 0, 0, 0,
    N'Quản trị viên',
    N'0987654321'
);
GO

INSERT INTO dbo.AspNetUserRoles ([UserId], [RoleId]) VALUES 
(N'33ca0b1c-3b95-4fb5-9003-dfc5210c85ee', N'd7593c8d-692a-4a25-8ff4-bf9c8fbe2821');
GO

SET IDENTITY_INSERT dbo.Category ON;
INSERT INTO dbo.Category ([id], [title], [description], [alias], [createdDate], [createdBy], [modifiedDate], [modifiedBy]) VALUES
(1, N'Laptop Gaming', N'Laptop chơi game và đồ họa hiệu năng cao', N'laptop-gaming', GETDATE(), N'System', GETDATE(), N'System'),
(2, N'PC Đồng Bộ & Gaming', N'Dàn máy tính để bàn PC lắp ráp và đồng bộ', N'pc-dong-bo-gaming', GETDATE(), N'System', GETDATE(), N'System'),
(3, N'Linh Kiện Máy Tính', N'CPU, VGA, RAM, Mainboard, Nguồn máy tính', N'linh-kien-may-tinh', GETDATE(), N'System', GETDATE(), N'System'),
(4, N'Màn Hình Máy Tính', N'Màn hình chuyên game và đồ họa sắc nét', N'man-hinh-may-tinh', GETDATE(), N'System', GETDATE(), N'System'),
(5, N'Phím Chuột & Phụ Kiện', N'Gaming Gear, Chuột, Bàn phím cơ, Tai nghe', N'phim-chuot-phu-kien', GETDATE(), N'System', GETDATE(), N'System');
SET IDENTITY_INSERT dbo.Category OFF;
GO

SET IDENTITY_INSERT dbo.SystemSettings ON;
INSERT INTO dbo.SystemSettings ([id], [settingKey], [settingValue], [settingDescription]) VALUES
(1, N'SettingTitle', N'PCShop - Hệ thống máy tính & linh kiện hàng đầu', N'Tiêu đề trang web'),
(2, N'SettingLogo', N'/Content/client/images/logo.png', N'Logo website'),
(3, N'SettingHotline', N'1900 1234', N'Số hotline hỗ trợ'),
(4, N'SettingEmail', N'support@pcshop.vn', N'Email liên hệ');
SET IDENTITY_INSERT dbo.SystemSettings OFF;
GO

IF OBJECT_ID('dbo.Permissions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Permissions (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Code NVARCHAR(100) NOT NULL UNIQUE,
        Name NVARCHAR(200) NOT NULL,
        Module NVARCHAR(100) NOT NULL,
        Description NVARCHAR(300) NULL
    );
END
GO

IF OBJECT_ID('dbo.RolePermissions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RolePermissions (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        RoleId NVARCHAR(128) NOT NULL,
        PermissionId INT NOT NULL,
        CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE,
        CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY (PermissionId) REFERENCES dbo.Permissions(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_RolePermissions_Role_Perm UNIQUE (RoleId, PermissionId)
    );
END
GO

IF COL_LENGTH('dbo.SystemSettings', 'settingName') IS NULL
    ALTER TABLE dbo.SystemSettings ADD settingName NVARCHAR(200) NULL;
IF COL_LENGTH('dbo.SystemSettings', 'settingGroup') IS NULL
    ALTER TABLE dbo.SystemSettings ADD settingGroup NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.SystemSettings', 'settingType') IS NULL
    ALTER TABLE dbo.SystemSettings ADD settingType NVARCHAR(50) NULL;
GO
