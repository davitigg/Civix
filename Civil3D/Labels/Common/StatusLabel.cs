using System;
using Civil3D.Enums;
using Civil3D.Labels.Interfaces;

namespace Civil3D.Labels.Common
{
    public class StatusLabel : ILabel
    {
        private Status? Type { get; set; }
        private Label CustomLabel { get; set; }

        public StatusLabel(Status type)
        {
            Type = type;
        }

        public StatusLabel(string customLabel)
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
                    case Status.Existing: return "არს.";
                    case Status.Design: return "საპ.";
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}