using Civil3D.Labels.Interfaces;

namespace Civil3D.Labels.Components
{
    public class BenchmarkLabel : ILabel
    {
        private uint Id { get; set; }

        public BenchmarkLabel(uint id)
        {
            Id = id;
        }

        public string Text => $"RP-{Id}";
    }
}