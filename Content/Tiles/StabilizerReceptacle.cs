using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.Materials;
using Radiance.Content.Items.StabilizationCrystals;
using Radiance.Core.Systems;
using Terraria.Localization;
using Terraria.ObjectData;
using static Radiance.Content.Items.BaseItems.BaseStabilizationCrystal;

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
                    Vector2 basePosition = entity.TileEntityWorldCenter() - Main.screenPosition + TileDrawingZero;
                    Color color = Lighting.GetColor(i, j);
                    if (entity.inventory is not null && !entity.GetSlot(0).IsAir && entity.CrystalPlaced is not null)
                    {
                        Texture2D crystalTexture = ModContent.Request<Texture2D>(entity.CrystalPlaced.placedTexture).Value;
                        spriteBatch.Draw(crystalTexture, basePosition + Vector2.UnitY * 4, null, color * 0.2f, 0, new Vector2(crystalTexture.Width / 2, crystalTexture.Height), 1.1f + Main.rand.NextFloat(0, 0.05f), SpriteEffects.None, 0);
                        spriteBatch.Draw(crystalTexture, basePosition + Vector2.UnitY * 4, null, color * 5, 0, new Vector2(crystalTexture.Width / 2, crystalTexture.Height), 1, SpriteEffects.None, 0);
                    }
                }
            }
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (TryGetTileEntityAs(i, j, out StabilizerReceptacleTileEntity entity))
            {
                player.noThrow = 2;
                player.cursorItemIconEnabled = true;
                if (entity.inventory is not null)
                    player.cursorItemIconID = entity.GetSlot(0).IsAir ? ModContent.ItemType<StabilizationCrystal>() : entity.GetSlot(0).type;

                entity.AddHoverUI();
            }
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (TryGetTileEntityAs(i, j, out StabilizerReceptacleTileEntity entity) && entity.CrystalPlaced != null)
                Lighting.AddLight(entity.TileEntityWorldCenter() - Vector2.UnitY * 8, entity.CrystalPlaced.crystalColor.ToVector3() * 0.3f);
        }

        public override bool RightClick(int i, int j)
        {
            if (TryGetTileEntityAs(i, j, out StabilizerReceptacleTileEntity entity) && !Main.LocalPlayer.ItemAnimationActive)
            {
                Item item = GetPlayerHeldItem();
                if (item.ModItem is BaseStabilizationCrystal || entity.CrystalPlaced != null)
                {
                    int dust = item.ModItem is BaseStabilizationCrystal ? (item.ModItem as BaseStabilizationCrystal).dustID : entity.CrystalPlaced.dustID;
                    TileEntitySystem.shouldUpdateStability = true;
                    bool success = false;
                    entity.DropItem(0, new Vector2(i * 16, j * 16), out _);
                    if (!item.IsAir && !item.favorited)
                        entity.SafeInsertSlot(0, item, out success, true, true);

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
            if (!fail && TryGetTileEntityAs(i, j, out StabilizerReceptacleTileEntity entity))
            {
                if (entity.CrystalPlaced is not null)
                {
                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/CrystalInsert"), new Vector2(i * 16 + entity.Width * 8, j * 16 + -entity.Height * 8));
                    SpawnCrystalDust(MultitileOriginWorldPosition(i, j) - (Vector2.UnitY * 2) + (Vector2.UnitX * 10), (entity.GetSlot(0).ModItem as BaseStabilizationCrystal).dustID);
                    entity.DropAllItems(new Vector2(i * 16, j * 16));
                }
                Point origin = GetTileOrigin(i, j);
                ModContent.GetInstance<StabilizerReceptacleTileEntity>().Kill(origin.X, origin.Y);
            }
        }
    }

    public class StabilizerReceptacleTileEntity : StabilizerTileEntity, IInventory, ISpecificStackSlotInventory
    {
        public StabilizerReceptacleTileEntity() : base(ModContent.TileType<StabilizerReceptacle>())
        {
            inventorySize = 1;
            this.ConstructInventory();
        }

        public BaseStabilizationCrystal CrystalPlaced => inventory != null ? this.GetSlot(0).ModItem as BaseStabilizationCrystal : null;
        public override int StabilityLevel => CrystalPlaced != null ? CrystalPlaced.stabilizationLevel / 2 : 0;
        public override int StabilizerRange => CrystalPlaced != null ? CrystalPlaced.stabilizationRange / 2 : 0;
        public override StabilizeType StabilizationType => CrystalPlaced != null ? CrystalPlaced.stabilizationType : StabilizeType.Basic;

        public Item[] inventory { get; set; }
        public int inventorySize { get; set; }
        public byte[] inputtableSlots => new byte[] { 0 };
        public byte[] outputtableSlots => Array.Empty<byte>();

        public Dictionary<int, int> allowedStackPerSlot => new Dictionary<int, int>()
        {
            [0] = 1
        };

        public bool CanInsertSlot(Item item, byte slot, bool overrideValidInputs, bool ignoreItemImprint)
        {
            if ((!ignoreItemImprint && !itemImprintData.ImprintAcceptsItem(item)) || (!overrideValidInputs && !inputtableSlots.Contains(slot)))
                return false;

            return item.ModItem is not null && item.ModItem is BaseStabilizationCrystal;
        }

        protected override HoverUIData GetHoverUI()
        {
            List<HoverUIElement> data = new List<HoverUIElement>();
            if (CrystalPlaced != null)
                data.Add(new RectangleUIElement("AoESquare", StabilizerRange * 16, StabilizerRange * 16, CrystalPlaced.crystalColor));

            return new HoverUIData(this, Position.ToVector2() * 16 + new Vector2(8, 8), data.ToArray());
        }

        public override void OrderedUpdate()
        {
        }

        public override void SaveExtraData(TagCompound tag)
        {
            this.SaveInventory(tag);
        }

        public override void LoadExtraData(TagCompound tag)
        {
            this.LoadInventory(tag);
        }
    }

    #endregion Stabilizer Receptacle

    public class StabilizerReceptacleItem : BaseTileItem
    {
        public StabilizerReceptacleItem() : base("StabilizerReceptacleItem", "Stabilization Receptacle", "Stabilizes nearby Apparatuses with a decreased range and potency", "StabilizerReceptacle", 1, Item.sellPrice(0, 0, 5, 0), ItemRarityID.Blue)
        {
        }

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