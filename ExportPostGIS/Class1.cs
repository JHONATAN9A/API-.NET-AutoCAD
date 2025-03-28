using System.Reflection.Emit;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Npgsql;

using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace ExportPostGIS
{
    public class LayerGeometry
    {
        public long id_layer { get; set; }
        public string name_layer { get; set; } = string.Empty;
        public string color_layer { get; set; } = string.Empty;
        public string type_layer { get; set; } = string.Empty;
        public string geometry { get; set; } = string.Empty;
    }

    public class Class1
    {
        [CommandMethod("ExportPostGIS")]
        public void ExportPostGIS()
        {
            string host = Environment.GetEnvironmentVariable("DB_SERVER");
            string port = Environment.GetEnvironmentVariable("DB_PORT");
            string database = Environment.GetEnvironmentVariable("DB_NAME");
            string user = Environment.GetEnvironmentVariable("DB_USER");
            string password = Environment.GetEnvironmentVariable("DB_PASSWORD");
            string connString = $"Host={host};Port={port};Username={user};Password={password};Database={database}";

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            ed.WriteMessage("\n🔄 El comando ExportPostGIS ha iniciado...");

            List<LayerGeometry> geometries = new List<LayerGeometry>();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                LayerTable layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                BlockTable blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord modelSpace = (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                foreach (ObjectId layerId in layerTable)
                {
                    LayerTableRecord layer = (LayerTableRecord)tr.GetObject(layerId, OpenMode.ForRead);
                    string layerName = layer.Name;
                    string color = layer.Color.ToString();

                    foreach (ObjectId entId in modelSpace)
                    {
                        Entity ent = (Entity)tr.GetObject(entId, OpenMode.ForRead);
                        if (ent.Layer == layerName)
                        {
                            string wkt = ConvertToWKT(ent);

                            if (!string.IsNullOrEmpty(wkt))
                            {
                                ed.WriteMessage($"\n🧩 WKT generado: {wkt}");

                                geometries.Add(new LayerGeometry
                                {
                                    id_layer = layerId.Handle.Value,
                                    name_layer = layerName,
                                    color_layer = color,
                                    type_layer = ent.GetType().Name,
                                    geometry = wkt
                                });
                            }
                        }
                    }
                }

                tr.Commit();
            }

            LoadPostgresSQL(connString, geometries, ed);

            ed.WriteMessage("\n✅ Proceso finalizado.");
        }

        private string ConvertToWKT(Entity entity)
        {
            if (entity is Line line)
            {
                return $"LINESTRING({line.StartPoint.X} {line.StartPoint.Y}, {line.EndPoint.X} {line.EndPoint.Y})";
            }

            if (entity is Polyline poly)
            {
                if (poly.Closed)
                {
                    string wkt = "POLYGON((";
                    for (int i = 0; i < poly.NumberOfVertices; i++)
                    {
                        Point2d pt = poly.GetPoint2dAt(i);
                        wkt += $"{pt.X} {pt.Y},";
                    }
                    // Cerrar el polígono correctamente
                    Point2d firstPt = poly.GetPoint2dAt(0);
                    wkt += $"{firstPt.X} {firstPt.Y}))";
                    return wkt;
                }
                else
                {
                    string wkt = "LINESTRING(";
                    for (int i = 0; i < poly.NumberOfVertices; i++)
                    {
                        Point2d pt = poly.GetPoint2dAt(i);
                        wkt += $"{pt.X} {pt.Y},";
                    }
                    wkt = wkt.TrimEnd(',') + ")";
                    return wkt;
                }
            }

            if (entity is DBPoint point)
            {
                return $"POINT({point.Position.X} {point.Position.Y})";
            }

            
            return null;
        }

        private void LoadPostgresSQL(string connCredentials, List<LayerGeometry> layers, Editor ed)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connCredentials))
                {
                    conn.Open();
                    ed.WriteMessage("\n📡 Conexión abierta a PostgreSQL...");

                    using var transaction = conn.BeginTransaction();

                    using var cmd = new NpgsqlCommand(@"
                    SET search_path TO public, postgis;
                    INSERT INTO test.autocad (id_layer, name_layer, color_layer, type_layer, geometry)
                    VALUES (@id, @name, @color, @type, test.ST_GeomFromText(@geom)::test.geometry(geometry, 2236));", conn, transaction);

                    foreach (var layer in layers)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("id", layer.id_layer);
                        cmd.Parameters.AddWithValue("name", layer.name_layer);
                        cmd.Parameters.AddWithValue("color", layer.color_layer);
                        cmd.Parameters.AddWithValue("type", layer.type_layer);
                        cmd.Parameters.AddWithValue("geom", layer.geometry);

                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    ed.WriteMessage("\n✅ Todos los datos fueron insertados correctamente.");
                }
            }
            catch (Exception ex)
            {
                ed.WriteMessage($"\n❌ Error en la conexión o inserción: {ex.Message}");
            }
        }
    }
}
