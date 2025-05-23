using Civil3D.Labels.Interfaces;

namespace Civil3D.Labels.Common
{
    public class DiameterLabel : ILabel, ISectionLabel
    {
        private int DiameterMm { get; set; }

        public DiameterLabel(int diameterMm)
        {
            DiameterMm = diameterMm;
        }

        public string Text => $"⌀{DiameterMm}მმ";
    }
}