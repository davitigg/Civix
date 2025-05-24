using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Civil3D.Autodesk
{
    internal static class EdUtilities
    {
        internal static readonly Editor Ed = Application.DocumentManager.MdiActiveDocument.Editor;

        internal static void PanTo(Point3d targetWcs)
        {
            var view = Ed.GetCurrentView();

            var wcs2dcs = Matrix3d.PlaneToWorld(view.ViewDirection);
            wcs2dcs = Matrix3d.Displacement(view.Target - Point3d.Origin) * wcs2dcs;
            wcs2dcs = Matrix3d.Rotation(-view.ViewTwist, view.ViewDirection, view.Target) * wcs2dcs;
            var dcs2wcs = wcs2dcs.Inverse();

            var newCenter = targetWcs.TransformBy(dcs2wcs);

            view.CenterPoint = new Point2d(newCenter.X, newCenter.Y);

            Ed.SetCurrentView(view);
        }

        internal static bool PromptYesOrNo(string message, bool defaultYes = false)
        {
            var opts = new PromptKeywordOptions(message)
            {
                AllowNone = false
            };
            opts.Keywords.Add("Yes");
            opts.Keywords.Add("No");
            opts.Keywords.Default = defaultYes ? "Yes" : "No";

            var result = Ed.GetKeywords(opts);

            if (result.Status != PromptStatus.OK) throw new Exception(ErrorStatus.UserBreak);

            return result.StringResult == "Yes";
        }
    }
}