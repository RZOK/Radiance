using Radiance.Content.Items;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.StabilizationCrystals;
using Radiance.Core.Systems;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles
{
    #region Stabilizer Receptacle

    public class StabilizerReceptacle : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.StyleHorizontal = true;
            HitSound = SoundID.Tink;
            DustType = -1;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Stabilization Receptacle");
            AddMapEntry(new Color(255, 197, 97), name);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<StabilizerReceptacleTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (TryGetTileEntityAs(i, j, out StabilizerReceptacleTileEntity entity))
            {
                Tile tile = Main.tile[i, j];
                if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
                {
                    Vector2 basePosition = entity.TileEntityWorldCenter() - Main.screenPosition + tileDrawingZero;
                    Color color = Lighting.GetColor(i, j);
                    if (entity.inventory != null && !entity.GetSlot(0).IsAir && entity.CrystalPlaced != null)
                    {
                        Texture2D crystalTexture = ModContent.Request<Texture2D>(entity.CrystalPlaced.PlacedTexture).Value;

                        Main.spriteBatch.Draw(crystalTexture, basePosition + new Vector2(0, 4 - crystalTexture.Height / 2), null, color * 0.2f, 0, crystalTexture.Size() / 2, 1.2f + Main.rand.NextFloat(0, 0.3f), SpriteEffects.None, 0);
                        Main.spriteBatch.Draw(crystalTexture, basePosition + new Vector2(0, 4), null, color * 5, 0, new Vector2(crystalTexture.Width / 2, crystalTexture.Height), 1, SpriteEffects.None, 0);
                    }
                }
            }
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            RadianceInterfacePlayer mp = player.GetModPlayer<RadianceInterfacePlayer>();
            if (TryGetTileEntityAs(i, j, out StabilizerReceptacleTileEntity entity))
            {
                player.noThrow = 2;
                player.cursorItemIconEnabled = true;
                if (entity.inventory != null)
                    player.cursorItemIconID = entity.GetSlot(0).IsAir ? ModContent.ItemType<StabilizationCrystal>() : entity.GetSlot(0).type;

                entity.AddHoverUI();
            }
        }
        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (TryGetTileEntityAs(i, j, out StabilizerReceptacleTileEntity entity) && entity.CrystalPlaced != null)
                Lighting.AddLight(entity.TileEntityWorldCenter() - Vector2.UnitY * 8, entity.CrystalPlaced.CrystalColor.ToVector3() * 0.3f);
        }
        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (TryGetTileEntityAs(i, j, out StabilizerReceptacleTileEntity entity) && !player.ItemAnimationActive)
            {
                Item selItem = GetPlayerHeldItem();
                if (selItem.ModItem as IStabilizationCrystal != null || entity.CrystalPlaced != null)
                {
                    int dust = selItem.ModItem as IStabilizationCrystal == null ? entity.CrystalPlaced.DustID : (selItem.ModItem as IStabilizationCrystal).DustID;
                    bool success = false;
                    entity.DropItem(0, new Vector2(i * 16, j * 16));
                    if (selItem.ModItem as IStabilizationCrystal != null)
                        entity.SafeInsertItemIntoSlot(0, ref selItem, out success, 1);

                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/CrystalInsert"), new Vector2(i * 16 + entity.Width * 8, j * 16 + -entity.Height * 8));
                    SpawnCrystalDust(MultitileOriginWorldPosition(i, j) + new Vector2(2, -4), dust);
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
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (TryGetTileEntityAs(i, j, out StabilizerReceptacleTileEntity entity))
            {
                if (entity.CrystalPlaced != null)
                {
                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/CrystalInsert"), new Vector2(i * 16 + entity.Width * 8, j * 16 + -entity.Height * 8));
                    SpawnCrystalDust(MultitileOriginWorldPosition(i, j) - (Vector2.UnitY * 2) + (Vector2.UnitX * 10), (entity.GetSlot(0).ModItem as IStabilizationCrystal).DustID);
                    entity.DropAllItems(new Vector2(i * 16, j * 16));
                }
                Point origin = GetTileOrigin(i, j);
                ModContent.GetInstance<StabilizerReceptacleTileEntity>().Kill(origin.X, origin.Y);
            }
        }
    }

    public class StabilizerReceptacleTileEntity : StabilizerTileEntity, IInventory
    {
        public StabilizerReceptacleTileEntity() : base(ModContent.TileType<StabilizerReceptacle>()) { }

        public IStabilizationCrystal CrystalPlaced => inventory != null ? this.GetSlot(0).ModItem as IStabilizationCrystal : null;
        public override int StabilityLevel => CrystalPlaced != null ? CrystalPlaced.StabilizationLevel / 2 : 0;
        public override int StabilizerRange => CrystalPlaced != null ? CrystalPlaced.StabilizationRange / 2 : 0;
        public override StabilizeType StabilizationType => CrystalPlaced != null ? CrystalPlaced.StabilizationType : StabilizeType.Basic;

        public Item[] inventory { get; set; }

        public byte[] inputtableSlots => new byte[] { 0 };
        public byte[] outputtableSlots => Array.Empty<byte>();
        protected override HoverUIData ManageHoverUI()
        {
            List<HoverUIElement> data = new List<HoverUIElement>();
            if (CrystalPlaced != null)
                data.Add(new SquareUIElement("AoESquare", StabilizerRange * 16 - 6, CrystalPlaced.CrystalColor));

            return new HoverUIData(this, Position.ToVector2() * 16 + new Vector2(8, 8), data.ToArray());
        }
        public override void OrderedUpdate()
        {
            this.ConstructInventory(1);
            if (StabilizerRange > 0 && StabilityLevel > 0)
            {
                var entitiesInRange = TileEntitySystem.TileEntitySearchSoft(this, StabilizerRange);

                var entitiesToStabilize = entitiesInRange.Where(x => x.usesStability && x.idealStability > 0 &&
                Math.Abs(Position.X - (x.Position.X + x.Width - 1)) <= StabilizerRange &&
                Math.Abs(Position.Y - (x.Position.Y + x.Height - 1)) <= StabilizerRange);

                var stabilizersInRange = entitiesInRange.Where(x => x is StabilizerTileEntity sb && sb.StabilityLevel > 0);
                foreach (ImprovedTileEntity e in entitiesToStabilize)
                {
                    float realStabilityLevel = StabilityLevel;
                    foreach (StabilizerTileEntity ste in stabilizersInRange)
                    {
                        if (ste.Position == Position)
                            continue;

                        float distance = Vector2.Distance(Position.ToVector2(), ste.Position.ToVector2()) / Vector2.Distance(Position.ToVector2(), Position.ToVector2() + Vector2.One * StabilizerRange);
                        realStabilityLevel *= MathHelper.Lerp(0.67f, 1, distance);
                    }
                    if (realStabilityLevel > 0)
                        e.stability += realStabilityLevel / entitiesToStabilize.Count();
                }
            }
        }

        public override void SaveData(TagCompound tag)
        {
            base.SaveData(tag);
            this.SaveInventory(tag);
        }

        public override void LoadData(TagCompound tag)
        {
            base.LoadData(tag);
            this.LoadInventory(tag, 1);
        }
    }

    #endregion Stabilizer Receptacle

    public class StabilizerReceptacleItem : BaseTileItem
    {
        public StabilizerReceptacleItem() : base("StabilizerReceptacleItem", "Stabilization Receptacle", "Stabilizes nearby Apparatuses with a decreased range and potency", "StabilizerReceptacle", 1, Item.sellPrice(0, 0, 5, 0), ItemRarityID.Blue) { }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup("SilverGroup", 3)
                .AddIngredient<PetrifiedCrystal>(2)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}