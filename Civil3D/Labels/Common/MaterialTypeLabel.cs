using System;
using Civil3D.Enums;
using Civil3D.Labels.Interfaces;

namespace Civil3D.Labels.Common
{
    public class MaterialTypeLabel : ILabel
    {
        private Material? Type { get; set; }
        private Label CustomLabel { get; set; }

        public MaterialTypeLabel(Material type)
        {
            Type = type;
        }

        public MaterialTypeLabel(string customLabel)
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
                    case Material.Concrete: return "რკინაბეტონი";
                    case Material.Metal: return "ლითონი";
                    case Material.Asbestos: return "აზბესტი";
                    case Material.CastIron: return "ჩუგუნი";
                    case Material.Plastic: return "პლასტმასი";
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}