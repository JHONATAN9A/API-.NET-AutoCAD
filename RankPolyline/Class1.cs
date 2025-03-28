using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Colors;


namespace RankPolyline
{
    public class Class1
    {
        [CommandMethod("RankPolyline")]
        public void RankPolyline()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            ed.WriteMessage("\n🚀 El comando RankPolyline ha iniciado.");

            Dictionary<string, List<object>> polylineData = new Dictionary<string, List<object>>
            {
                { "Long_200", new List<object>() },
                { "Long_200_to_1000", new List<object>() },
                { "Long_1000", new List<object>() },
                { "Vert_menor_10", new List<object>() },
                { "Vert_mayor_10", new List<object>() },
                { "close_polyline", new List<object>() },
                { "open_polyline", new List<object>() }
            };

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                ed.WriteMessage("\n🔄 Iniciando transacción...");

                UnitsValue currentUnits = db.Insunits;
                if (currentUnits != UnitsValue.Feet)
                {
                    db.Insunits = UnitsValue.Feet;
                    ed.WriteMessage("\n📏 Unidades ajustadas a pies.");
                }

                LayerTable layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                string[] capas = layerTable.Cast<ObjectId>()
                                           .Select(id => ((LayerTableRecord)tr.GetObject(id, OpenMode.ForRead)).Name)
                                           .ToArray();
                string namePolylineLayer = SelectLayer(capas, "Polilinea");
                ed.WriteMessage("\n🎨 Capa seleccionada: " + namePolylineLayer);

                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord space = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                ed.WriteMessage("\n📊 Analizando polilíneas...");

                foreach (ObjectId objId in space)
                {
                    Entity ent = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                    if (ent is Polyline polyline && ent.Layer == namePolylineLayer)
                    {
                        double lengthFt = Math.Round(polyline.Length, 2);
                        int vertexCount = polyline.NumberOfVertices;
                        bool isClosed = polyline.Closed;

                        ed.WriteMessage($"\n📏 Longitud: {lengthFt}, 🔺 Vértices: {vertexCount}, 🔒 Es cerrado: {isClosed}");
                        polyline.UpgradeOpen();

                        if (lengthFt < 200)
                        {
                            polylineData["Long_200"].Add(lengthFt);
                            polyline.Color = Color.FromColorIndex(ColorMethod.ByAci, 1);
                        }
                        else if (lengthFt >= 200 && lengthFt < 1000)
                        {
                            polylineData["Long_200_to_1000"].Add(lengthFt);
                            polyline.Color = Color.FromColorIndex(ColorMethod.ByAci, 2);
                        }
                        else
                        {
                            polylineData["Long_1000"].Add(lengthFt);
                            polyline.Color = Color.FromColorIndex(ColorMethod.ByAci, 3);
                        }

                        if (vertexCount <= 10)
                            polylineData["Vert_menor_10"].Add(vertexCount);
                        else
                            polylineData["Vert_mayor_10"].Add(vertexCount);

                        if (isClosed)
                            polylineData["close_polyline"].Add(vertexCount);
                        else
                            polylineData["open_polyline"].Add(vertexCount);
                    }
                }

                ed.WriteMessage("\n📂 Exportando datos a CSV...");
                ExportToCsv(polylineData, @".\reultCommand.csv", ed);

                tr.Commit();
                ed.WriteMessage("\n✅ Transacción completada con éxito.");
            }
        }

        public static string SelectLayer(string[] capas, string option = "")
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            PromptKeywordOptions opciones = new PromptKeywordOptions($"\n🔍 Seleccione la capa {option}:");
            foreach (string capa in capas)
            {
                opciones.Keywords.Add(capa);
            }
            opciones.AllowNone = false;

            PromptResult resultado = ed.GetKeywords(opciones);
            ed.WriteMessage("\n🎨 Capa seleccionada: " + resultado.StringResult);
            return resultado.Status == PromptStatus.OK ? resultado.StringResult : null;
        }

        static void ExportToCsv(Dictionary<string, List<object>> data, string filePath, Editor ed)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Longitud <200, 200-1000, >1000, Vértices <=10, >10, Cerradas, Abiertas");
                writer.WriteLine($"{data["Long_200"].Count}, {data["Long_200_to_1000"].Count}, {data["Long_1000"].Count}, " +
                                 $"{data["Vert_menor_10"].Count}, {data["Vert_mayor_10"].Count}, {data["close_polyline"].Count}, {data["open_polyline"].Count}");
                ed.WriteMessage($"\n✅ Archivo CSV exportado en: {filePath}");
            }
        }
    }
}