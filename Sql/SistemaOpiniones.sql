-- =========================================================================
-- Sistema de Análisis de Opiniones de Clientes con Proceso ETL
-- Script de creación de la base de datos analítica (SQL Server / LocalDB)
-- =========================================================================

-- Base de datos
IF DB_ID('SistemaOpiniones') IS NULL
BEGIN
    CREATE DATABASE SistemaOpiniones;
END
GO

USE SistemaOpiniones;
GO

-- Requerido por los índices filtrados.
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- =========================================================================
-- Tablas
-- =========================================================================

IF OBJECT_ID('dbo.Clientes', 'U') IS NULL
BEGIN
    CREATE TABLE Clientes (
        IdCliente VARCHAR(50) PRIMARY KEY,
        Nombre VARCHAR(150) NOT NULL,
        Email VARCHAR(150) NULL
    );
END
GO

IF OBJECT_ID('dbo.Productos', 'U') IS NULL
BEGIN
    CREATE TABLE Productos (
        IdProducto VARCHAR(50) PRIMARY KEY,
        Nombre VARCHAR(150) NOT NULL,
        Categoria VARCHAR(100) NULL
    );
END
GO

-- FuenteDatos usa IDENTITY; el ETL resuelve las fuentes por TipoFuente.
IF OBJECT_ID('dbo.FuenteDatos', 'U') IS NULL
BEGIN
    CREATE TABLE FuenteDatos (
        IdFuente INT IDENTITY(1,1) PRIMARY KEY,
        TipoFuente VARCHAR(50) NOT NULL,
        FechaCarga DATETIME DEFAULT GETDATE()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_FuenteDatos_TipoFuente')
BEGIN
    CREATE UNIQUE INDEX UQ_FuenteDatos_TipoFuente ON FuenteDatos(TipoFuente);
END
GO

-- =========================================================================
-- Tabla de hechos
-- =========================================================================
IF OBJECT_ID('dbo.Opiniones', 'U') IS NULL
BEGIN
    CREATE TABLE Opiniones (
        IdOpinion INT IDENTITY(1,1) PRIMARY KEY,
        IdCliente VARCHAR(50) NULL,        -- Hay opiniones sin cliente identificable.
        IdProducto VARCHAR(50) NOT NULL,
        IdFuente INT NOT NULL,
        Fecha DATETIME NOT NULL,
        Comentario VARCHAR(MAX) NOT NULL,
        Clasificacion VARCHAR(20) NOT NULL,
        PuntajeSatisfaccion INT NULL,
        OrigenId VARCHAR(50) NULL,

        CONSTRAINT FK_Opiniones_Clientes FOREIGN KEY (IdCliente) REFERENCES Clientes(IdCliente),
        CONSTRAINT FK_Opiniones_Productos FOREIGN KEY (IdProducto) REFERENCES Productos(IdProducto),
        CONSTRAINT FK_Opiniones_FuenteDatos FOREIGN KEY (IdFuente) REFERENCES FuenteDatos(IdFuente),
        CONSTRAINT CK_Opiniones_Clasificacion CHECK (Clasificacion IN ('Positiva', 'Negativa', 'Neutra'))
    );
END
GO

-- Evita duplicados entre corridas del ETL usando el Id original y la fuente.
-- El índice filtrado requiere ANSI_NULLS y QUOTED_IDENTIFIER activos.
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Opiniones_OrigenId_IdFuente')
BEGIN
    CREATE UNIQUE INDEX UQ_Opiniones_OrigenId_IdFuente
        ON Opiniones(OrigenId, IdFuente)
        WHERE OrigenId IS NOT NULL;
END
GO

-- Índices de apoyo
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Opiniones_IdProducto')
    CREATE INDEX IX_Opiniones_IdProducto ON Opiniones(IdProducto);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Opiniones_Fecha')
    CREATE INDEX IX_Opiniones_Fecha ON Opiniones(Fecha);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Opiniones_Clasificacion')
    CREATE INDEX IX_Opiniones_Clasificacion ON Opiniones(Clasificacion);
GO

-- =========================================================================
-- Vistas
-- =========================================================================

CREATE OR ALTER VIEW vw_ClasificacionOpiniones AS
SELECT
    o.IdProducto,
    p.Nombre AS NombreProducto,
    o.Clasificacion,
    COUNT(*) AS CantidadOpiniones,
    CAST(100.0 * COUNT(*) / SUM(COUNT(*)) OVER (PARTITION BY o.IdProducto) AS DECIMAL(5,2)) AS PorcentajeDelProducto
FROM Opiniones o
INNER JOIN Productos p ON p.IdProducto = o.IdProducto
GROUP BY o.IdProducto, p.Nombre, o.Clasificacion;
GO

CREATE OR ALTER VIEW vw_ResumenPorProducto AS
SELECT
    p.IdProducto,
    p.Nombre AS NombreProducto,
    p.Categoria,
    COUNT(o.IdOpinion) AS TotalOpiniones,
    SUM(CASE WHEN o.Clasificacion = 'Positiva' THEN 1 ELSE 0 END) AS Positivas,
    SUM(CASE WHEN o.Clasificacion = 'Negativa' THEN 1 ELSE 0 END) AS Negativas,
    SUM(CASE WHEN o.Clasificacion = 'Neutra' THEN 1 ELSE 0 END) AS Neutras,
    CAST(100.0 * SUM(CASE WHEN o.Clasificacion = 'Positiva' THEN 1 ELSE 0 END)
        / NULLIF(COUNT(o.IdOpinion), 0) AS DECIMAL(5,2)) AS PorcentajeSatisfaccion,
    AVG(CAST(o.PuntajeSatisfaccion AS DECIMAL(5,2))) AS PuntajePromedio
FROM Productos p
LEFT JOIN Opiniones o ON o.IdProducto = p.IdProducto
GROUP BY p.IdProducto, p.Nombre, p.Categoria;
GO

CREATE OR ALTER VIEW vw_TendenciaSatisfaccionMensual AS
SELECT
    o.IdProducto,
    p.Nombre AS NombreProducto,
    DATEFROMPARTS(YEAR(o.Fecha), MONTH(o.Fecha), 1) AS Mes,
    COUNT(*) AS TotalOpiniones,
    SUM(CASE WHEN o.Clasificacion = 'Positiva' THEN 1 ELSE 0 END) AS Positivas,
    SUM(CASE WHEN o.Clasificacion = 'Negativa' THEN 1 ELSE 0 END) AS Negativas,
    SUM(CASE WHEN o.Clasificacion = 'Neutra' THEN 1 ELSE 0 END) AS Neutras,
    AVG(CAST(o.PuntajeSatisfaccion AS DECIMAL(5,2))) AS PuntajePromedio
FROM Opiniones o
INNER JOIN Productos p ON p.IdProducto = o.IdProducto
GROUP BY o.IdProducto, p.Nombre, DATEFROMPARTS(YEAR(o.Fecha), MONTH(o.Fecha), 1);
GO

-- =========================================================================
-- Procedimientos almacenados
-- =========================================================================

CREATE OR ALTER PROCEDURE sp_ListarOpinionesPorProductoYFecha
    @IdProducto VARCHAR(50) = NULL,
    @FechaInicio DATETIME = NULL,
    @FechaFin DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        o.IdOpinion, o.IdCliente, c.Nombre AS NombreCliente,
        o.IdProducto, p.Nombre AS NombreProducto,
        f.TipoFuente, o.Fecha, o.Comentario, o.Clasificacion, o.PuntajeSatisfaccion
    FROM Opiniones o
    INNER JOIN Productos p ON p.IdProducto = o.IdProducto
    LEFT JOIN Clientes c ON c.IdCliente = o.IdCliente
    INNER JOIN FuenteDatos f ON f.IdFuente = o.IdFuente
    WHERE (@IdProducto IS NULL OR o.IdProducto = @IdProducto)
      AND (@FechaInicio IS NULL OR o.Fecha >= @FechaInicio)
      AND (@FechaFin IS NULL OR o.Fecha <= @FechaFin)
    ORDER BY o.Fecha DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_ClasificacionPorTipo
    @IdProducto VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Clasificacion, SUM(CantidadOpiniones) AS CantidadOpiniones
    FROM vw_ClasificacionOpiniones
    WHERE @IdProducto IS NULL OR IdProducto = @IdProducto
    GROUP BY Clasificacion;
END
GO

CREATE OR ALTER PROCEDURE sp_TendenciaSatisfaccion
    @IdProducto VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Mes, IdProducto, NombreProducto, TotalOpiniones, Positivas, Negativas, Neutras, PuntajePromedio
    FROM vw_TendenciaSatisfaccionMensual
    WHERE @IdProducto IS NULL OR IdProducto = @IdProducto
    ORDER BY Mes;
END
GO
