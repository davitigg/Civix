using System;
using Civil3D.Enums;
using Civil3D.Labels.Interfaces;

namespace Civil3D.Labels.Common
{
    public class StructureTypeLabel : ILabel
    {
        private Structure? Type { get; set; }
        private Label CustomLabel { get; set; }

        public StructureTypeLabel(Structure type)
        {
            Type = type;
        }

        public StructureTypeLabel(string customLabel)
        {
            CustomLabel = new Label(customLabel);
        }

        public string Text
        {
            get
            {
                if (!Type.HasValue) return CustomLabel.Text;

                switch (Type)
                {
                    case Structure.Pipe: return "მილი";
                    case Structure.Ditch: return "კიუვეტი";
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}