using Civil3D.Labels.Interfaces;

namespace Civil3D.Labels.Common
{
    public class WidthHeightLabel : ILabel, ISectionLabel
    {
        private int WidthMm { get; set; }
        private int HeightMm { get; set; }

        public WidthHeightLabel(int widthMm, int heightMm)
        {
            WidthMm = widthMm;
            HeightMm = heightMm;
        }

        public string Text => $"{WidthMm}×{HeightMm}მმ";
    }
}