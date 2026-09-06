using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Items.RadianceCells;
using Radiance.Core.Systems;
using Radiance.Utilities;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using Radiance.Core.Interfaces;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.TransmutationRecipeSystem;
using static Radiance.Core.Research.ResearchHandler;
using Radiance.Content.Items.Armor;
using System.Security.Cryptography.Xml;
using System.CodeDom;
using Radiance.Core.Research.Elements;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using rail;
using MonoMod.Core.Platforms;
using Steamworks;
using Terraria.Modules;

namespace Radiance.Core.Research
{
    internal class ResearchUI : SmartUIState
    {
        public static ResearchUI Instance { get; set; }
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Builder Accessories Bar"));
        public override bool Visible => researchTable.tableVisible;
        public ResearchTable researchTable = new();

        public ResearchUI()
        {
            Instance = this;
        }

        public override void OnInitialize()
        {
            Append(researchTable);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            researchTable.Left.Set(-ResearchTable.mainTexture.Width / 2f, 0.5f);
            researchTable.Top.Set(-ResearchTable.mainTexture.Height / 2f, 0.5f);
            researchTable.Width.Set(ResearchTable.mainTexture.Width, 0);
            researchTable.Height.Set(ResearchTable.mainTexture.Height, 0);
            Recalculate();
        }
    }
    internal class ResearchTable : UIElement
    {
        public static Texture2D mainTexture => ModContent.Request<Texture2D>($"{nameof(Radiance)}/Core/Research/Assets/ResearchTable").Value;
        public static Texture2D drawerTexture => ModContent.Request<Texture2D>($"{nameof(Radiance)}/Core/Research/Assets/ResearchSlot_Drawer").Value;
        public static Texture2D keySlotTexture => ModContent.Request<Texture2D>($"{nameof(Radiance)}/Core/Research/Assets/ResearchSlot_Key").Value;
        private ResearchPlayer ResearchPlayer => Main.LocalPlayer.GetModPlayer<ResearchPlayer>();
        public ResearchUI UIParent => Parent as ResearchUI;
        public bool tableVisible = false;
        public static Vector2 padding = new Vector2(14, 14);
        private const int DRAWER_PADDING = 10;
        private const int INITIAL_DRAWER_VERTICAL_PADDING = 6;
        private const int DRAWER_MAX_HEIGHT = 240;
        private List<Rectangle> visibleDrawers = new List<Rectangle>();
        public static RenderTarget2D componentTarget;
        public ResearchComponent heldComponent;
        public override void Update(GameTime gameTime)
        {
            if ((ResearchPlayer.tablePosition.HasValue && Main.LocalPlayer.position.Distance(ResearchPlayer.tablePosition.Value) > (Main.LocalPlayer.blockRange + Player.tileRangeX + 4) * 16f))
                CloseTable();

            UpdateHeldComponent();
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            spriteBatch.GetSpritebatchDetails(out SpriteSortMode spriteSortMode, out BlendState blendState, out SamplerState samplerState, out DepthStencilState depthStencilState, out RasterizerState rasterizerState, out Effect effect, out Matrix matrix);
            spriteBatch.End();
            spriteBatch.Begin(spriteSortMode, blendState, SamplerState.PointClamp, depthStencilState, rasterizerState, effect, matrix);


            Rectangle dimensions = GetDimensions().ToRectangle();
            Vector2 drawPos = dimensions.TopLeft();

            DrawTable(spriteBatch, drawPos);
            List<ResearchComponent> researchComponents = new List<ResearchComponent>() { new MirrorComponent(Vector2.Zero, 0), new BeamSpawnerComponent(Vector2.Zero, 0), new BeamSpawnerComponent(Vector2.Zero, 0), new BeamSpawnerComponent(Vector2.Zero, 0) };
            for (int i = 0; i < visibleDrawers.Count; i++)
            {
                bool right = i > 1;
                Rectangle drawer = visibleDrawers[i];
                DrawDrawer(spriteBatch, drawer, right);
            }
            DrawKeySlot(spriteBatch, drawPos);

            if (ResearchPlayer.activeBoard is not null)
                DrawComponents(spriteBatch);

            spriteBatch.End();
            spriteBatch.Begin(spriteSortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, matrix);
        }

        public void UpdateHeldComponent()
        {
            if (heldComponent is not null)
            {
                if (Main.mouseLeftRelease)
                {
                    if (heldComponent.CanPlace(ResearchPlayer.activeBoard))
                    {
                        if (heldComponent.inDrawer)
                        {
                            ResearchComponent clonedComponent = (ResearchComponent)heldComponent.Clone();
                            clonedComponent.inDrawer = false;
                            ResearchPlayer.activeBoard.components.Add(clonedComponent);
                        }
                        heldComponent = null;
                    }
                    else
                    {
                        if ((!heldComponent.inDrawer && !heldComponent.playerPlaced) || IsInTable(heldComponent))
                        {
                            heldComponent.position = heldComponent.lastSafePosition;
                            heldComponent.rotation = heldComponent.lastSafeRotation;
                        }
                        else
                            ResearchPlayer.activeBoard.components.Remove(heldComponent);

                        heldComponent = null;
                    }
                }
                else
                {
                    if (!Main.keyState.PressingShift())
                        heldComponent.position = Vector2.Lerp(heldComponent.position, Main.MouseScreen, 0.35f);
                    else
                        heldComponent.rotation = Lerp(heldComponent.rotation, heldComponent.position.AngleTo(Main.MouseScreen), 0.5f);
                }
            }
        }
        public bool IsInTable(ResearchComponent component)
        {
            Rectangle dimensions = GetDimensions().ToRectangle();
            Vector2 drawPos = dimensions.TopLeft();
            dimensions.Inflate(-(int)padding.X, -(int)padding.Y);

            return
                dimensions.Contains((component.position + new Vector2(component.width / 2, component.height / 2).RotatedBy(component.rotation)).ToPoint()) &&
                dimensions.Contains((component.position + new Vector2(component.width / -2, component.height / 2).RotatedBy(component.rotation)).ToPoint()) &&
                dimensions.Contains((component.position + new Vector2(component.width / -2, component.height / -2).RotatedBy(component.rotation)).ToPoint()) &&
                dimensions.Contains((component.position + new Vector2(component.width / 2, component.height / -2).RotatedBy(component.rotation)).ToPoint());
        }
        internal void CloseTable()
        {
            SoundEngine.PlaySound(SoundID.MenuClose);
            ResearchPlayer.tablePosition = null;
            tableVisible = false;
        }
        protected void DrawTable(SpriteBatch spriteBatch, Vector2 position)
        {
            Rectangle dimensions = GetDimensions().ToRectangle();
            spriteBatch.Draw(mainTexture, position + mainTexture.Size() / 2, null, Color.White, 0, mainTexture.Size() / 2, 1, SpriteEffects.None, 0);
            if (dimensions.Contains(Main.MouseScreen.ToPoint()))
                Main.LocalPlayer.mouseInterface = true;
        }
        internal void DrawKeySlot(SpriteBatch spriteBatch, Vector2 position)
        {
            Vector2 drawPos = position + new Vector2(4f, 56f);
            spriteBatch.Draw(keySlotTexture, drawPos, null, Color.White, 0, new Vector2(keySlotTexture.Width, keySlotTexture.Height / 2f), 1, SpriteEffects.None, 0);
        }
        internal void DrawDrawer(SpriteBatch spriteBatch, Rectangle rect, bool right)
        {
            Vector2 position = new Vector2(rect.X, rect.Y);

            Rectangle topCorner = new Rectangle(0, 0, 34, 14);
            Rectangle bottomCorner = new Rectangle(0, 36, 34, 14);
            Rectangle middleSection = new Rectangle(0, 20, 34, 10);
            Rectangle topEdge = new Rectangle(40, 0, 2, 14);
            Rectangle bottomEdge = new Rectangle(40, 36, 2, 14);

            Rectangle topTallStretch = new Rectangle(0, 16, 34, 2);
            Rectangle bottomTallStretch = new Rectangle(0, 32, 34, 2);
            Rectangle bottomLongStretch = new Rectangle(36, 36, 2, 14);
            Rectangle topLongStretch = new Rectangle(36, 0, 2, 14);
            Rectangle middleFiller = new Rectangle(36, 16, 2, 2);
            Rectangle edgeFiller = new Rectangle(40, 16, 2, 2);

            float horizontalStretch = rect.Width - topCorner.Width - topEdge.Width;
            float verticalStretch = rect.Height - topCorner.Height - bottomCorner.Height - middleSection.Height;
            Vector2 horizontalStretchScale = new Vector2(horizontalStretch / 2f, 1f);
            Vector2 verticalStretchScale = new Vector2(1f, verticalStretch / 4f);
            Vector2 fillScale = new Vector2(horizontalStretch, verticalStretch + middleSection.Height) / 2f;

            SpriteEffects flipped = SpriteEffects.None;
            if (right)
            {
                flipped = SpriteEffects.FlipHorizontally;

                spriteBatch.Draw(drawerTexture, position - new Vector2(-rect.Width + topCorner.Width, -topCorner.Height - verticalStretch / 2f), middleSection, Color.White, 0, Vector2.Zero, Vector2.One, flipped, 0);

                spriteBatch.Draw(drawerTexture, position - new Vector2(-rect.Width + topCorner.Width, 0), topCorner, Color.White, 0, Vector2.Zero, Vector2.One, flipped, 0);
                spriteBatch.Draw(drawerTexture, position - new Vector2(-rect.Width + topCorner.Width, -topCorner.Height), topTallStretch, Color.White, 0, Vector2.Zero, verticalStretchScale, flipped, 0);

                spriteBatch.Draw(drawerTexture, position - new Vector2(-edgeFiller.Width - horizontalStretch, -topCorner.Height - middleSection.Height - verticalStretch / 2f), bottomTallStretch, Color.White, 0, Vector2.Zero, verticalStretchScale, flipped, 0);
                spriteBatch.Draw(drawerTexture, position - new Vector2(-edgeFiller.Width - horizontalStretch, -rect.Height + bottomCorner.Height), bottomCorner, Color.White, 0, Vector2.Zero, Vector2.One, flipped, 0);

                spriteBatch.Draw(drawerTexture, position - new Vector2(-edgeFiller.Width, 0f), topLongStretch, Color.White, 0, Vector2.Zero, horizontalStretchScale, flipped, 0);
                spriteBatch.Draw(drawerTexture, position - new Vector2(-edgeFiller.Width, -topCorner.Height), middleFiller, Color.White, 0, Vector2.Zero, fillScale, flipped, 0);
                spriteBatch.Draw(drawerTexture, position - new Vector2(-edgeFiller.Width, -rect.Height + bottomCorner.Height), bottomLongStretch, Color.White, 0, Vector2.Zero, horizontalStretchScale, flipped, 0);

                spriteBatch.Draw(drawerTexture, position, topEdge, Color.White, 0, Vector2.Zero, Vector2.One, flipped, 0);
                spriteBatch.Draw(drawerTexture, position - new Vector2(0f, -topCorner.Height), edgeFiller, Color.White, 0, Vector2.Zero, new Vector2(1f, fillScale.Y), flipped, 0);
                spriteBatch.Draw(drawerTexture, position - new Vector2(0f, -rect.Height + bottomCorner.Height), bottomEdge, Color.White, 0, Vector2.Zero, Vector2.One, flipped, 0);

                Rectangle dimensions = new Rectangle((int)position.X, (int)position.Y, rect.Width, rect.Height);
                if (dimensions.Contains(Main.MouseScreen.ToPoint()))
                    Main.LocalPlayer.mouseInterface = true;
            }
            else
            {
                spriteBatch.Draw(drawerTexture, position - new Vector2(rect.Width, -topCorner.Height - verticalStretch / 2f), middleSection, Color.White, 0, Vector2.Zero, Vector2.One, flipped, 0);

                spriteBatch.Draw(drawerTexture, position - new Vector2(rect.Width, 0), topCorner, Color.White, 0, Vector2.Zero, Vector2.One, flipped, 0);
                spriteBatch.Draw(drawerTexture, position - new Vector2(rect.Width, -topCorner.Height), topTallStretch, Color.White, 0, Vector2.Zero, verticalStretchScale, flipped, 0);

                spriteBatch.Draw(drawerTexture, position - new Vector2(rect.Width, -topCorner.Height - middleSection.Height - verticalStretch / 2f), bottomTallStretch, Color.White, 0, Vector2.Zero, verticalStretchScale, flipped, 0);
                spriteBatch.Draw(drawerTexture, position - new Vector2(rect.Width, -rect.Height + bottomCorner.Height), bottomCorner, Color.White, 0, Vector2.Zero, Vector2.One, flipped, 0);

                spriteBatch.Draw(drawerTexture, position - new Vector2(rect.Width - topCorner.Width, 0), topLongStretch, Color.White, 0, Vector2.Zero, horizontalStretchScale, flipped, 0);
                spriteBatch.Draw(drawerTexture, position - new Vector2(rect.Width - topCorner.Width, -topCorner.Height), middleFiller, Color.White, 0, Vector2.Zero, fillScale, flipped, 0);
                spriteBatch.Draw(drawerTexture, position - new Vector2(rect.Width - bottomCorner.Width, -rect.Height + bottomCorner.Height), bottomLongStretch, Color.White, 0, Vector2.Zero, horizontalStretchScale, flipped, 0);

                spriteBatch.Draw(drawerTexture, position - new Vector2(rect.Width - topCorner.Width - horizontalStretch, -topCorner.Height), edgeFiller, Color.White, 0, Vector2.Zero, new Vector2(1f, fillScale.Y), flipped, 0);
                spriteBatch.Draw(drawerTexture, position - new Vector2(rect.Width - topCorner.Width - horizontalStretch, 0), topEdge, Color.White, 0, Vector2.Zero, Vector2.One, flipped, 0);
                spriteBatch.Draw(drawerTexture, position - new Vector2(rect.Width - bottomCorner.Width - horizontalStretch, -rect.Height + bottomCorner.Height), bottomEdge, Color.White, 0, Vector2.Zero, Vector2.One, flipped, 0);

                Rectangle dimensions = new Rectangle((int)position.X - rect.Width, (int)position.Y, rect.Width, rect.Height);
                if (dimensions.Contains(Main.MouseScreen.ToPoint()))
                    Main.LocalPlayer.mouseInterface = true;
            }
        }
        public static void DrawComponentsToTarget()
        {
            ResearchUI.Instance.researchTable.visibleDrawers.Clear();
            GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;
            graphicsDevice.SetRenderTarget(componentTarget);
            graphicsDevice.Clear(Color.Transparent);
            if (Main.dedServ || Main.gameMenu || Main.spriteBatch is null || componentTarget is null || graphicsDevice is null)
            {
                graphicsDevice.SetRenderTargets(null);
                return;
            }
            ResearchPlayer ResearchPlayer = Main.LocalPlayer.GetModPlayer<ResearchPlayer>();
            if (ResearchPlayer.activeBoard is null)
                return;

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity);

            ResearchBoard activeBoard = ResearchPlayer.activeBoard;
            List<ResearchComponent> activeComponents = activeBoard.components;
            if (activeComponents is not null)
            {
                foreach (ResearchComponent component in activeComponents)
                {
                    component.Draw(Main.spriteBatch, Color.White);
                }
            }

            // Create drawer objects

            List<List<ResearchComponent>> componentsPerDrawer = new List<List<ResearchComponent>>();
            List<ResearchComponent> componentsToDrawer = new List<ResearchComponent>(activeBoard.availableComponents);
            Rectangle baseDrawerSize = new Rectangle(0, 0, DRAWER_PADDING * 2, DRAWER_PADDING); 

            Rectangle currentDrawerSize = baseDrawerSize;
            List<ResearchComponent> currentDrawer = new List<ResearchComponent>();
            while (componentsToDrawer.Count > 0) 
            {
                ResearchComponent component = componentsToDrawer.Pop();
                int componentHeightWithPadding = component.height + DRAWER_PADDING;
                if (currentDrawerSize.Height + componentHeightWithPadding > DRAWER_MAX_HEIGHT)
                {
                    currentDrawerSize = baseDrawerSize;
                    componentsPerDrawer.Add(new List<ResearchComponent>(currentDrawer));
                    currentDrawer.Clear();
                }
                currentDrawer.Add(component);
                currentDrawerSize.Height += componentHeightWithPadding;
            }
            componentsPerDrawer.Add(currentDrawer);

            // Draw components fitted into drawers

            Rectangle dimensions = ResearchUI.Instance.researchTable.GetDimensions().ToRectangle();
            Vector2 drawPos = dimensions.TopLeft();
            for (int i = 0; i < componentsPerDrawer.Count; i++)
            {
                List<ResearchComponent> components = componentsPerDrawer[i];

                int width = components.Max(x => x.width + DRAWER_PADDING * 2);
                int height = components.Sum(x => x.height) + DRAWER_PADDING * (components.Count + 1) + INITIAL_DRAWER_VERTICAL_PADDING;

                Vector2 position = drawPos + new Vector2(4f, mainTexture.Height / 2f + (mainTexture.Height / 5f * -(1 - i % 2 * 2)) - height / 2f);
                if(i > 1)
                    position.X += mainTexture.Width - 8f;

                int componentY = DRAWER_PADDING + INITIAL_DRAWER_VERTICAL_PADDING / 2;
                foreach (ResearchComponent component in components)
                {
                    componentY += component.height / 2;
                    if(ResearchUI.Instance.researchTable.heldComponent != component)
                        component.position = new Vector2(position.X + (width - 8f)/ 2f * -(1 - i / 2 * 2), position.Y + componentY);

                    component.Draw(Main.spriteBatch, Color.White);
                    componentY += component.height / 2 + DRAWER_PADDING;
                }
                ResearchUI.Instance.researchTable.visibleDrawers.Add(new Rectangle((int)position.X, (int)position.Y, width, height));
            }

            if (ResearchUI.Instance.researchTable.heldComponent is not null)
            {
                Color color = Color.White;
                if (!ResearchUI.Instance.researchTable.heldComponent.CanPlace(ResearchPlayer.activeBoard))
                    color = Color.Red;
                ResearchUI.Instance.researchTable.heldComponent.Draw(Main.spriteBatch, color);
            }
            Main.spriteBatch.End();
            graphicsDevice.SetRenderTargets(null);
        }
        internal void DrawComponents(SpriteBatch spriteBatch)
        {
            spriteBatch.GetSpritebatchDetails(out SpriteSortMode spriteSortMode, out BlendState blendState, out SamplerState samplerState, out DepthStencilState depthStencilState, out RasterizerState rasterizerState, out Effect effect, out Matrix matrix);
            
            Effect circleEffect = Terraria.Graphics.Effects.Filters.Scene["SolidColor"].GetShader().Shader;
            circleEffect.Parameters["sampleTexture"].SetValue(componentTarget);
            circleEffect.Parameters["color"].SetValue(Color.Black.ToVector4() * 0.5f);

            spriteBatch.End();
            spriteBatch.Begin(spriteSortMode, blendState, samplerState, depthStencilState, rasterizerState, circleEffect, matrix);
            Main.spriteBatch.Draw(componentTarget, Vector2.UnitY * 2f, null, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);

            spriteBatch.End();
            spriteBatch.Begin(spriteSortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, matrix);
            Main.spriteBatch.Draw(componentTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);

        }
    }
}