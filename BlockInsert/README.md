# Inserción de Bloques en Extremos de Polilíneas en AutoCAD

## Descripción  
Este programa permite copiar un bloque de referencia y colocarlo en el inicio y final de un conjunto de polilíneas dentro de AutoCAD. Para evitar duplicaciones, se mantiene un registro de ubicaciones donde ya se ha insertado el bloque. Además, el bloque no se coloca en el punto de origen de la polilínea, sino en su centro geométrico.

## Funcionamiento  

1. **Inicio del comando** (`InsertBlock()`):  
   - Se obtiene el documento activo en AutoCAD.  
   - Se listan las capas disponibles y el usuario selecciona la capa de polilíneas y la capa del bloque.  

2. **Selección del bloque de referencia**:  
   - Se busca en el espacio de trabajo un bloque ubicado en la capa especificada por el usuario.  
   - Si no se encuentra un bloque en la capa, el proceso se detiene.  

3. **Procesamiento de las polilíneas**:  
   - Se recorren todas las polilíneas dentro de la capa seleccionada.  
   - Se obtienen los puntos inicial y final de cada polilínea.  
   - Se inserta el bloque en estos puntos, asegurando que no haya duplicados mediante un `HashSet<Point3d>`.  

4. **Inserción del bloque** (`DrawBlock()`):  
   - Se verifica si la ubicación ya tiene un bloque insertado.  
   - Se clona el bloque de referencia y se ajusta su posición al centro de la geometría.  
   - Se inserta en el modelo sin modificar su geometría original.  

## Consideraciones  
- **El bloque se posiciona en el centro geométrico** y no en el punto de origen.  
- **Se evita la inserción duplicada** mediante una lista de ubicaciones ya utilizadas.     
- **El bloque pequeño ubicado en la esquina inferior izquierda se mueve a una nueva capa** para facilitar su manipulación, ya que inicialmente estaba unido a un bloque mucho mayor que representa el margen del proyecto (Esto se realiza de manera manual).  