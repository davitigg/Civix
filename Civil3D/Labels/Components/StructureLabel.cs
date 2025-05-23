using System;
using System.Collections.Generic;
using Civil3D.Labels.Common;
using Civil3D.Labels.Interfaces;
using Civil3D.Utilities;

namespace Civil3D.Labels.Components
{
    public class StructureLabel : ILabel
    {
        private StatusLabel Status { get; set; }
        private MaterialTypeLabel MaterialType { get; set; }
        private StructureTypeLabel StructureType { get; set; }
        private ISectionLabel Section { get; set; }
        private LengthLabel Length { get; set; }
        private IStationPositionLabel Station { get; set; }
        private Label AdditionalInfo { get; set; }

        public StructureLabel(StatusLabel status, StructureTypeLabel structureType,
            MaterialTypeLabel materialType = null, ISectionLabel section = null, LengthLabel length = null,
            IStationPositionLabel station = null, Label additionalInfo = null)
        {
            Status = status ?? throw new ArgumentNullException(nameof(status));
            MaterialType = materialType;
            StructureType = structureType ?? throw new ArgumentNullException(nameof(structureType));
            Section = section;
            Length = length;
            Station = station;
            AdditionalInfo = additionalInfo;
        }

        public string Text
        {
            get
            {
                var parts = new List<string>();

                parts.Add(Status.Text);
                if (MaterialType != null) parts.Add(MaterialType.Text.ToGenitive());
                parts.Add(StructureType.Text);
                if (Section != null) parts.Add(Section.Text);
                if (Length != null) parts.Add(Length.Text);
                if (Station != null) parts.Add(Station.Text);
                if (AdditionalInfo != null) parts.Add($"/ {AdditionalInfo.Text}");

                var text = string.Join(" ", parts);

                return text;
            }
        }
    }
}