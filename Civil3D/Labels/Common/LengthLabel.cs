using Civil3D.Labels.Interfaces;

namespace Civil3D.Labels.Common
{
    public class LengthLabel : ILabel
    {
        private double LengthM { get; set; }

        public LengthLabel(double lengthM)
        {
            LengthM = lengthM;
        }

        public string Text => $"L{LengthM:0.##}მ";
    }
}