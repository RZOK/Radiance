using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Items.StabilizationCrystals;
using Radiance.Core.Systems;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles
{
    #region Stabilizer Column

    public class StabilizerColumn : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.CoordinateHeights = new int[3] { 16, 16, 18 };
            HitSound = SoundID.Dig;
            DustType = -1;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Stabilization Column");
            AddMapEntry(new Color(255, 197, 97), name);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<StabilizerColumnTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }
        public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (TryGetTileEntityAs(i, j, out StabilizerColumnTileEntity entity))
            {
                Tile tile = Main.tile[i, j];
                if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
                {
                    Vector2 basePosition = entity.TileEntityWorldCenter() - Main.screenPosition + tileDrawingZero;
                    Color color = Lighting.GetColor(i, j);
                    if (entity.inventory != null && !entity.GetSlot(0).IsAir && entity.CrystalPlaced != null)
                    {
                        Texture2D crystalTexture = ModContent.Request<Texture2D>(entity.CrystalPlaced.PlacedTexture).Value;
                        spriteBatch.Draw(crystalTexture, basePosition + new Vector2(0, -6 - crystalTexture.Height / 2), null, color * 0.2f, 0, crystalTexture.Size() / 2, 1.2f + Main.rand.NextFloat(0, 0.3f), SpriteEffects.None, 0);
                        spriteBatch.Draw(crystalTexture, basePosition + new Vector2(0, -6), null, color * 5, 0, new Vector2(crystalTexture.Width / 2, crystalTexture.Height), 1, SpriteEffects.None, 0);

                    }
                }
            }
        }
        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            if (drawData.tileFrameX == 0 && drawData.tileFrameY == 0 && TryGetTileEntityAs(i, j, out StabilizerColumnTileEntity entity) && entity.CrystalPlaced != null)
            {
                Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
            }
        }

        public override void MouseOver(int i, int j)
        {
            if (TryGetTileEntityAs(i, j, out StabilizerColumnTileEntity entity))
            {
                if(entity.inventory != null)
                    Main.LocalPlayer.SetCursorItem(entity.GetSlot(0).IsAir ? ModContent.ItemType<StabilizationCrystal>() : entity.GetSlot(0).type);

                entity.AddHoverUI();
            }
        }
        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (TryGetTileEntityAs(i, j, out StabilizerColumnTileEntity entity) && entity.CrystalPlaced != null)
                Lighting.AddLight(entity.TileEntityWorldCenter() - Vector2.UnitY * 8, entity.CrystalPlaced.CrystalColor.ToVector3() * 0.3f);
        }
        public override bool RightClick(int i, int j)
        {
            if (TryGetTileEntityAs(i, j, out StabilizerColumnTileEntity entity) && !Main.LocalPlayer.ItemAnimationActive)
            {
                Item selItem = GetPlayerHeldItem();
                if (selItem.ModItem as IStabilizationCrystal != null || entity.CrystalPlaced != null)
                {
                    int dust = selItem.ModItem as IStabilizationCrystal == null ? entity.CrystalPlaced.DustID : (selItem.ModItem as IStabilizationCrystal).DustID;
                    bool success = false;
                    entity.DropItem(0, new Vector2(i * 16, j * 16));
                    if (selItem.ModItem as IStabilizationCrystal != null)
                        entity.SafeInsertItemIntoSlot(0, ref selItem, out success, 1);

                    TileEntitySystem.ResetStability();

                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/CrystalInsert"), new Vector2(i * 16 + entity.Width * 8, j * 16 + -entity.Height * 8));
                    SpawnCrystalDust(MultitileOriginWorldPosition(i, j) + new Vector2(2, -2), dust);
                    return true;
                }
            }
            return false;
        }

        public static void SpawnCrystalDust(Vector2 pos, int dust)
        {
            for (int i = 0; i < 8; i++)
            {
                int d = Dust.NewDust(pos, 8, 18, dust);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 0.3f;
                Main.dust[d].scale = 1.7f;
                Main.dust[d].fadeIn = 1.1f;
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (TryGetTileEntityAs(i, j, out StabilizerColumnTileEntity entity))
            {
                if (entity.CrystalPlaced != null)
                {
                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/CrystalInsert"), new Vector2(i * 16 + entity.Width * 8, j * 16 + -entity.Height * 8));
                    SpawnCrystalDust(MultitileOriginWorldPosition(i, j) - (Vector2.UnitY * 2) + (Vector2.UnitX * 10), (entity.GetSlot(0).ModItem as IStabilizationCrystal).DustID);
                    entity.DropAllItems(new Vector2(i * 16, j * 16));
                }
                ModContent.GetInstance<StabilizerColumnTileEntity>().Kill(i, j);
            }
        }
    }

    public class StabilizerColumnTileEntity : StabilizerTileEntity, IInventory
    {
        public StabilizerColumnTileEntity() : base(ModContent.TileType<StabilizerColumn>()) { }

        public IStabilizationCrystal CrystalPlaced => inventory != null ? this.GetSlot(0).ModItem as IStabilizationCrystal : null;
        public override int StabilityLevel => CrystalPlaced != null ? CrystalPlaced.StabilizationLevel : 0;
        public override int StabilizerRange => CrystalPlaced != null ? CrystalPlaced.StabilizationRange : 0;
        public override StabilizeType StabilizationType => CrystalPlaced != null ? CrystalPlaced.StabilizationType : StabilizeType.Basic;

        public Item[] inventory { get; set; }

        public byte[] inputtableSlots => new byte[] { 0 };
        public byte[] outputtableSlots => Array.Empty<byte>();

        protected override HoverUIData ManageHoverUI()
        {
            List<HoverUIElement> data = new List<HoverUIElement>();
            if (CrystalPlaced != null)
                data.Add(new SquareUIElement("AoESquare", StabilizerRange * 16, CrystalPlaced.CrystalColor));

            return new HoverUIData(this, Position.ToVector2() * 16 + new Vector2(8, 8), data.ToArray());
        }
        public override void OrderedUpdate()
        {
            this.ConstructInventory(1);
        }

        public override void SaveData(TagCompound tag)
        {
            this.SaveInventory(tag);
        }

        public override void LoadData(TagCompound tag)
        {
            this.LoadInventory(tag, 1);
        }
    }

    #endregion Stabilizer Column

    public class StabilizerColumnItem : BaseTileItem
    {
        public StabilizerColumnItem() : base("StabilizerColumnItem", "Stabilization Column", "Stabilizes nearby Apparatuses", "StabilizerColumn", 1, Item.sellPrice(0, 0, 3, 0), ItemRarityID.Blue) { }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup(RecipeGroupID.Wood, 8)
                .AddRecipeGroup("SilverGroup", 2)
                .AddIngredient<ShimmeringGlass>()
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}