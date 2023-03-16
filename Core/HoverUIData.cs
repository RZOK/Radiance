using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Radiance.Core
{
    public class HoverUIData
    {
        public Vector2 position;
        public int timer;
        public List<HoverUIElement> elements;
        public HoverUIData(Vector2 position, params HoverUIElement[] elements)
        {
            this.position = position;
            this.elements = elements.ToList();
        }
    }
    public abstract class HoverUIElement
    {
        public Vector2 elementPosition;
        public Vector2 targetPosition;
        public abstract void Draw(SpriteBatch spriteBatch);
        public void Update()
        {

        }
    }
    public class TextUIElement : HoverUIElement
    {
        public string text;
        public Color color;
        public TextUIElement(string text, Color color, Vector2 targetPosition)
        {
            this.text = text;
            this.color = color;
            this.targetPosition = targetPosition;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}
