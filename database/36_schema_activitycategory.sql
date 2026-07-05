-- Categorias de actividad (Diseno, Backend, QA, etc.), por empresa, en vez de
-- texto libre: evita duplicados como "Backend"/"backend"/"Back-end" y permite
-- reutilizarlas entre proyectos de la misma empresa.
CREATE TABLE dbo.ActivityCategory
(
    ActivityCategoryId INT IDENTITY(1,1) NOT NULL,
    CompanyId           INT               NOT NULL,
    Name                NVARCHAR(100)     NOT NULL,
    IsActive            BIT               NOT NULL CONSTRAINT DF_ActivityCategory_IsActive DEFAULT (1),
    CreatedBy           INT               NOT NULL,
    CreatedDate         DATETIME2         NOT NULL CONSTRAINT DF_ActivityCategory_CreatedDate DEFAULT (SYSUTCDATETIME()),
    ModifiedBy          INT               NULL,
    ModifiedDate        DATETIME2         NULL,
    IsDeleted           BIT               NOT NULL CONSTRAINT DF_ActivityCategory_IsDeleted DEFAULT (0),
    CONSTRAINT PK_ActivityCategory PRIMARY KEY CLUSTERED (ActivityCategoryId),
    CONSTRAINT FK_ActivityCategory_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company (CompanyId),
    CONSTRAINT UQ_ActivityCategory_Company_Name UNIQUE (CompanyId, Name)
);
GO
