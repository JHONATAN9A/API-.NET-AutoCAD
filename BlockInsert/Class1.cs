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

            ed.WriteMessage("\nEl comando InsertBlockOnPolyline ha iniciado.");

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // Seleccionar capas de interes
                LayerTable layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                string[] capas = layerTable.Cast<ObjectId>()
                                           .Select(id => ((LayerTableRecord)tr.GetObject(id, OpenMode.ForRead)).Name)
                                           .ToArray();

                string namePolylineLayer = SelectLayer(capas, "Polilinea");
                string nameBlockLayer = SelectLayer(capas, "Bloque");
                ed.WriteMessage($"\n Capa 1:{namePolylineLayer}, Capa 2: {nameBlockLayer}");

                // Seleccionar el bloque y recorrer las Polilineas
                BlockTableRecord space = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);

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

                foreach (ObjectId objId in space)
                {
                    Entity entidad = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                    if (entidad is Polyline poly && poly.Layer == namePolylineLayer)
                    {
                        // Obtener la coordenada inicial y final
                        Point3d puntoInicial = poly.GetPoint3dAt(0);
                        Point3d puntoFinal = poly.GetPoint3dAt(poly.NumberOfVertices - 1);

                        ed.WriteMessage($"\nPolilínea - Inicio: {puntoInicial}, Fin: {puntoFinal}");
                        DrawBlock(selectBlock, puntoInicial, space, tr);
                        DrawBlock(selectBlock, puntoFinal, space, tr);
                    }
                }

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

        private HashSet<Point3d> pointIntersect = new HashSet<Point3d>();
        private void DrawBlock(Entity selectBlock, Point3d coordinate, BlockTableRecord espacioModelo, Transaction tr)
        {
            if (pointIntersect.Contains(coordinate))
                return; // Evita insertar duplicados

            pointIntersect.Add(coordinate); // Registra la coordenada

            DBPoint punto = new DBPoint(coordinate);
            punto.SetDatabaseDefaults();
            punto.ColorIndex = 1; // 1 = Rojo

            espacioModelo.AppendEntity(punto);
            tr.AddNewlyCreatedDBObject(punto, true);

            // Clonar el bloque
            Entity copyBlock = selectBlock.Clone() as Entity;
            if (copyBlock == null) return;

            // Obtener el punto central del bloque
            Point3d centro = getCenter(selectBlock);

            // Calcular el desplazamiento desde el centro
            Vector3d desplazamiento = coordinate - centro;
            copyBlock.TransformBy(Matrix3d.Displacement(desplazamiento));

            // Agregar la copia al dibujo
            espacioModelo.AppendEntity(copyBlock);
            tr.AddNewlyCreatedDBObject(copyBlock, true);
        }

        private Point3d getCenter(Entity entidad)
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


