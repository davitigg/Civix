using System;
using Civil3D.Labels.Interfaces;

namespace Civil3D.Labels.Common
{
    public class StationRangeLabel : ILabel, IStationPositionLabel
    {
        private double FromM { get; set; }
        private double ToM { get; set; }

        public StationRangeLabel(double fromM, double toM)
        {
            if (fromM >= toM)
                throw new ArgumentException("From station must be less than to To station");

            FromM = fromM;
            ToM = toM;
        }

        public string Text
        {
            get
            {
                var fromStationNumber = (int)(FromM / 100);
                var fromOffsetMeters = FromM % 100;
                var fromHasDecimalFraction = fromOffsetMeters % 1 > 0;

                var toStationNumber = (int)(ToM / 100);
                var toOffsetMeters = ToM % 100;
                var toHasDecimalFraction = toOffsetMeters % 1 > 0;

                var from = !fromHasDecimalFraction
                    ? $"{fromStationNumber}+{fromOffsetMeters:00}"
                    : $"{fromStationNumber}+{fromOffsetMeters:00.#}";

                var to = !toHasDecimalFraction
                    ? $"{toStationNumber}+{toOffsetMeters:00}"
                    : $"{toStationNumber}+{toOffsetMeters:00.#}";

                return $"({from} - {to})";
            }
        }
    }
}