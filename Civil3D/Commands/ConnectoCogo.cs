using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.DatabaseServices;
using System.Collections.Generic;
using System.Linq;

namespace Civil3D.Commands
{
    /// <summary>
    /// Connects selected COGO points into categorized 3D polylines based on their descriptions.
    /// </summary>
    /// <remarks>
    /// Each polyline represents a specific entity (e.g., road, fence) and is created or extended
    /// incrementally from user-selected COGO points matching pre-defined description codes.
    /// </remarks>
    public class ConnectoCogo
    {
        private readonly Dictionary<string, List<string>> _entityToPointDescriptionMap = new Dictionary<string, List<string>>()
        {
            { "გზა", new List<string>() { "2", "9" } },
            { "ღობე", new List<string>() { "3", "3-" } },
            { "შენობა", new List<string>() { "4", "4-" } },
            { "სანიაღვრის თავი", new List<string>() { "7" } },
            { "სანიაღვრის ძირი", new List<string>() { "8" } },
            { "კედლის თავი", new List<string>() { "21" } },
            { "კედლის ძირი", new List<string>() { "22" } },
            { "გაზის მილი", new List<string>() { "15" } }
        };

        private readonly Editor _editor;
        private readonly Database _database;

        public ConnectoCogo()
        {
            _editor = Application.DocumentManager.MdiActiveDocument.Editor;
            _database = Application.DocumentManager.MdiActiveDocument.Database;
        }

        [CommandMethod("ConnectoCogo")]
        public void ConnectCogoPoints()
        {
            try
            {
                Dictionary<string, ObjectId?> entityToPolylineIdMap = new Dictionary<string, ObjectId?>();
                List<string> pointDescriptionFilters = new List<string>();

                foreach (string entity in _entityToPointDescriptionMap.Keys.ToList())
                {
                    entityToPolylineIdMap.Add(entity, null);
                    pointDescriptionFilters.AddRange(_entityToPointDescriptionMap[entity]);
                }

                while (true)
                {
                    List<CogoPoint> selectedCogoPoints = SelectCogoPoints(pointDescriptionFilters);

                    if (selectedCogoPoints == null) break; // nothing selected
                    if (selectedCogoPoints.Count == 0) continue; // no valid points selected

                    foreach (string entity in entityToPolylineIdMap.Keys.ToList())
                    {
                        ObjectId? polylineId = entityToPolylineIdMap[entity];
                        List<CogoPoint> entitySelectedCogoPoints = selectedCogoPoints
                            .Where(p => _entityToPointDescriptionMap[entity].Any(d => d == p.RawDescription.Replace("*", "")))
                            .ToList();

                        if (entitySelectedCogoPoints.Count == 0) continue;

                        if (polylineId == null)
                        {
                            polylineId = Create3DPolyline(entitySelectedCogoPoints);
                            entityToPolylineIdMap[entity] = polylineId;
                        }
                        else
                        {
                            ExtendPolyline(polylineId.Value, entitySelectedCogoPoints);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                _editor.WriteMessage($"\n*Error*: {ex}");
            }
        }

        private List<CogoPoint> SelectCogoPoints(List<string> descriptionFilters)
        {
            PromptSelectionOptions selectionOptions = new PromptSelectionOptions
            {
                MessageForAdding = "\nSelect COGO points (Press Enter without selecting to finish):"
            };

            TypedValue[] filter = { new TypedValue((int)DxfCode.Start, "AECC_COGO_POINT") };
            SelectionFilter selectionFilter = new SelectionFilter(filter);

            PromptSelectionResult selectionResult = _editor.GetSelection(selectionOptions, selectionFilter);
            if (selectionResult.Status != PromptStatus.OK) return null;

            List<CogoPoint> cogoPoints = new List<CogoPoint>();

            using (Transaction trans = _database.TransactionManager.StartTransaction())
            {
                foreach (SelectedObject selectedObject in selectionResult.Value)
                {
                    if (selectedObject != null)
                    {
                        CogoPoint cogoPoint = trans.GetObject(selectedObject.ObjectId, OpenMode.ForRead) as CogoPoint;
                        if (cogoPoint != null && descriptionFilters.Any(d => d == cogoPoint.RawDescription.Replace("*", "")))
                        {
                            cogoPoints.Add(cogoPoint);
                        }
                    }
                }

                trans.Commit();
            }

            return cogoPoints.OrderBy(pt => pt.PointNumber).ToList();
        }

        private ObjectId Create3DPolyline(List<CogoPoint> cogoPoints)
        {
            if (cogoPoints == null || cogoPoints.Count == 0)
            {
                throw new System.Exception("No points provided to create the Polyline3d.");
            }

            ObjectId polylineId;

            using (Transaction trans = _database.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = trans.GetObject(_database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                using (Polyline3d polyline = new Polyline3d(Poly3dType.SimplePoly, new Point3dCollection(), false))
                {
                    polylineId = modelSpace.AppendEntity(polyline);
                    trans.AddNewlyCreatedDBObject(polyline, true);

                    foreach (CogoPoint cogoPoint in cogoPoints)
                    {
                        Point3d point = new Point3d(cogoPoint.Location.X, cogoPoint.Location.Y, cogoPoint.Elevation);
                        PolylineVertex3d vertex = new PolylineVertex3d(point);
                        polyline.AppendVertex(vertex);
                        trans.AddNewlyCreatedDBObject(vertex, true);
                    }
                }

                trans.Commit();
            }

            return polylineId;
        }

        private void ExtendPolyline(ObjectId polylineId, List<CogoPoint> additionalPoints)
        {
            if (additionalPoints == null || additionalPoints.Count == 0)
            {
                throw new System.Exception("No additional points provided to extend the Polyline3d.");
            }

            using (Transaction trans = _database.TransactionManager.StartTransaction())
            {
                Polyline3d polyline = trans.GetObject(polylineId, OpenMode.ForWrite) as Polyline3d
                                      ?? throw new System.Exception("The specified ObjectId does not exist or is not a valid Polyline3d.");

                HashSet<Point3d> existingVertices = new HashSet<Point3d>();
                foreach (ObjectId vertexId in polyline)
                {
                    PolylineVertex3d existingVertex = trans.GetObject(vertexId, OpenMode.ForRead) as PolylineVertex3d;
                    if (existingVertex != null)
                    {
                        existingVertices.Add(existingVertex.Position);
                    }
                }

                foreach (CogoPoint cogoPoint in additionalPoints)
                {
                    Point3d point = new Point3d(cogoPoint.Location.X, cogoPoint.Location.Y, cogoPoint.Elevation);
                    if (!existingVertices.Contains(point))
                    {
                        PolylineVertex3d vertex = new PolylineVertex3d(point);
                        polyline.AppendVertex(vertex);
                        trans.AddNewlyCreatedDBObject(vertex, true);
                    }
                }

                trans.Commit();
            }
        }
    }
}