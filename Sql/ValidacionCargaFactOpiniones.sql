USE SistemaOpiniones;
GO

SET NOCOUNT ON;
GO

SELECT
    COUNT(*) AS CantidadAntes,
    MIN(IdOpinion) AS PrimerIdAntes,
    MAX(IdOpinion) AS UltimoIdAntes
FROM dbo.Opiniones;
GO

SELECT
    COUNT(*) AS CantidadDespues,
    MIN(IdOpinion) AS PrimerIdDespues,
    MAX(IdOpinion) AS UltimoIdDespues,
    MIN(Fecha) AS PrimeraFechaOpinion,
    MAX(Fecha) AS UltimaFechaOpinion
FROM dbo.Opiniones;
GO

SELECT
    fuente.TipoFuente,
    COUNT(*) AS CantidadOpiniones
FROM dbo.Opiniones AS opinion
INNER JOIN dbo.FuenteDatos AS fuente
    ON fuente.IdFuente = opinion.IdFuente
GROUP BY fuente.TipoFuente
ORDER BY fuente.TipoFuente;
GO

SELECT
    Clasificacion,
    COUNT(*) AS CantidadOpiniones
FROM dbo.Opiniones
GROUP BY Clasificacion
ORDER BY Clasificacion;
GO

SELECT
    SUM(CASE
        WHEN NULLIF(LTRIM(RTRIM(Comentario)), '') IS NULL THEN 1
        ELSE 0
    END) AS ComentariosVacios,
    SUM(CASE WHEN Fecha IS NULL THEN 1 ELSE 0 END) AS FechasNulas,
    SUM(CASE WHEN IdProducto IS NULL THEN 1 ELSE 0 END) AS ProductosNulos,
    SUM(CASE WHEN IdFuente IS NULL THEN 1 ELSE 0 END) AS FuentesNulas,
    SUM(CASE
        WHEN Clasificacion NOT IN ('Positiva', 'Negativa', 'Neutra') THEN 1
        ELSE 0
    END) AS ClasificacionesInvalidas,
    SUM(CASE WHEN IdCliente IS NULL THEN 1 ELSE 0 END)
        AS ClientesAnonimosONulificados
FROM dbo.Opiniones;
GO

SELECT
    OrigenId,
    IdFuente,
    COUNT(*) AS Cantidad
FROM dbo.Opiniones
WHERE OrigenId IS NOT NULL
GROUP BY OrigenId, IdFuente
HAVING COUNT(*) > 1;
GO

SELECT
    (
        SELECT COUNT(*)
        FROM dbo.Opiniones AS opinion
        LEFT JOIN dbo.Productos AS producto
            ON producto.IdProducto = opinion.IdProducto
        WHERE producto.IdProducto IS NULL
    ) AS ProductosHuerfanos,
    (
        SELECT COUNT(*)
        FROM dbo.Opiniones AS opinion
        LEFT JOIN dbo.Clientes AS cliente
            ON cliente.IdCliente = opinion.IdCliente
        WHERE opinion.IdCliente IS NOT NULL
          AND cliente.IdCliente IS NULL
    ) AS ClientesHuerfanos,
    (
        SELECT COUNT(*)
        FROM dbo.Opiniones AS opinion
        LEFT JOIN dbo.FuenteDatos AS fuente
            ON fuente.IdFuente = opinion.IdFuente
        WHERE fuente.IdFuente IS NULL
    ) AS FuentesHuerfanas;
GO

SELECT TOP (20)
    opinion.IdOpinion,
    opinion.OrigenId,
    opinion.IdCliente,
    opinion.IdProducto,
    fuente.TipoFuente,
    opinion.Fecha,
    opinion.Clasificacion,
    opinion.PuntajeSatisfaccion,
    opinion.Comentario
FROM dbo.Opiniones AS opinion
INNER JOIN dbo.FuenteDatos AS fuente
    ON fuente.IdFuente = opinion.IdFuente
ORDER BY opinion.IdOpinion;
GO

SELECT
    ORDINAL_POSITION AS Posicion,
    COLUMN_NAME AS Columna,
    DATA_TYPE AS TipoDato,
    CHARACTER_MAXIMUM_LENGTH AS LongitudMaxima,
    IS_NULLABLE AS PermiteNulos
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo'
  AND TABLE_NAME = 'Opiniones'
ORDER BY ORDINAL_POSITION;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Opiniones)
    THROW 51000, 'La tabla de hechos Opiniones quedó vacía.', 1;

IF EXISTS
(
    SELECT 1
    FROM dbo.Opiniones
    WHERE NULLIF(LTRIM(RTRIM(Comentario)), '') IS NULL
       OR Fecha IS NULL
       OR IdProducto IS NULL
       OR IdFuente IS NULL
       OR Clasificacion NOT IN ('Positiva', 'Negativa', 'Neutra')
)
    THROW 51001, 'La tabla de hechos contiene datos obligatorios inválidos.', 1;

IF EXISTS
(
    SELECT 1
    FROM dbo.Opiniones
    WHERE OrigenId IS NOT NULL
    GROUP BY OrigenId, IdFuente
    HAVING COUNT(*) > 1
)
    THROW 51002, 'La tabla de hechos contiene opiniones duplicadas.', 1;

IF EXISTS
(
    SELECT 1
    FROM dbo.Opiniones AS opinion
    LEFT JOIN dbo.Productos AS producto
        ON producto.IdProducto = opinion.IdProducto
    WHERE producto.IdProducto IS NULL
)
    THROW 51003, 'Existen opiniones asociadas a productos inexistentes.', 1;

IF EXISTS
(
    SELECT 1
    FROM dbo.Opiniones AS opinion
    LEFT JOIN dbo.Clientes AS cliente
        ON cliente.IdCliente = opinion.IdCliente
    WHERE opinion.IdCliente IS NOT NULL
      AND cliente.IdCliente IS NULL
)
    THROW 51004, 'Existen opiniones asociadas a clientes inexistentes.', 1;

IF EXISTS
(
    SELECT 1
    FROM dbo.Opiniones AS opinion
    LEFT JOIN dbo.FuenteDatos AS fuente
        ON fuente.IdFuente = opinion.IdFuente
    WHERE fuente.IdFuente IS NULL
)
    THROW 51005, 'Existen opiniones asociadas a fuentes inexistentes.', 1;

PRINT 'VALIDACION EXITOSA: dbo.Opiniones contiene datos validos e integros.';
GO
