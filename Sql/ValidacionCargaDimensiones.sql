-- Script de validación de carga para dimensiones del modelo analítico
-- Base de datos: SistemaOpiniones
-- Consulta volumen, muestras, estructura e integridad referencial de Clientes, Productos y FuenteDatos.

USE SistemaOpiniones;
GO

-- Conteos y volumen de datos por dimensión
WITH ConteosDimensiones AS
(
    SELECT
        1 AS Orden,
        N'Clientes' AS Tabla,
        COUNT(*) AS Cantidad
    FROM dbo.Clientes

    UNION ALL

    SELECT
        2 AS Orden,
        N'Productos' AS Tabla,
        COUNT(*) AS Cantidad
    FROM dbo.Productos

    UNION ALL

    SELECT
        3 AS Orden,
        N'FuenteDatos' AS Tabla,
        COUNT(*) AS Cantidad
    FROM dbo.FuenteDatos
)
SELECT
    N'Conteo real de dimensiones' AS [Etiqueta],
    Tabla,
    Cantidad
FROM ConteosDimensiones
ORDER BY Orden;
GO

-- Muestras de registros por dimensión
SELECT TOP (10)
    N'Muestra de la dimensión Cliente — dbo.Clientes' AS [Etiqueta],
    c.IdCliente,
    c.Nombre,
    c.Email
FROM dbo.Clientes AS c
ORDER BY c.IdCliente;
GO

SELECT TOP (10)
    N'Muestra de la dimensión Producto — dbo.Productos' AS [Etiqueta],
    p.IdProducto,
    p.Nombre,
    p.Categoria
FROM dbo.Productos AS p
ORDER BY p.IdProducto;
GO

SELECT TOP (10)
    N'Muestra de la dimensión Fuente — dbo.FuenteDatos' AS [Etiqueta],
    f.IdFuente,
    f.TipoFuente,
    f.FechaCarga
FROM dbo.FuenteDatos AS f
ORDER BY f.IdFuente;
GO

-- Estructura del esquema (INFORMATION_SCHEMA)
SELECT
    N'Estructura de las dimensiones — INFORMATION_SCHEMA.COLUMNS' AS [Etiqueta],
    c.TABLE_SCHEMA + N'.' + c.TABLE_NAME AS [Tabla],
    c.COLUMN_NAME AS [Columna],
    c.DATA_TYPE AS [TipoDato],
    c.CHARACTER_MAXIMUM_LENGTH AS [Longitud],
    c.IS_NULLABLE AS [PermiteNulos],
    c.ORDINAL_POSITION AS [PosicionOrdinal]
FROM INFORMATION_SCHEMA.COLUMNS AS c
WHERE c.TABLE_SCHEMA = 'dbo'
  AND c.TABLE_NAME IN ('Clientes', 'Productos', 'FuenteDatos')
ORDER BY
    CASE c.TABLE_NAME
        WHEN 'Clientes' THEN 1
        WHEN 'Productos' THEN 2
        WHEN 'FuenteDatos' THEN 3
    END,
    c.ORDINAL_POSITION;
GO

-- Calidad de datos (nulos, vacíos y claves duplicadas)
-- Los conteos de [Problemas] deben ser 0
SELECT
    N'Clientes con identificador o nombre vacío — dbo.Clientes' AS [Etiqueta],
    COUNT(*) AS [Problemas]
FROM dbo.Clientes AS c
WHERE NULLIF(LTRIM(RTRIM(c.IdCliente)), '') IS NULL
   OR NULLIF(LTRIM(RTRIM(c.Nombre)), '') IS NULL

UNION ALL

SELECT
    N'Productos con identificador o nombre vacío — dbo.Productos' AS [Etiqueta],
    COUNT(*) AS [Problemas]
FROM dbo.Productos AS p
WHERE NULLIF(LTRIM(RTRIM(p.IdProducto)), '') IS NULL
   OR NULLIF(LTRIM(RTRIM(p.Nombre)), '') IS NULL

UNION ALL

SELECT
    N'Fuentes con tipo vacío — dbo.FuenteDatos' AS [Etiqueta],
    COUNT(*) AS [Problemas]
FROM dbo.FuenteDatos AS f
WHERE NULLIF(LTRIM(RTRIM(f.TipoFuente)), '') IS NULL

UNION ALL

SELECT
    N'Claves duplicadas de la dimensión Cliente — dbo.Clientes.IdCliente' AS [Etiqueta],
    COUNT(*) AS [Problemas]
FROM
(
    SELECT c.IdCliente
    FROM dbo.Clientes AS c
    GROUP BY c.IdCliente
    HAVING COUNT(*) > 1
) AS DuplicadosClientes

UNION ALL

SELECT
    N'Claves duplicadas de la dimensión Producto — dbo.Productos.IdProducto' AS [Etiqueta],
    COUNT(*) AS [Problemas]
FROM
(
    SELECT p.IdProducto
    FROM dbo.Productos AS p
    GROUP BY p.IdProducto
    HAVING COUNT(*) > 1
) AS DuplicadosProductos

UNION ALL

SELECT
    N'Claves duplicadas de la dimensión Fuente — dbo.FuenteDatos.IdFuente' AS [Etiqueta],
    COUNT(*) AS [Problemas]
FROM
(
    SELECT f.IdFuente
    FROM dbo.FuenteDatos AS f
    GROUP BY f.IdFuente
    HAVING COUNT(*) > 1
) AS DuplicadosFuentes

UNION ALL

SELECT
    N'Tipos de fuente duplicados para resolución del ETL — dbo.FuenteDatos.TipoFuente' AS [Etiqueta],
    COUNT(*) AS [Problemas]
FROM
(
    SELECT f.TipoFuente
    FROM dbo.FuenteDatos AS f
    GROUP BY f.TipoFuente
    HAVING COUNT(*) > 1
) AS TiposFuenteDuplicados;
GO

-- Relación e integridad referencial con dbo.Opiniones
SELECT
    N'Cantidad de opiniones por TipoFuente — dbo.Opiniones' AS [Etiqueta],
    f.TipoFuente,
    COUNT(*) AS [Opiniones]
FROM dbo.Opiniones AS o
INNER JOIN dbo.FuenteDatos AS f
    ON f.IdFuente = o.IdFuente
GROUP BY f.TipoFuente
ORDER BY f.TipoFuente;
GO

-- Verificación de claves foráneas huérfanas (debe retornar 0)
SELECT
    N'Opiniones con producto inexistente' AS [Etiqueta],
    COUNT(*) AS [Problemas]
FROM dbo.Opiniones AS o
LEFT JOIN dbo.Productos AS p
    ON p.IdProducto = o.IdProducto
WHERE p.IdProducto IS NULL

UNION ALL

SELECT
    N'Opiniones con fuente inexistente' AS [Etiqueta],
    COUNT(*) AS [Problemas]
FROM dbo.Opiniones AS o
LEFT JOIN dbo.FuenteDatos AS f
    ON f.IdFuente = o.IdFuente
WHERE f.IdFuente IS NULL

UNION ALL

SELECT
    N'Opiniones con cliente informado inexistente' AS [Etiqueta],
    COUNT(*) AS [Problemas]
FROM dbo.Opiniones AS o
LEFT JOIN dbo.Clientes AS c
    ON c.IdCliente = o.IdCliente
WHERE o.IdCliente IS NOT NULL
  AND c.IdCliente IS NULL;
GO
