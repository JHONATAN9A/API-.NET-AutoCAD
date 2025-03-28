# Exportación de Geometrías de AutoCAD a PostgreSQL con PostGIS

## Descripción  
Este proyecto permite exportar todas las geometrías de un archivo de AutoCAD a una base de datos PostgreSQL con PostGIS en el SRID **2236**. Para ello, las entidades del dibujo se recorren, se extraen sus atributos y se convierten a formato **WKT (Well-Known Text)** antes de ser insertadas en la base de datos.

## Funcionamiento  
1. **Obtención de credenciales**:  
   - Se obtienen los datos de conexión a la base de datos desde las variables de entorno del proyecto.  

2. **Extracción de geometrías** (`ExportPostGIS()`):  
   - Se accede al documento activo en AutoCAD y se recorren todas las entidades dentro del espacio modelo.  
   - Se extraen atributos como nombre de la capa, color, tipo y geometría.  
   - La geometría se convierte a formato **WKT** mediante `ConvertToWKT()` para que sea reconocida por PostGIS.  
   - Toda la información se almacena en un array de objetos `LayerGeometry`.  

3. **Exportación a PostgreSQL** (`LoadPostgresSQL()`):  
   - Se establece la conexión con PostgreSQL usando `NpgsqlConnection`.  
   - Se inicia una transacción e inserta cada geometría en la tabla `test.autocad`.  
   - Se utiliza `ST_GeomFromText()` para convertir las geometrías al sistema PostGIS con SRID **2236**. 
   
4. **Generar Reporte** (`GenerateReport()`):  
   - Genera un archivo `ExportPostGIS_Report.txt` en el cual se  mencionan la cantidad de registros exportados y si ocurrio algun error.  

## Creación de la Tabla en PostgreSQL  
Antes de ejecutar la exportación, es necesario crear la tabla donde se almacenarán las geometrías. Para ello, ejecutar el siguiente comando en la base de datos PostgreSQL:

```sql
CREATE TABLE test.autocad (
    id SERIAL PRIMARY KEY,
    id_layer BIGINT NOT NULL,
    name_layer TEXT NOT NULL,
    color_layer TEXT,
    type_layer TEXT NOT NULL,
    geometry test.geometry(geometry, 2236) NOT NULL
); 

```

## Consideraciones
- Las entidades no se agrupan por capa, sino que cada una se inserta individualmente en la base de datos.
- Advertencia: El proceso de inserción puede ser lento y consumir mucha memoria, lo que podría provocar un colapso temporal. Sin embargo, esto no significa que los datos no se inserten correctamente en la base de datos.
- Para optimizar la carga de datos, se recomienda mejorar la función de inserción en la base de datos o considerar métodos alternativos, como exportar los datos a un archivo .csv y luego cargarlos a PostgreSQL mediante COPY o pgAdmin.