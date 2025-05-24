using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using DBObject = Autodesk.AutoCAD.DatabaseServices.DBObject;
using Entity = Autodesk.AutoCAD.DatabaseServices.Entity;

namespace Civil3D.Autodesk
{
    internal static class DbUtilities
    {
        internal static readonly Database Db = Application.DocumentManager.MdiActiveDocument.Database;

        internal static List<CogoPoint> SelectCogoPoints(Func<CogoPoint, bool> filter)
        {
            var cogoPoints = new List<CogoPoint>();
            using (var transaction = Db.TransactionManager.StartTransaction())
            {
                var blockTable = transaction.GetObject(Db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var modelSpace =
                    transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as
                        BlockTableRecord;

                foreach (var pointId in CivilApplication.ActiveDocument.CogoPoints)
                {
                    var cogoPoint = transaction.GetObject(pointId, OpenMode.ForRead) as CogoPoint;

                    if (cogoPoint != null && filter(cogoPoint))
                    {
                        cogoPoints.Add(cogoPoint);
                    }
                }
            }

            return cogoPoints;
        }

        internal static T GetObject<T>(ObjectId objectId, OpenMode openMode, Transaction transaction = null)
            where T : DBObject
        {
            if (transaction != null)
            {
                return transaction.GetObject(objectId, openMode) as T;
            }

            if (openMode == OpenMode.ForWrite)
            {
                throw new InvalidOperationException("Write mode requires an external transaction.");
            }

            using (var tempTransaction = Db.TransactionManager.StartTransaction())
            {
                var obj = tempTransaction.GetObject(objectId, openMode) as T;
                return obj;
            }
        }

        internal static ObjectId AddEntityToModelSpace(Entity entity)
        {
            using (var transaction = Db.TransactionManager.StartTransaction())
            {
                var blockTable = transaction.GetObject(Db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var modelSpace =
                    transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as
                        BlockTableRecord;

                var id = modelSpace.AppendEntity(entity);
                transaction.AddNewlyCreatedDBObject(entity, true);
                transaction.Commit();

                return id;
            }
        }
    }
}