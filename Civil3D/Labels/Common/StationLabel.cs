using Civil3D.Labels.Interfaces;

namespace Civil3D.Labels.Common
{
    public class StationLabel : ILabel, IStationPositionLabel
    {
        private double DistanceM { get; set; }

        public StationLabel(double distanceM)
        {
            DistanceM = distanceM;
        }

        public StationLabel(int distanceM)
        {
            DistanceM = distanceM;
        }

        public string Text
        {
            get
            {
                var stationNumber = (int)(DistanceM / 100);
                var offsetMeters = DistanceM % 100;
                var hasDecimalFraction = offsetMeters % 1 > 0;

                return !hasDecimalFraction
                    ? $"({stationNumber}+{offsetMeters:00})"
                    : $"({stationNumber}+{offsetMeters:00.#})";
            }
        }
    }
}