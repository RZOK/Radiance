using MonoMod.Cil;
using Terraria.GameInput;
using Terraria.Map;
using Terraria.UI;

namespace Radiance.Content.Items.Tools.Misc
{
    public class MultifacetedLens : ModItem
    {
        private static RenderTarget2D minimapTarget;
        public override void Load()
        {
            On_Main.DrawMiscMapIcons += DrawTileEntityMapElements;

            RenderTargetsManager.ResizeRenderTargetDelegateEvent += ResizeRenderTarget;
            RenderTargetsManager.DrawToRenderTargetsDelegateEvent += DrawToRenderTarget;
            ResizeRenderTarget();
        }

        private void DrawTileEntityMapElements(On_Main.orig_DrawMiscMapIcons orig, Main self, SpriteBatch spriteBatch, Vector2 mapTopLeft, Vector2 mapX2Y2AndOff, Rectangle? mapRect, float mapScale, float drawScale, ref string mouseTextString)
        {
            orig(self, spriteBatch, mapTopLeft, mapX2Y2AndOff, mapRect, mapScale, drawScale, ref mouseTextString);

            List<ImprovedTileEntity> tileEntitiesToRemove = new List<ImprovedTileEntity>();
            List<ImprovedTileEntity> visibleTileEntities = Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().visibleTileEntities;
            if (Main.mapStyle == 2 || Main.mapFullscreen)
            {
                foreach (ImprovedTileEntity tileEntity in visibleTileEntities)
                {
                    Vector2 drawPos = ((tileEntity.TileEntityWorldCenter() / 16 - mapTopLeft) * mapScale + mapX2Y2AndOff).Floor();
                    tileEntity.DrawMapUI(spriteBatch, drawPos, mapScale);
                }
            }
            else
            {
                Main.spriteBatch.Draw(minimapTarget, new Vector2(Main.miniMapX, Main.miniMapY), null, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
            }

            foreach (ImprovedTileEntity tileEntity in visibleTileEntities)
            {
                Vector2 drawPos = ((tileEntity.TileEntityWorldCenter() / 16 - mapTopLeft) * mapScale + mapX2Y2AndOff).Floor();
                ImprovedTileEntity.mapIconRenderer.DrawWithOutlines(tileEntity, drawPos, Color.White, 0, drawScale, SpriteEffects.None, out bool remove);
                if (remove)
                    tileEntitiesToRemove.Add(tileEntity);
            }
            visibleTileEntities.RemoveAll(tileEntitiesToRemove.Contains);
        }
        private void ResizeRenderTarget()
        {
            Main.QueueMainThreadAction(() =>
            {
                if (minimapTarget != null && !minimapTarget.IsDisposed)
                    minimapTarget.Dispose();

                minimapTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, 240, 240);
            });
        }

        private void DrawToRenderTarget()
        {
            GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;
            graphicsDevice.SetRenderTarget(minimapTarget);
            graphicsDevice.Clear(Color.Transparent);
            if (Main.dedServ || Main.gameMenu || Main.spriteBatch is null || minimapTarget is null || graphicsDevice is null)
            {
                graphicsDevice.SetRenderTargets(null);
                return;
            }
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity);

            List<ImprovedTileEntity> visibleTileEntities = Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().visibleTileEntities;
            if (Main.mapStyle == 1)
            {
                foreach (ImprovedTileEntity tileEntity in visibleTileEntities)
                {
                    float scale = Main.mapMinimapScale;
                    if (scale <= 0.2f)
                        scale = 0.2f;

                    Vector2 topLeft = new Vector2((Main.screenPosition.X + PlayerInput.RealScreenWidth / 2f) / 16f, (Main.screenPosition.Y + PlayerInput.RealScreenHeight / 2f) / 16f);
                    float width = Main.miniMapWidth / scale;
                    float height = Main.miniMapHeight / scale;
                    float centerX = (float)((int)topLeft.X - width / 2f);
                    float centerY = (float)((int)topLeft.Y - height / 2f);

                    float x2Off = -(topLeft.X - (float)(int)((Main.screenPosition.X + (float)(PlayerInput.RealScreenWidth / 2)) / 16f)) * scale;
                    float y2Off = -(topLeft.Y - (float)(int)((Main.screenPosition.Y + (float)(PlayerInput.RealScreenHeight / 2)) / 16f)) * scale;
                    Vector2 mapX2Y2AndOff = new Vector2(x2Off, y2Off);
                    Vector2 drawPos = ((tileEntity.TileEntityWorldCenter() / 16f - new Vector2(centerX, centerY)) * scale + mapX2Y2AndOff).Floor();
                    tileEntity.DrawMapUI(Main.spriteBatch, drawPos, scale);
                }
            }

            Main.spriteBatch.End();
            graphicsDevice.SetRenderTargets(null);
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Multifaceted Lens");
            Tooltip.SetDefault("Right click an Apparatus to always be viewing it\nAllows the visibility of Rays to be toggled while in your inventory");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 1;
            Item.rare = ItemRarityID.Green;
        }
    }

    public class MultifacetedLensBuilderToggle : BuilderToggle
    {
        public override bool Active() => Main.LocalPlayer.HasItem(ModContent.ItemType<MultifacetedLens>());

        public override string Texture => $"{nameof(Radiance)}/Content/Items/Tools/Misc/MultifacetedLens_BuilderToggle";
        public override string HoverTexture => $"{nameof(Radiance)}/Content/Items/Tools/Misc/MultifacetedLens_BuilderToggle_Outline";

        public override string DisplayValue()
        {
            string text = "Ray Visibility: ";
            string[] textMessages = new[] { "Off", "On" };

            return text + textMessages[CurrentState];
        }

        public override bool Draw(SpriteBatch spriteBatch, ref BuilderToggleDrawParams drawParams)
        {
            switch (CurrentState)
            {
                case 0:
                    drawParams.Color = Color.Gray;
                    break;

                case 1:
                    drawParams.Color = Color.White;
                    break;
            }
            return true;
        }
    }
    public class MultifacetedLensHoverElement : HoverUIElement
    {
        public MultifacetedLensHoverElement() : base("MultifacetedLens") { }
        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/Items/Tools/Misc/MultifacetedLens_HoverUI").Value;

            spriteBatch.Draw(tex, realDrawPosition, null, Color.White * timerModifier * 0.6f, 0, tex.Size() / 2, 1f, SpriteEffects.None, 0);
        }
    }
}