using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using Radiance.Utilities;
using Terraria.ModLoader;

namespace Radiance.Core.Encycloradia
{
    public static class ResearchHandler
    {
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
            public float rotation;
            public bool canBePickedUp = false;
            public bool pickedUp = false;
            public bool solid = false;
            public bool reflective = false;
            public ResearchElement(Vector2 position, float rotation)
            {
                this.position = position;
                this.rotation = rotation;
            }
            public virtual void Draw(SpriteBatch spriteBatch) { }
            public virtual void Update() { }
            public sealed void 
        }
        public class ResearchBeam : ResearchElement
        {
            public Vector2 startPos;
            public Vector2 endPos;
            public ResearchBeam(Vector2 startPos, Vector2 endPos) : base(startPos, 0) 
            {
                this.endPos = endPos;
            }
            internal PrimitiveTrail RayPrimDrawer;
            internal PrimitiveTrail RayPrimDrawer2;
            public void DrawRay()
            {
                for (int i = 0; i < j; i++)
                {
                    RadianceDrawing.DrawSoftGlow(i == 0 ? endPos : startPos, CommonColors.RadianceColor1, 0.2f, RadianceDrawing.DrawingMode.Beam);
                    RadianceDrawing.DrawSoftGlow(i == 0 ? endPos : startPos, Color.White, 0.16f, RadianceDrawing.DrawingMode.Beam);
                }
                Effect effect = Filters.Scene["UVMapStreak"].GetShader().Shader;

                RayPrimDrawer = RayPrimDrawer ?? new PrimitiveTrail(2, w => 10, c => CommonColors.RadianceColor1, new NoTip());
                RayPrimDrawer.SetPositionsSmart(new List<Vector2>() { startPos, endPos }, endPos, RadianceUtils.RigidPointRetreivalFunction);
                RayPrimDrawer.NextPosition = endPos;
                effect.Parameters["time"].SetValue(0);
                effect.Parameters["fadePower"].SetValue(5);
                effect.Parameters["colorPower"].SetValue(1.6f);
                Main.graphics.GraphicsDevice.Textures[1] = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/BasicTrail").Value;
                RayPrimDrawer?.Render(effect, -Main.screenPosition);

                RayPrimDrawer2 = RayPrimDrawer2 ?? new PrimitiveTrail(2, w => 4, c => Color.White, new NoTip());
                RayPrimDrawer2.SetPositionsSmart(new List<Vector2>() { startPos, endPos }, endPos, RadianceUtils.RigidPointRetreivalFunction);
                RayPrimDrawer2.NextPosition = endPos;
                effect.Parameters["time"].SetValue(0);
                effect.Parameters["fadePower"].SetValue(3);
                effect.Parameters["colorPower"].SetValue(1.6f);
                Main.graphics.GraphicsDevice.Textures[1] = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/BasicTrail").Value;
                RayPrimDrawer2?.Render(effect, -Main.screenPosition);
            }
            public override void Draw(SpriteBatch spriteBatch)
            {

            }
        }
    }
}
