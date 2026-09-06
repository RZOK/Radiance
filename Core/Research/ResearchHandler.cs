using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Core.Research.Elements;
using Radiance.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;

namespace Radiance.Core.Research
{
    public static class ResearchHandler
    {
        public class ResearchBoard
        {
            public readonly Rectangle Board = new Rectangle(0, 0, 626, 626);
            public List<ResearchComponent> components;
            public List<ResearchComponent> availableComponents;

            public ResearchBoard()
            {

            }
        }

        public abstract class ResearchComponent
        {
            public Vector2 position;
            public float rotation;
            public int width;
            public int height;

            public bool playerPlaced = false;
            public bool movable = false;    

            public bool pickedUp = false;

            public ResearchComponent(Vector2 position, float rotation, int width, int height)
            {
                this.position = position;
                this.width = width;
                this.height = height;
                this.rotation = rotation;
            }

            public abstract void Draw(SpriteBatch spriteBatch);

            public abstract void Update();

            public abstract void OnBeamCollide(Vector2 collidePosition, ResearchBeam beam);
        }
    }
}