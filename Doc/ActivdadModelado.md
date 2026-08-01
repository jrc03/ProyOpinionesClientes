Actividad 3.1 – Modelado de la Base de Datos Sistema de Opiniones de Clientes

Objetivo de la práctica

Diseñar y modelar la base de datos analítica que almacenará los resultados del proceso
ETL del Sistema de Análisis de Opiniones de Clientes.
El modelo deberá integrar y consolidar los datos provenientes de encuestas internas,
reseñas web y comentarios obtenidos a través de APIs externas (redes sociales).

Enunciado de la práctica

Una empresa de comercio electrónico desea analizar las opiniones de sus clientes acerca
de los productos que ofrece, recopiladas desde distintos canales:

•  Archivos CSV: con los resultados de encuestas internas de satisfacción.

•  Base de datos relacional: con reseñas publicadas en su sitio web.

•  API REST: que expone los comentarios de usuarios en redes sociales.

Tu tarea consiste en modelar la base de datos analítica donde se almacenará toda la
información procesada por el ETL.
El modelo deberá permitir el análisis de indicadores clave como:

•  Total de comentarios procesados por periodo o producto.

•  Clasificación de opiniones (positivas, negativas o neutras).

•  Porcentaje de satisfacción por producto.

•  Tendencias de opinión a lo largo del tiempo.

Para lograrlo, deberás identificar las entidades principales, sus relaciones y definir las
claves primarias y foráneas necesarias para garantizar la integridad de los datos.

Requerimientos mínimos del modelo

1.  Identificar las entidades principales, considerando los datos de todas las fuentes

(encuestas, reseñas, redes sociales).

2.  Definir relaciones entre las tablas (por ejemplo, productos, comentarios,clientes,

opiniones, fechas, fuentes).

3.  Establecer llaves primarias y foráneas para mantener la integridad referencial.

4.  Diseñar un modelo tipo “estrella” que facilite las consultas analíticas.

5.  Generar el script SQL que cree la base de datos y sus tablas principales.

Entregables

1.  Diagrama Entidad–Relación (DER) que refleje el modelo propuesto.

2.  Script SQL para la creación de las tablas con sus claves y relaciones.

3.  Documento breve explicando las decisiones de diseño (estructura elegida,

normalización, relaciones y criterios de análisis).

Preguntas que debe responder el modelado de la base de datos

  1. Análisis general de comentarios

•  ¿Cuántos comentarios se han procesado en total?

•  ¿Cuál es el promedio de satisfacción general de los clientes?

•  ¿Cuántos comentarios corresponden a cada fuente de datos (encuestas, reseñas

web, redes sociales)?

•  ¿Qué porcentaje de los comentarios son positivos, negativos o neutros?

  2. Opiniones por producto

•  ¿Cuál es el producto con mayor número de comentarios?

•  ¿Qué productos tienen la mejor calificación promedio?

•  ¿Qué productos tienen más opiniones negativas?

•  ¿Cómo ha variado la satisfacción de cada producto a lo largo del tiempo?

•  ¿Cuál es el porcentaje de satisfacción por producto?

  3. Opiniones por cliente o segmento

•  ¿Qué clientes realizan más comentarios o encuestas?

•  ¿Qué grupos de clientes (por país, edad, tipo) muestran mayor nivel de

satisfacción?

•  ¿Existen patrones en las opiniones según la ubicación o el tipo de cliente?

  4. Tendencias y evolución temporal

•  ¿Cómo ha cambiado la percepción del cliente mes a mes o trimestre a trimestre?

•  ¿Qué eventos (por ejemplo, lanzamiento de un nuevo producto o campaña)

generaron más comentarios?

•  ¿Se observa una mejora o deterioro en la satisfacción general a lo largo del tiempo?

  5. Clasificación de sentimientos

•  ¿Cuántas opiniones fueron clasificadas como positivas, negativas o neutras?

•  ¿Qué palabras clave se asocian más frecuentemente con las opiniones positivas o

negativas? (opcional, si se usa NLP básico)

•  ¿Qué porcentaje del total representan las opiniones positivas sobre el total de

comentarios?

  6. Comparativas entre canales

•  ¿Cuál canal genera más comentarios (encuestas, web o redes sociales)?

•  ¿Existe diferencia en el tono de las opiniones según el canal?

•  ¿Qué canal tiene mayor proporción de comentarios negativos?

