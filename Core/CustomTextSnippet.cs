using Microsoft.Xna.Framework;
using System;

namespace Radiance.Core
{
    public class CustomTextSnippet
    {
        public string text = string.Empty;
        public Color color = Color.White;
        public Color backgroundColor = Color.Black;
        public CustomTextSnippet(string text, Color color, Color backgroundColor)
        {
            this.text = text;
            this.color = color;
            this.backgroundColor = backgroundColor;
        }
    }
}
