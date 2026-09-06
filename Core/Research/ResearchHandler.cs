using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Core.Research.Elements;
using Radiance.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Localization;
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

        public abstract class ResearchComponent : ICloneable
        {
            public Vector2 position;
            public Vector2 lastSafePosition;
            public float rotation;
            public float lastSafeRotation;
            public int width;
            public int height;

            public bool playerPlaced = false;
            public bool movable = false;    

            public bool pickedUp = false;
            public bool inDrawer = false;

            public static readonly string LOCALIZATION_KEY = $"Mods.{Radiance.Instance.Name}.Research.Components.";
            public ResearchComponent(Vector2 position, float rotation, int width, int height)
            {
                this.position = position;
                this.width = width;
                this.height = height;
                this.rotation = rotation;
            }

            public void Draw(SpriteBatch spriteBatch, Color drawColor)
            {
                if (Hitbox().Contains(Main.MouseScreen.ToPoint()))
                {
                    if (Main.mouseLeft && !Main.mouseLeftRelease && ResearchUI.Instance.researchTable.heldComponent is null)
                    {
                        lastSafePosition = position;
                        lastSafeRotation = rotation;
                        ResearchUI.Instance.researchTable.heldComponent = this;
                    }
                    else
                    {
                        bool displayTooltip = false;
                        if (inDrawer && ResearchUI.Instance.researchTable.heldComponent is null)
                            displayTooltip = true;
                        if (displayTooltip)
                        {
                            string[] iconString =
                            {
                                $"[c/FFC042:{GetNameLocalizedText().Value}]",
                                $"{GetTooltipLocalizedText().Value}",
                                $"Component"
                            };
                            Main.LocalPlayer.SetFakeHoverText(iconString, new Color(180, 180, 180), tex: ModContent.Request<Texture2D>($"{nameof(Radiance)}/Core/Research/Assets/ResearchTooltipBackground").Value);
                        }
                    }
                }
                DrawExtra(spriteBatch, drawColor);
            }
            public abstract void DrawExtra(SpriteBatch spriteBatch, Color drawColor);

            public abstract void Update();

            public abstract void OnBeamCollide(Vector2 collidePosition, ResearchBeam beam);

            public Rectangle Hitbox() => new Rectangle((int)position.X - width / 2, (int)position.Y - height / 2, width, height);
            public LocalizedText GetNameLocalizedText() => Language.GetOrRegister($"{LOCALIZATION_KEY}{GetType().Name}.Name");
            public LocalizedText GetTooltipLocalizedText() => Language.GetOrRegister($"{LOCALIZATION_KEY}{GetType().Name}.ToolTip");
            public bool CanPlace(ResearchBoard board)
            {
                if(ResearchUI.Instance.researchTable.IsInTable(this))
                {
                    foreach (ResearchComponent component in board.components)
                    {
                        if (component == this)
                            continue;

                        int longestSize = width;
                        if (height > width)
                            longestSize = height;

                        int otherLongestSize = component.width;
                        if (component.height > component.width)
                            otherLongestSize = component.height;

                        if (position.Distance(component.position) < (longestSize + otherLongestSize) / 2)
                        {
                            if (IntersectsWithComponent(component))
                                return false;
                        }
                    }
                    return true;
                }
                return false;
            }
            public bool IntersectsWithComponent(ResearchComponent component)
            {
                Vector2[] edges = new Vector2[4];
                Vector2[] targetEdges = new Vector2[4];
                edges[0] = position + new Vector2(width / 2, height / 2).RotatedBy(rotation);
                edges[1] = position + new Vector2(width / -2, height / 2).RotatedBy(rotation);
                edges[2] = position + new Vector2(width / -2, height / -2).RotatedBy(rotation);
                edges[3] = position + new Vector2(width / 2, height / -2).RotatedBy(rotation);

                targetEdges[0] = component.position + new Vector2(component.width / 2, component.height / 2).RotatedBy(component.rotation);
                targetEdges[1] = component.position + new Vector2(component.width / -2, component.height / 2).RotatedBy(component.rotation);
                targetEdges[2] = component.position + new Vector2(component.width / -2, component.height / -2).RotatedBy(component.rotation);
                targetEdges[3] = component.position + new Vector2(component.width / 2, component.height / -2).RotatedBy(component.rotation);

                for (int i = 0; i < 4; i++)
                {
                    Vector2 edge1 = edges[i];
                    Vector2 edge2 = edges[(i + 1) % 4];
                    for (int j = 0; j < 4; j++)
                    {
                        Vector2 targetEdge1 = targetEdges[j];
                        Vector2 targetEdge2 = targetEdges[(j + 1) % 4];
                        if (Collision.CheckLinevLine(edge1, edge2, targetEdge1, targetEdge2).Length > 0)
                            return true;
                    }
                }
                return false;
            }
            public object Clone() => MemberwiseClone();
        }
    }
}