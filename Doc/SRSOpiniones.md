Sistema de Análisis de Opiniones de Clientes con
Proceso ETL

Contenido
1. Introducción ............................................................................................................. 3

1.1 Propósito ............................................................................................................. 3

1.2 Alcance ............................................................................................................... 3

1.3 Definiciones, Acrónimos y Abreviaturas ................................................................. 3

1.4 Referencias ......................................................................................................... 3

2. Descripción General del Sistema ................................................................................ 3

2.1 Perspectiva del Producto ...................................................................................... 3

2.2 Funciones del Sistema ......................................................................................... 4

2.3 Usuarios del Sistema ............................................................................................ 4

2.4 Restricciones ....................................................................................................... 5

3. Requisitos Específicos ............................................................................................... 5

3.1 Requisitos Funcionales ........................................................................................ 5

3.2 Requisitos No Funcionales ................................................................................... 5

4. Modelo de Datos (Resumen) ...................................................................................... 6

5. Entregables ............................................................................................................... 6

1. Introducción

1.1 Propósito

El propósito de este documento es definir los requisitos funcionales y no funcionales del
Sistema de Análisis de Opiniones de Clientes con Proceso ETL
El sistema permitirá consolidar, transformar y analizar opiniones de clientes provenientes
de encuestas internas, reseñas web y redes sociales, con el fin de generar indicadores de
satisfacción y tendencias de opinión.

1.2 Alcance

El sistema proveerá a la empresa de comercio electrónico de:

•

Integración de datos de múltiples canales (CSV, BD relacional y API REST).

•  Limpieza, validación y clasificación automática de opiniones.

•  Generación de indicadores clave de satisfacción por producto.

•  Visualización interactiva de tendencias de satisfacción y clasificación de opiniones.

1.3 Definiciones, Acrónimos y Abreviaturas

•  ETL: Extract, Transform, Load.

•  API REST: Interfaz de programación basada en HTTP/JSON.

•  NLP: Natural Language Processing (Procesamiento de Lenguaje Natural).

•  KPI: Indicador clave de desempeño.

1.4 Referencias

•  Microsoft .NET 8 Worker Services, HttpClient y ADO.NET.

•  Librerías NLP básicas en C# (ejemplo: ML.NET).

•  Power BI Report Builder y Chart.js para visualización.

2. Descripción General del Sistema

2.1 Perspectiva del Producto

El sistema estará compuesto por:

•  Módulo ETL en .NET Worker Service para extracción, transformación y carga de

datos.

•  Base de datos analítica en PostgreSQL, SQL Server o SQLite.

•  Módulo de reportes implementado en Power BI o ASP.NET Core.

2.2 Funciones del Sistema

•  Extracción de datos desde:

o  Archivos CSV (encuestas internas).

o  Base de datos relacional (reseñas web).

o  API REST (comentarios en redes sociales).

•  Transformación:

o  Limpieza de duplicados, comentarios vacíos y caracteres especiales.

o  Normalización de fechas, productos y clientes.

o  Clasificación de opiniones en positivas, negativas y neutras.

o  Cálculo de métricas como cantidad de comentarios y porcentaje de

satisfacción.

•  Carga de datos:

o

Inserción de comentarios procesados en la base analítica central.

•  Consultas y reportes:

o  Opiniones por producto y rango de fechas.

o  Clasificación de opiniones por tipo.

o  Tendencia de satisfacción por producto.

o  Porcentaje de satisfacción global y por producto.

2.3 Usuarios del Sistema

•  Analistas de negocio: Interpretan tendencias y niveles de satisfacción.

•  Gerencia: Consulta KPIs estratégicos.

•  Equipo de TI: Mantiene y configura el pipeline ETL.

2.4 Restricciones

•  El sistema debe ser desarrollado en .NET 8.

•  Los datos deben almacenarse en una base SQL (PostgreSQL, SQL Server o SQLite).

•  La clasificación de opiniones debe realizarse con un enfoque sencillo: palabras

clave o modelo NLP básico.

3. Requisitos Específicos

3.1 Requisitos Funcionales

1.  El sistema debe extraer datos desde CSV, BD relacional y API REST.

2.  El sistema debe limpiar duplicados y comentarios inválidos.

3.  El sistema debe clasificar automáticamente las opiniones (positiva, negativa,

neutra).

4.  El sistema debe almacenar todos los datos procesados en la BD analítica.

5.  El sistema debe permitir consultas de:

o  Opiniones por producto y fecha.

o  Número de comentarios procesados.

o  Porcentaje de satisfacción por producto.

o  Tendencia de satisfacción en el tiempo.

6.  El sistema debe mostrar un dashboard interactivo con KPIs y gráficas.

3.2 Requisitos No Funcionales

•  Rendimiento: Procesar al menos 50,000 comentarios en menos de 5 minutos.

•  Escalabilidad: Poder agregar nuevas fuentes de comentarios sin rediseñar la

arquitectura.

•  Seguridad: Acceso seguro a API y BD con credenciales protegidas.

•  Usabilidad: Dashboard intuitivo para usuarios no técnicos.

4. Modelo de Datos (Resumen)

Tablas principales:

•  Clientes (IdCliente, Nombre, Email).

•  Productos (IdProducto, Nombre, Categoría).

•  Opiniones (IdOpinion, IdCliente, IdProducto, Fuente, Fecha, Comentario,

Clasificación, PuntajeSatisfacción).

•  FuenteDatos (IdFuente, TipoFuente, FechaCarga).

5. Entregables

•  Código fuente en C# (.NET Worker Service).

•  Script SQL para creación de la base analítica.

•  Diagrama Entidad Relación del modelo de datos.

•  Documentación del pipeline ETL con diagrama de flujo.

•  Dashboard con indicadores clave de satisfacción y tendencias.

