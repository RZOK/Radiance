using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;

namespace Radiance.Core.Encycloradia
{
    public class ResearchPlayer : ModPlayer
    {
        public EncycloradiaResearch activeResearch;
    }
    public class EncycloradiaResearch
    {
        public List<ResearchElement> elements;
        public EncycloradiaEntry entry;
        public EncycloradiaResearch(List<ResearchElement> elements, EncycloradiaEntry entry)
        {
            this.elements = elements;
            this.entry = entry;
        }
    }
    public abstract class ResearchElement
    {
        public Vector2 position;
        public ResearchElement(Vector2 position)
        {
            this.position = position;
        }
        public void Draw(SpriteBatch spriteBatch) { }
        public void Update() { }
    }
}
