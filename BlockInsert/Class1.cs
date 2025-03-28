using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;


namespace BlockInsert
{
    public class Class1
    {
        [CommandMethod("InsertBlock")]
        public void InsertBlock()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            ed.WriteMessage("\n🚀 El comando InsertBlock ha iniciado.");

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                ed.WriteMessage("\n📂 Obteniendo capas disponibles...");
                LayerTable layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                string[] capas = layerTable.Cast<ObjectId>()
                                           .Select(id => ((LayerTableRecord)tr.GetObject(id, OpenMode.ForRead)).Name)
                                           .ToArray();

                string namePolylineLayer = SelectLayer(capas, "Polilinea");
                string nameBlockLayer = SelectLayer(capas, "Bloque");
                ed.WriteMessage($"\n✅ Capas seleccionadas - Polilínea: {namePolylineLayer}, Bloque: {nameBlockLayer}");

                BlockTableRecord space = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                ed.WriteMessage("\n🔍 Buscando bloque de referencia...");
                Entity selectBlock = null;

                foreach (ObjectId objId in space)
                {
                    Entity entidad = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                    if (entidad != null && entidad.Layer == nameBlockLayer)
                    {
                        selectBlock = entidad;
                        break;
                    }
                }

                if (selectBlock == null)
                {
                    ed.WriteMessage("\n⚠️ No se encontró un bloque en la capa especificada.");
                    return;
                }

                ed.WriteMessage("\n📏 Procesando polilíneas...");
                foreach (ObjectId objId in space)
                {
                    Entity entidad = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                    if (entidad is Polyline poly && poly.Layer == namePolylineLayer)
                    {
                        Point3d puntoInicial = poly.GetPoint3dAt(0);
                        Point3d puntoFinal = poly.GetPoint3dAt(poly.NumberOfVertices - 1);

                        ed.WriteMessage($"\n🟢 Polilínea detectada - Inicio: {puntoInicial}, Fin: {puntoFinal}");
                        DrawBlock(selectBlock, puntoInicial, space, tr, ed);
                        DrawBlock(selectBlock, puntoFinal, space, tr, ed);
                    }
                }

                tr.Commit();
                ed.WriteMessage("\n✅ Transacción completada exitosamente.");
            }
        }

        public static string SelectLayer(string[] capas, string option = "")
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            PromptKeywordOptions opciones = new PromptKeywordOptions($"\n🗂️ Seleccione la capa {option}:");
            foreach (string capa in capas)
            {
                opciones.Keywords.Add(capa);
            }
            opciones.AllowNone = false;

            PromptResult resultado = ed.GetKeywords(opciones);
            ed.WriteMessage("\n✅ Capa seleccionada: " + resultado.StringResult);
            return resultado.Status == PromptStatus.OK ? resultado.StringResult : null;
        }

        private HashSet<Point3d> pointIntersect = new HashSet<Point3d>();
        private void DrawBlock(Entity selectBlock, Point3d coordinate, BlockTableRecord espacioModelo, Transaction tr, Editor ed)
        {
            if (pointIntersect.Contains(coordinate))
            {
                ed.WriteMessage("\n⚠️ Bloque ya insertado en esta coordenada.");
                return;
            }

            pointIntersect.Add(coordinate);
            ed.WriteMessage($"\n📍 Insertando bloque en: {coordinate}");

            DBPoint punto = new DBPoint(coordinate);
            punto.SetDatabaseDefaults();
            punto.ColorIndex = 1;

            espacioModelo.AppendEntity(punto);
            tr.AddNewlyCreatedDBObject(punto, true);

            Entity copyBlock = selectBlock.Clone() as Entity;
            if (copyBlock == null) return;

            Point3d centro = GetCenter(selectBlock);
            Vector3d desplazamiento = coordinate - centro;
            copyBlock.TransformBy(Matrix3d.Displacement(desplazamiento));

            espacioModelo.AppendEntity(copyBlock);
            tr.AddNewlyCreatedDBObject(copyBlock, true);
        }

        private Point3d GetCenter(Entity entidad)
        {
            if (entidad.Bounds.HasValue)
            {
                Extents3d bounds = entidad.Bounds.Value;
                return new Point3d(
                    (bounds.MinPoint.X + bounds.MaxPoint.X) / 2,
                    (bounds.MinPoint.Y + bounds.MaxPoint.Y) / 2,
                    (bounds.MinPoint.Z + bounds.MaxPoint.Z) / 2
                );
            }
            return Point3d.Origin;
        }
    }
}


