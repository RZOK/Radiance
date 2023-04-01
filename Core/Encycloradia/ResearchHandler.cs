using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;

namespace Radiance.Core.Encycloradia
{
    public static class ResearchHandler
    {
        public class ResearchBoard
        {
            public readonly Rectangle Board = new Rectangle(0, 0, 628, 628);
            public Vector2 mouseCollisionPoint => Main.player[Main.myPlayer].GetModPlayer<ResearchPlayer>().mouseFrame;
            public List<ResearchElement> elements;
            public EncycloradiaEntry entry;

            public ResearchBoard(EncycloradiaResearch research)
            {
                elements = new List<ResearchElement>(research.elements);
                elements.ForEach(x => x.board = this);
                entry = research.entry;
            }

            public void Add(ResearchElement element) => elements.Add(element);

            public void RemoveRange(List<ResearchElement> elements) => elements.ForEach(x => Remove(x));

            public bool Remove(ResearchElement element) => elements.Remove(element);
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
            public float rotation;
            public bool canBePickedUp = false;
            public bool pickedUp = false;
            public bool solid = false;
            public int reflectCount = 0;
            public ResearchBoard board;
            public bool IsAlive => board != null;
            public int width;
            public int height;

            public ResearchElement(Vector2 position, float rotation, int width, int height, ResearchBoard board = null)
            {
                this.position = position;
                this.width = width;
                this.height = height;
                this.rotation = rotation;
                this.board = board;
            }

            public virtual void Draw(SpriteBatch spriteBatch, Vector2 drawPos)
            { }

            public virtual void Update()
            { }

            public virtual void OnCollide(Vector2 collidePosition, ResearchBeam beam)
            { }
        }

        #region Elements

        public class BeamSpawner : ResearchElement
        {
            public ResearchBeam beam;

            public BeamSpawner(Vector2 position, float rotation, ResearchBoard board = null) : base(position, rotation, 34, 26, board)
            {
            }

            public override void Update()
            {
                if (board != null)
                {
                    Vector2 start = position + Vector2.UnitX.RotatedBy(rotation) * width / 2;
                    if (beam == null)
                    {
                        beam = new ResearchBeam(start, Vector2.UnitX.RotatedBy(rotation), this, board);
                        board.Add(beam);
                    }
                    beam.startPos = start;
                    rotation = (board.mouseCollisionPoint - position).ToRotation();
                    beam.velocity = Vector2.UnitX.RotatedBy(rotation);
                }
            }

            public override void Draw(SpriteBatch spriteBatch, Vector2 drawPos)
            {
                Texture2D tex = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/ResearchElements/BeamSpawner").Value;
                Texture2D outlineTex = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/ResearchElements/BeamSpawnerOutline").Value;
                spriteBatch.Draw(tex, position + drawPos, null, Color.White, rotation, tex.Size() / 2, 1, SpriteEffects.None, 0);
                spriteBatch.Draw(outlineTex, position + drawPos, null, new Color(0, 255, 174), rotation, outlineTex.Size() / 2, 1, SpriteEffects.None, 0);

                Vector2 start = position + Vector2.UnitX.RotatedBy(rotation) * width / 2;
                Vector2 realStart = start + Main.screenPosition + drawPos;
                RadianceDrawing.DrawSoftGlow(realStart, CommonColors.RadianceColor1, 0.2f, RadianceDrawing.DrawingMode.Beam);
                RadianceDrawing.DrawSoftGlow(realStart, Color.White, 0.16f, RadianceDrawing.DrawingMode.Beam);
            }
        }

        public class StaticMirror : ResearchElement
        {
            public StaticMirror(Vector2 position, float rotation, ResearchBoard board = null) : base(position, rotation, 34, 8, board)
            {
                reflectCount = 3;
            }

            public override void Update()
            {
                reflectCount = 3;
            }

            public override void Draw(SpriteBatch spriteBatch, Vector2 drawPos)
            {
                Texture2D tex = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/ResearchElements/StaticMirror").Value;
                Texture2D outlineTex = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/ResearchElements/StaticMirrorOutline").Value;
                spriteBatch.Draw(tex, position + drawPos, null, Color.White, rotation, tex.Size() / 2, 1, SpriteEffects.None, 0);
                spriteBatch.Draw(outlineTex, position + drawPos, null, new Color(0, 255, 174), rotation, outlineTex.Size() / 2, 1, SpriteEffects.None, 0);
            }

            public override void OnCollide(Vector2 collidePosition, ResearchBeam beam)
            {
            }
        }

        #endregion Elements

        #region Beam

        public class ResearchBeam : ResearchElement
        {
            public Vector2 startPos;
            public Vector2 endPos;
            public Vector2 velocity;
            public ResearchElement spawningElement;

            public ResearchBeam(Vector2 startPos, Vector2 velocity, ResearchElement spawningElement, ResearchBoard board) : base(startPos, 0, 1, 1, board)
            {
                this.startPos = startPos;
                this.endPos = startPos;
                this.velocity = velocity;
                this.spawningElement = spawningElement;
            }

            internal PrimitiveTrail RayPrimDrawer;
            internal PrimitiveTrail RayPrimDrawer2;

            public override void Update()
            {
                if (spawningElement is not BeamSpawner)
                    board.Remove(this);
                if (IsAlive)
                    endPos = GetEndPoint();
            }

            public Vector2 GetEndPoint()
            {
                Vector2 endPoint = Vector2.One * -1;
                Vector2 nextPoint = startPos + velocity;
                float what = 0;
                while (endPoint == Vector2.One * -1)
                {
                    nextPoint += velocity;
                    if (!board.Board.Contains(nextPoint.ToPoint()))
                    {
                        endPoint = nextPoint - velocity;
                        break;
                    }
                    foreach (ResearchElement element in board.elements)
                    {
                        if (element.GetType() == typeof(ResearchBeam) || element == spawningElement)
                            continue;

                        if (Collision.CheckAABBvLineCollision(nextPoint, Vector2.One, element.position - Vector2.UnitX.RotatedBy(element.rotation) * element.width / 2, element.position + Vector2.UnitX.RotatedBy(element.rotation) * element.width / 2, element.height / 2, ref what))
                        {
                            if (element.reflectCount > 0)
                                element.reflectCount--;
                            endPoint = nextPoint;
                            if (element.reflectCount > 0)
                            {
                                Vector2 newVelocity = velocity;
                                if (Math.Abs(velocity.X) >= Math.Abs(velocity.Y))
                                    newVelocity.X = -newVelocity.X;
                                else
                                    newVelocity.Y = -newVelocity.Y;

                                ResearchBeam newBeam = new ResearchBeam(endPoint, newVelocity, element, board);
                                board.Add(newBeam);
                                newBeam.endPos = newBeam.GetEndPoint();
                            }
                            break;
                        }
                    }
                }
                return endPoint;
            }

            public override void Draw(SpriteBatch spriteBatch, Vector2 drawPos)
            {
                if (spawningElement is BeamSpawner)
                {
                    foreach (ResearchBeam element in board.elements.Where(x => x is ResearchBeam))
                    {
                        element.DrawRayStuff(spriteBatch, drawPos);
                    }
                }
            }

            public void DrawRayStuff(SpriteBatch spriteBatch, Vector2 drawPos)
            {
                Vector2 realStart = startPos + Main.screenPosition + drawPos;
                Vector2 realEnd = endPos + Main.screenPosition + drawPos;
                RadianceDrawing.DrawSoftGlow(realEnd, CommonColors.RadianceColor1, 0.2f, RadianceDrawing.DrawingMode.Beam);
                RadianceDrawing.DrawSoftGlow(realEnd, Color.White, 0.16f, RadianceDrawing.DrawingMode.Beam);
                Effect effect = Filters.Scene["UVMapStreak"].GetShader().Shader;

                RayPrimDrawer ??= new PrimitiveTrail(2, w => 10, c => CommonColors.RadianceColor1, new NoTip());
                RayPrimDrawer.SetPositionsSmart(new List<Vector2>() { realStart, realEnd }, realEnd);
                RayPrimDrawer.NextPosition = realEnd;
                effect.Parameters["time"].SetValue(0);
                effect.Parameters["fadePower"].SetValue(5);
                effect.Parameters["colorPower"].SetValue(1.6f);
                Main.graphics.GraphicsDevice.Textures[1] = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/BasicTrail").Value;
                RayPrimDrawer?.Render(effect, -Main.screenPosition);

                RayPrimDrawer2 ??= new PrimitiveTrail(2, w => 4, c => Color.White, new NoTip());
                RayPrimDrawer2.SetPositionsSmart(new List<Vector2>() { realStart, realEnd }, realEnd);
                RayPrimDrawer2.NextPosition = realEnd;
                effect.Parameters["time"].SetValue(0);
                effect.Parameters["fadePower"].SetValue(3);
                effect.Parameters["colorPower"].SetValue(1.6f);
                Main.graphics.GraphicsDevice.Textures[1] = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/BasicTrail").Value;
                RayPrimDrawer2?.Render(effect, -Main.screenPosition);
            }
        }

        #endregion Beam
    }
}