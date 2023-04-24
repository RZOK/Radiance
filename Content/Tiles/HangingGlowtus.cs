using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Core;
using Radiance.Utilities;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles
{
    public class HangingGlowtus : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileLighted[Type] = true;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Hanging Glowtus");
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
            float strength = 0.3f * Math.Clamp(Math.Abs(RadianceUtils.SineTiming(120)), 0.6f, 1f);
            r = 1f * strength;
            g = 0.9f * strength;
            b = 0.8f * strength;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 64, ModContent.ItemType<HangingGlowtusItem>());
        }
    }

    public class HangingGlowtusItem : BaseTileItem
    {
        public HangingGlowtusItem() : base("HangingGlowtusItem", "Hanging Glowtus", "", "HangingGlowtus", 1, Item.sellPrice(0, 0, 25, 0)) { }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.PotSuspended)
                .AddIngredient<GlowtusItem>()
                .Register();
        }
    }
}