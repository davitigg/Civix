using System;
using Civil3D.Labels.Interfaces;

namespace Civil3D.Labels.Common
{
    public class Label : ILabel
    {
        public Label(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Custom label must not be null or empty.", nameof(text));

            Text = text;
        }

        public string Text { get; }
    }
}