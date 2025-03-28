# Clasificación de Polilíneas en AutoCAD y Exportación a CSV

## Descripción  
Este proyecto permite analizar y clasificar polilíneas dentro de una capa de AutoCAD según ciertas condiciones, generando un archivo **.csv** con el resumen de los resultados. Las polilíneas se clasifican en base a su longitud, número de vértices y si están cerradas o abiertas.

## Funcionamiento  

1. **Inicio del proceso** (`RankPolyline()`):  
   - Se obtiene el documento activo en AutoCAD.  
   - Se inicializa una estructura de datos para almacenar la clasificación de las polilíneas.  

2. **Ajuste de unidades y selección de capa**:  
   - Se verifica si las unidades del dibujo están en pies y, de no ser así, se ajustan.  
   - Se obtiene la lista de capas disponibles y se solicita al usuario seleccionar la capa que contiene las polilíneas a clasificar.  

3. **Recorrido y clasificación de polilíneas**:  
   - Se recorren todas las entidades en el espacio modelo.  
   - Se identifican las polilíneas y se extraen sus atributos:  
     - **Longitud**: Se clasifican en tres categorías (<200, 200-1000, >1000).  
     - **Número de vértices**: Se dividen en ≤10 o >10.  
     - **Estado**: Se identifican como cerradas o abiertas.  
   - Se asignan colores diferentes a las polilíneas en función de su longitud.  

4. **Exportación de resultados** (`ExportToCsv()`):  
   - Se genera un archivo **.csv** con el conteo total de polilíneas en cada categoría.  
   - El archivo se guarda en el directorio del proyecto con el nombre `reultCommand.csv`.