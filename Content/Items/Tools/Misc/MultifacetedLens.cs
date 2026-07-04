using MonoMod.Cil;
using Terraria.Map;
using Terraria.UI;

namespace Radiance.Content.Items.Tools.Misc
{
    public class MultifacetedLens : ModItem
    {
        public override void Load()
        {
            On_Main.DrawMiscMapIcons += DrawTileEntityMapElements;
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

            foreach (ImprovedTileEntity tileEntity in visibleTileEntities)
            {
                Vector2 drawPos = ((tileEntity.TileEntityWorldCenter() / 16 - mapTopLeft) * mapScale + mapX2Y2AndOff).Floor();
                ImprovedTileEntity.mapIconRenderer.DrawWithOutlines(tileEntity, drawPos, Color.White, 0, drawScale, SpriteEffects.None, out bool remove);
                if (remove)
                    tileEntitiesToRemove.Add(tileEntity);
            }
            visibleTileEntities.RemoveAll(tileEntitiesToRemove.Contains);
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