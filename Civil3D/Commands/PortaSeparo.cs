using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Civil3D
{
    /// <summary>
    /// Splits a selected polyline segment to isolate a portion and creates a new segment labeled as a gate.
    /// </summary>
    /// <remarks>
    /// The command lets the user select a segment of a polyline (typically a fence), splits it,
    /// and creates a new polyline in a specific gate layer to represent a gate opening.
    /// </remarks>
    public class PortaSeparo
    {
        private readonly Editor _editor;
        private readonly Database _database;

        public PortaSeparo()
        {
            _editor = Application.DocumentManager.MdiActiveDocument.Editor;
            _database = Application.DocumentManager.MdiActiveDocument.Database;
        }

        [CommandMethod("PortaSeparo")]
        public void SeparateGateFromFence()
        {
            short originalSelectionPreview = (short)Application.GetSystemVariable("SELECTIONPREVIEW");

            try
            {
                Application.SetSystemVariable("SELECTIONPREVIEW", 0);

                (Point3d StartPoint, Point3d EndPoint, ObjectId PolylineId)? selection = SelectPolylineSegment();
                if (selection == null) return;

                (Point3d startPoint, Point3d endPoint, ObjectId polylineId) = selection.Value;

                SplitPolylineAtPoints(polylineId, startPoint, endPoint);

                CreateNewPolyline(startPoint, endPoint, "_ჭიშკარი");
            }
            catch (System.Exception ex)
            {
                _editor.WriteMessage($"\n*Error*: {ex}");
            }
            finally
            {
                Application.SetSystemVariable("SELECTIONPREVIEW", originalSelectionPreview);
            }
        }

        private (Point3d StartPoint, Point3d EndPoint, ObjectId PolylineId)? SelectPolylineSegment()
        {
            PromptEntityOptions entityOptions = new PromptEntityOptions("\nSelect a polyline segment:");
            entityOptions.SetRejectMessage("\nThe selected object is not a 2D polyline.");
            entityOptions.AddAllowedClass(typeof(Polyline), true);
            entityOptions.AllowNone = false;

            PromptEntityResult entityResult = _editor.GetEntity(entityOptions);
            if (entityResult.Status != PromptStatus.OK) return null;

            ObjectId polylineId = entityResult.ObjectId;

            using (Transaction trans = _database.TransactionManager.StartTransaction())
            {
                Polyline polyline = trans.GetObject(polylineId, OpenMode.ForRead) as Polyline
                    ?? throw new System.Exception("The selected object is not a valid 2D polyline.");

                Point3d pickedPoint = entityResult.PickedPoint;

                LineSegment3d closestSegment = null;
                double closestDistance = double.MaxValue;

                for (int i = 0; i < polyline.NumberOfVertices - 1; i++)
                {
                    Point3d start = polyline.GetPoint3dAt(i);
                    Point3d end = polyline.GetPoint3dAt(i + 1);

                    LineSegment3d segment = new LineSegment3d(start, end);

                    Point3d closestPoint = segment.GetClosestPointTo(pickedPoint).Point;
                    double distance = closestPoint.DistanceTo(pickedPoint);

                    if (distance < closestDistance)
                    {
                        closestSegment = segment;
                        closestDistance = distance;
                    }
                }

                if (closestSegment == null)
                    throw new System.Exception("No valid segment could be identified.");


                trans.Commit();

                return (closestSegment.StartPoint, closestSegment.EndPoint, polylineId);
            }
        }

        private ObjectId CreateNewPolyline(Point3d startPoint, Point3d endPoint, string layerName = null)
        {
            using (Transaction trans = _database.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = trans.GetObject(_database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                string targetLayer;
                if (!string.IsNullOrEmpty(layerName))
                {
                    LayerTable layerTable = trans.GetObject(_database.LayerTableId, OpenMode.ForRead) as LayerTable;
                    if (!layerTable.Has(layerName))
                    {
                        throw new System.Exception($"Layer '{layerName}' does not exist in the drawing.");
                    }
                    targetLayer = layerName;
                }
                else
                {
                    LayerTableRecord currentLayer = trans.GetObject(_database.Clayer, OpenMode.ForRead) as LayerTableRecord;
                    targetLayer = currentLayer?.Name ?? "0";
                }

                Polyline newPolyline = new Polyline();
                newPolyline.AddVertexAt(0, new Point2d(startPoint.X, startPoint.Y), 0, 0, 0);
                newPolyline.AddVertexAt(1, new Point2d(endPoint.X, endPoint.Y), 0, 0, 0);
                newPolyline.Layer = targetLayer;

                ObjectId polylineId = modelSpace.AppendEntity(newPolyline);
                trans.AddNewlyCreatedDBObject(newPolyline, true);

                trans.Commit();
                return polylineId;
            }
        }

        private void SplitPolylineAtPoints(ObjectId polylineId, Point3d startPoint, Point3d endPoint)
        {
            using (Transaction trans = _database.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = trans.GetObject(_database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                Polyline originalPolyline = trans.GetObject(polylineId, OpenMode.ForWrite) as Polyline
                    ?? throw new System.Exception("The specified ObjectId does not exist or is not a valid Polyline.");

                int startIndex = -1, endIndex = -1;

                for (int i = 0; i < originalPolyline.NumberOfVertices; i++)
                {
                    Point3d vertex = originalPolyline.GetPoint3dAt(i);
                    if (vertex.DistanceTo(startPoint) < Tolerance.Global.EqualPoint) startIndex = i;
                    if (vertex.DistanceTo(endPoint) < Tolerance.Global.EqualPoint) endIndex = i;
                }

                if (startIndex == -1 || endIndex == -1)
                    throw new System.Exception("The start or end point does not match any vertex on the polyline.");

                if (startIndex >= endIndex)
                    throw new System.Exception("The start point must appear before the end point on the polyline.");

                Polyline polylinePart1 = new Polyline();
                Polyline polylinePart2 = new Polyline();

                for (int i = 0; i <= startIndex; i++)
                {
                    Point3d vertex = originalPolyline.GetPoint3dAt(i);
                    polylinePart1.AddVertexAt(polylinePart1.NumberOfVertices, new Point2d(vertex.X, vertex.Y), 0, 0, 0);
                }

                for (int i = endIndex; i < originalPolyline.NumberOfVertices; i++)
                {
                    Point3d vertex = originalPolyline.GetPoint3dAt(i);
                    polylinePart2.AddVertexAt(polylinePart2.NumberOfVertices, new Point2d(vertex.X, vertex.Y), 0, 0, 0);
                }

                modelSpace.AppendEntity(polylinePart1);
                trans.AddNewlyCreatedDBObject(polylinePart1, true);

                modelSpace.AppendEntity(polylinePart2);
                trans.AddNewlyCreatedDBObject(polylinePart2, true);

                originalPolyline.Erase();

                trans.Commit();
            }
        }
    }
}
