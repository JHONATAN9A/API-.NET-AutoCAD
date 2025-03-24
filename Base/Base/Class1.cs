using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace Base
{
    public class Class1
    {
        [CommandMethod("AutoCAD_I")]
        public void Hello()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            ed.WriteMessage("\n¡Hola, AutoCAD desde .NET!");
        }
    }
}
