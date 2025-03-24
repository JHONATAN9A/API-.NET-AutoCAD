# API-.NET-AuotCAD
Primeros pasos para implementar comandos en AutoCAD, utilizando la API de .NET

### Configuración del Entorno  

Para desarrollar y ejecutar aplicaciones en AutoCAD con .NET Framework 8, sigue estos pasos:  

1. **Descargar e instalar AutoCAD**  
   - Asegúrate de tener una versión compatible de AutoCAD instalada en tu sistema.  

2. **Instalar Visual Studio**  
   - Descarga e instala [Visual Studio](https://visualstudio.microsoft.com/) con soporte para desarrollo en .NET.  

3. **Crear un nuevo proyecto en Visual Studio**  
   - Selecciona **C#** como lenguaje de programación.  
   - Elige **.NET Framework 8** como versión del framework.  

4. **Configurar referencias de AutoCAD en Visual Studio**  
   - Agrega las siguientes referencias necesarias para la interacción con AutoCAD:  
     - `AcDbMgd.dll`  
     - `AcMgd.dll`  
     - `AcCoreMgd.dll`  
   - Estas bibliotecas se encuentran en la siguiente ruta:  
     ```
     C:\Program Files\Autodesk\AutoCAD 20XX\Managed\
     ```
   - Asegúrate de reemplazar `20XX` con la versión específica de AutoCAD que estés utilizando.  

5. **Configurar las referencias para evitar conflictos**  
   - En las propiedades de cada referencia (`AcDbMgd.dll`, `AcMgd.dll`, `AcCoreMgd.dll`):  
     - Busca la opción **"Copiar Local"** y configúrala en `False`.  
     - Esto evita conflictos al cargar la aplicación en AutoCAD.  

Una vez completados estos pasos, tu entorno estará listo para desarrollar aplicaciones en AutoCAD con .NET Framework 8. 🚀

### Compilación y Despliegue de la DLL  

Para compilar y cargar correctamente tu DLL en AutoCAD, sigue estos pasos:  

1. **Configurar el proyecto para generar la DLL correctamente**  
   - En Visual Studio, haz clic derecho sobre el proyecto y selecciona **Propiedades**.  
   - En la pestaña **Aplicación**, establece el **Tipo de salida** como `Biblioteca de clases`.  
   - En la pestaña **Compilación**, configura la **Plataforma de destino** en `x64` (si AutoCAD es de 64 bits).  

2. **Compilar el proyecto**  
   - Usa el atajo de teclado: `Ctrl + Shift + B`.  
   - Esto generará el archivo **AutoCADPlugin.dll** en la carpeta de salida.  

3. **Cargar la DLL en AutoCAD**  
   - Abre AutoCAD y escribe el comando:  
     ```
     NETLOAD
     ```
   - Se abrirá un cuadro de diálogo donde debes seleccionar la DLL compilada.  
   - La DLL se encuentra en la siguiente ruta dentro de tu proyecto:  
     ```
     bin\Debug\AutoCADPlugin.dll
     ```

4. **Ejecutar el comando en AutoCAD**  
   - Una vez cargada la DLL, en la consola de AutoCAD escribe el nombre del comando que programaste y presiona `Enter`.  
   - Esto ejecutará las instrucciones definidas en tu código.  

Siguiendo estos pasos, podrás compilar y ejecutar comandos personalizados en AutoCAD sin problemas. 🚀