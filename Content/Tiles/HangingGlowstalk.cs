using Radiance.Content.Items.BaseItems;
using Terraria.Enums;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles
{
    public class HangingGlowstalk : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileLighted[Type] = true;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Hanging Glowstalk");
            AddMapEntry(new Color(241, 226, 172), name);

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);

            TileObjectData.addTile(Type);

            DustType = -1;
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            if (Main.tile[i, j].TileFrameX == 0 && Main.tile[i, j].TileFrameY == 0)
            {
                if (Main.rand.NextBool(20))
                {
                    int d = Dust.NewDust(new Vector2(i * 16 + 8, j * 16 + 6), 14, 14, DustID.TreasureSparkle);
                    Main.dust[d].velocity *= 0f;
                    Main.dust[d].noGravity = true;
                }
            }
        }

        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
        {
            offsetY = -2;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            float strength = 0.3f * Math.Clamp(Math.Abs(SineTiming(120)), 0.6f, 1f);
            r = 1f * strength;
            g = 0.9f * strength;
            b = 0.8f * strength;
        }
    }

    public class HangingGlowstalkItem : BaseTileItem
    {
        public HangingGlowstalkItem() : base("HangingGlowstalkItem", "Hanging Glowstalk", "", "HangingGlowstalk", 1, Item.sellPrice(0, 0, 25, 0))
        {
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.PotSuspended)
                .AddIngredient<GlowstalkItem>()
                .Register();
        }
    }
}