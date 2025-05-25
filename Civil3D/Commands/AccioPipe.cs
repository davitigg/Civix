using System;
using System.Text;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Civil3D.Autodesk;
using Civil3D.Enums;
using Civil3D.Forms;
using Civil3D.Utilities;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace Civil3D.Commands
{
    public class AccioPipe
    {
        private const string PipePointRawDescription = "10";
        private const string ExistingPipeLayer = "_მილი";
        private const string DesignPipeLayer = "_საპ. მილი";
        private const string MLeaderStyle = "ისარი";
        private const string TextStyle = "Sylfaen";

        [CommandMethod("AccioPipe")]
        public void InsertPipesAndLabels()
        {
            try
            {
                var startPointNumber = PromptUserForStartPoint();
                var cogoPoints = DbUtilities.SelectCogoPoints(p =>
                    p.PointNumber >= startPointNumber && p.RawDescription.Replace("*", "") == PipePointRawDescription);

                if (cogoPoints.Count < 2)
                {
                    EdUtilities.Ed.WriteMessage(
                        $"\nOnly {cogoPoints.Count} matching point(s) found." +
                        $" At least 2 are required to create a polyline." +
                        $" (Start point number: '{startPointNumber}', raw description: '{PipePointRawDescription}').");
                    return;
                }

                for (int i = 0; i < cogoPoints.Count - 1; i += 2)
                {
                    var point1 = new Point2d(cogoPoints[i].Location.X, cogoPoints[i].Location.Y);
                    var point2 = new Point2d(cogoPoints[i + 1].Location.X, cogoPoints[i + 1].Location.Y);
                    var currentPolylineId = CreatePolyline(point1, point2);

                    EdUtilities.PanTo(cogoPoints[i + 1].Location);

                    var existingLabelAdded = false;
                    var designLabelAdded = false;
                    var addExistingLabel = EdUtilities.PromptYesOrNo(
                        $"\nAdd existing pipe label to polyline ({cogoPoints[i].PointNumber}–{cogoPoints[i + 1].PointNumber})?");
                    if (addExistingLabel)
                    {
                        existingLabelAdded = TryAddPipeLabel(
                            currentPolylineId,
                            Status.Existing,
                            $"{cogoPoints[i].PointNumber}-{cogoPoints[i + 1].PointNumber} — არსებული მილის მონაცემები",
                            40);
                    }

                    var addDesignLabel = EdUtilities.PromptYesOrNo(
                        $"\nAdd design pipe label to polyline ({cogoPoints[i].PointNumber}–{cogoPoints[i + 1].PointNumber})?");
                    if (addDesignLabel)
                    {
                        designLabelAdded = TryAddPipeLabel(
                            currentPolylineId,
                            Status.Design,
                            $"{cogoPoints[i].PointNumber}-{cogoPoints[i + 1].PointNumber} — საპროექტო მილის მონაცემები",
                            50);
                    }

                    if (!existingLabelAdded && designLabelAdded)
                    {
                        ChangePolylineLayerToDesign(currentPolylineId);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.ErrorStatus == ErrorStatus.UserBreak)
                {
                    EdUtilities.Ed.WriteMessage("\nCommand canceled by user.");
                }
                else
                {
                    EdUtilities.Ed.WriteMessage($"\n*Error*: {ex}");
                }
            }
            catch (System.Exception ex)
            {
                EdUtilities.Ed.WriteMessage($"\n*Error*: {ex}");
            }
        }

        private uint PromptUserForStartPoint()
        {
            var options = new PromptIntegerOptions("\nEnter start point number:")
            {
                AllowNegative = false,
                AllowZero = false,
                AllowNone = false,
                LowerLimit = 1,
                DefaultValue = 1
            };

            var result = EdUtilities.Ed.GetInteger(options);

            if (result.Status != PromptStatus.OK) throw new Exception(ErrorStatus.UserBreak);

            return (uint)result.Value;
        }

        private string CreatePipeAnnotation(PipeAnnotationForm form, Status status, double length = 0)
        {
            var sb = new StringBuilder();

            switch (status)
            {
                case Status.Existing:
                    sb.Append("არს.");
                    break;
                case Status.Design:
                    sb.Append("საპ.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }

            sb.Append(" ").Append(form.Material.ToGenitive()).Append(" მილი\n");

            if (form.IsCircular)
                sb.Append($" Ø{form.DiameterMm}მმ");
            else
                sb.Append($" {form.WidthMm}×{form.HeightMm}მმ");

            if (length > 0)
                sb.Append($" ℓ={length:0.#}მ");

            sb.Append(" ()");

            if (!string.IsNullOrWhiteSpace(form.AdditionalInfo))
                sb.Append(" / ").Append(form.AdditionalInfo);

            return sb.ToString();
        }

        private ObjectId CreatePolyline(Point2d point1, Point2d point2)
        {
            var polyline = new Polyline { Layer = ExistingPipeLayer };
            polyline.AddVertexAt(0, point1, 0, 0, 0);
            polyline.AddVertexAt(1, point2, 0, 0, 0);
            var polylineId = DbUtilities.AddEntityToModelSpace(polyline);

            return polylineId;
        }

        private void AddPipeAnnotation(ObjectId polylineId, Status status, string annotation, int position)
        {
            var polyline = DbUtilities.GetObject<Polyline>(polylineId, OpenMode.ForRead);

            var firstVertex = polyline.GetPointAtDist(polyline.Length / 2);
            var lastVertex = new Point3d(firstVertex.X + position, firstVertex.Y + position, 0);

            var mleader = new MLeader();
            var leaderIndex = mleader.AddLeader();
            mleader.AddLeaderLine(leaderIndex);
            mleader.AddFirstVertex(leaderIndex, firstVertex);
            mleader.AddLastVertex(leaderIndex, lastVertex);
            mleader.Layer = status == Status.Design ? DesignPipeLayer : ExistingPipeLayer;
            mleader.ContentType = ContentType.MTextContent;

            var mldict = DbUtilities.GetObject<DBDictionary>(DbUtilities.Db.MLeaderStyleDictionaryId, OpenMode.ForRead);
            var mleaderStyleId = mldict.GetAt(MLeaderStyle);
            if (!mleaderStyleId.IsNull)
            {
                mleader.MLeaderStyle = mleaderStyleId;
            }

            var mtext = new MText
            {
                Contents = annotation,
                Location = lastVertex,
                TextHeight = 2.5,
                Width = 50
            };

            var textStyleTable =
                DbUtilities.GetObject<TextStyleTable>(DbUtilities.Db.TextStyleTableId, OpenMode.ForRead);
            var textStyleId = textStyleTable[TextStyle];
            if (!textStyleId.IsNull)
            {
                mtext.TextStyleId = textStyleId;
            }

            mleader.MText = mtext;

            DbUtilities.AddEntityToModelSpace(mleader);
        }

        private bool TryAddPipeLabel(ObjectId polylineId, Status status, string title, int offset)
        {
            var form = new PipeAnnotationForm(title);
            Application.ShowModalDialog(form);
            if (form.DialogResult != DialogResult.OK) return false;

            var polyline = DbUtilities.GetObject<Polyline>(polylineId, OpenMode.ForRead);

            var annotation = CreatePipeAnnotation(form, status, polyline.Length);

            AddPipeAnnotation(polylineId, status, annotation, offset);

            return true;
        }

        private void ChangePolylineLayerToDesign(ObjectId polylineId)
        {
            using (var transaction = DbUtilities.Db.TransactionManager.StartTransaction())
            {
                var polyline = DbUtilities.GetObject<Polyline>(polylineId, OpenMode.ForWrite, transaction);
                polyline.Layer = DesignPipeLayer;
                transaction.Commit();
            }
        }
    }
}