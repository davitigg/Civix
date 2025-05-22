using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Internal;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using System.Collections.Generic;
using Entity = Autodesk.AutoCAD.DatabaseServices.Entity;

namespace Civil3D
{
    /// <summary>
    /// Inserts predefined block symbols at COGO point locations based on their RawDescription values.
    /// </summary>
    /// <remarks>
    /// The command reads COGO points in the drawing, matches their descriptions to block names, and inserts
    /// corresponding blocks at each point’s location, if not already present.
    /// </remarks>
    public class AccioSymbols
    {
        private readonly Dictionary<string, string> _pointDescriptionToBlockMap = new Dictionary<string, string>()
        {
            { "11", "ჭა" },   { "13", "ელ. ბოძი" },   { "14", "ხე" },
        };
        private readonly List<string> _objectScaleList = new List<string>()
        {
            "1:100", "1:200", "1:250", "1:500", "1:1000"
        };

        private readonly Editor _editor;
        private readonly Database _database;

        public AccioSymbols()
        {
            _editor = Application.DocumentManager.MdiActiveDocument.Editor;
            _database = Application.DocumentManager.MdiActiveDocument.Database;
        }

        [CommandMethod("AccioSymbols")]
        public void InsertMapSymbols()
        {
            try
            {
                using (Transaction transaction = _database.TransactionManager.StartTransaction())
                {
                    BlockTable blockTable = transaction.GetObject(_database.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord modelSpace = transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    int objectsPlaced = 0;
                    foreach (ObjectId pointId in CivilApplication.ActiveDocument.CogoPoints)
                    {
                        CogoPoint cogoPoint = transaction.GetObject(pointId, OpenMode.ForRead) as CogoPoint;

                        if (cogoPoint != null && _pointDescriptionToBlockMap.TryGetValue(cogoPoint.RawDescription.Replace("*", ""), out string blockName))
                        {
                            Point3d blockLocation = new Point3d(cogoPoint.Location.X, cogoPoint.Location.Y, 0);

                            if (!BlockExistsAtLocation(transaction, modelSpace, blockLocation, blockName))
                            {
                                InsertBlock(transaction, blockTable, modelSpace, blockLocation, blockName);
                                objectsPlaced++;
                            }
                        }
                    }

                    transaction.Commit();
                    _editor.WriteMessage($"\n{objectsPlaced} blocks placed.");
                }
            }
            catch (System.Exception ex)
            {
                _editor.WriteMessage($"\n*Error*: {ex}");
            }
        }

        private bool BlockExistsAtLocation(Transaction transaction, BlockTableRecord targetSpace, Point3d location, string blockName)
        {
            foreach (ObjectId entityId in targetSpace)
            {
                Entity entity = transaction.GetObject(entityId, OpenMode.ForRead) as Entity;

                if (entity is BlockReference blockReference &&
                    blockReference.Position.IsEqualTo(location, Tolerance.Global) &&
                    blockReference.Name == blockName)
                {
                    return true;
                }
            }

            return false;
        }

        private void InsertBlock(Transaction transaction, BlockTable blockTable, BlockTableRecord targetSpace, Point3d location, string blockName)
        {
            if (blockTable.Has(blockName))
            {
                BlockTableRecord blockTableRecord = transaction.GetObject(blockTable[blockName], OpenMode.ForRead) as BlockTableRecord;

                BlockReference blockReference = new BlockReference(location, blockTableRecord.ObjectId);

                if (blockReference.Annotative == AnnotativeStates.True)
                {
                    ObjectContextManager objectContextManager = _database.ObjectContextManager;
                    ObjectContextCollection objectContextCollection = objectContextManager.GetContextCollection("ACDB_ANNOTATIONSCALES");
                    ObjectContexts.AddContext(blockReference, objectContextCollection.CurrentContext);
                    foreach (var scale in _objectScaleList)
                    {
                        ObjectContext objectContext = objectContextCollection.GetContext(scale);
                        if (objectContext != null) ObjectContexts.AddContext(blockReference, objectContext);
                    }
                }

                targetSpace.AppendEntity(blockReference);
                transaction.AddNewlyCreatedDBObject(blockReference, true);
            }
            else
            {
                throw new System.Exception($"Block '{blockName}' not found in the drawing.");
            }
        }
    }
}
