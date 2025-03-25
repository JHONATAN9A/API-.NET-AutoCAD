using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

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
            ed.WriteMessage("\nEl comando RankPolyline ha iniciado.");

            Dictionary<string, List<object>> polylineData = new Dictionary<string, List<object>>
            {
                { "Long_200", new List<object>() }, // Longitud menor de 200 ft
                { "Long_200_to_1000", new List<object>() }, // Longitud entre 200 y 1000 ft
                { "Long_1000", new List<object>() }, // Longitud mayor de 1000 ft
                { "Vert_menor_10", new List<object>() }, // Número de vertices menor o igual a 10
                { "Vert_mayor_10", new List<object>() }, // Número de vertices mayor de 10
                { "close_polyline", new List<object>() }, // Si son polilíneas cerradas
                { "open_polyline", new List<object>() } // Si son polilíneas abiertas
            };


            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // Ajustar unidade de AutoCAD a ft
                UnitsValue currentUnits = db.Insunits;
                if (currentUnits != UnitsValue.Feet)
                {
                    db.Insunits = UnitsValue.Feet;
                }

                // Seleccionar capas de interes
                LayerTable layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                string[] capas = layerTable.Cast<ObjectId>()
                                           .Select(id => ((LayerTableRecord)tr.GetObject(id, OpenMode.ForRead)).Name)
                                           .ToArray();

                string namePolylineLayer = SelectLayer(capas, "Polilinea");

                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord Space = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                foreach (ObjectId objId in Space)
                {
                    Entity ent = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                    if (ent is Polyline polyline && ent.Layer == namePolylineLayer)
                    {
                        double lengthFt = Math.Round(polyline.Length, 2); 
                        int vertexCount = polyline.NumberOfVertices;
                        bool isClosed = polyline.Closed;

                        ed.WriteMessage($"\n Longitud: {lengthFt}, Vertices: {vertexCount}, Es cerrado: {isClosed}");

                        if (lengthFt < 200) polylineData["Long_200"].Add(lengthFt);
                        if (lengthFt >= 200 & lengthFt < 1000 ) polylineData["Long_200_to_1000"].Add(lengthFt);
                        if (lengthFt > 1000) polylineData["Long_1000"].Add(lengthFt);
                        if (vertexCount <= 10) polylineData["Vert_menor_10"].Add(vertexCount);
                        if (vertexCount > 10) polylineData["Vert_mayor_10"].Add(vertexCount);
                        if (isClosed) polylineData["close_polyline"].Add(vertexCount);
                        if (!isClosed) polylineData["open_polyline"].Add(vertexCount);


                    }
                }
                ExportToCsv(polylineData, @".\reultCommand.csv", ed);

                tr.Commit();
            }

            

        }

        public static string SelectLayer(string[] capas, string Option = "")
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            PromptKeywordOptions opciones = new PromptKeywordOptions($"\nSeleccione la capa {Option}:");
            foreach (string capa in capas)
            {
                opciones.Keywords.Add(capa);
            }
            opciones.AllowNone = false;

            PromptResult resultado = ed.GetKeywords(opciones);
            return resultado.Status == PromptStatus.OK ? resultado.StringResult : null;
        }

        static void ExportToCsv(Dictionary<string, List<object>> data, string filePath, Editor ed)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {

                string Long_200 = data["Long_200"].Count.ToString();
                string Long_200_to_1000 = data["Long_200_to_1000"].Count.ToString();
                string Long_1000 = data["Long_1000"].Count.ToString();
                string Vert_menor_10 = data["Vert_menor_10"].Count.ToString();
                string Vert_mayor_10 = data["Vert_mayor_10"].Count.ToString();
                string close_polyline = data["close_polyline"].Count.ToString();
                string open_polyline = data["open_polyline"].Count.ToString();


                writer.WriteLine("Longitud menor de 200 (ft), Longitud entre 200 y 1000 (ft), Longitud mayor de 1000 (ft), Número de vertices menor o igual a 10, Número de vertices mayor de 10, Si son polilíneas cerradas, Si son polilíneas abiertas");
                writer.WriteLine($"{Long_200}, {Long_200_to_1000}, {Long_1000}, {Vert_menor_10}, {Vert_mayor_10}, {close_polyline}, {open_polyline}");

                ed.WriteMessage($"✅ Archivo CSV exportado en: {filePath}");
            }
        }

    }
}
