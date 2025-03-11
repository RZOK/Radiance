using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Items.RadianceCells;
using Radiance.Core.Loaders;
using Radiance.Core.Systems;
using Terraria.Localization;
using Terraria.ObjectData;
using static Radiance.Content.Items.BaseItems.BaseContainer;

namespace Radiance.Content.Tiles.Transmutator
{
    #region Projector

    public class Projector : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.CoordinateHeights = new int[3] { 16, 16, 18 };
            HitSound = SoundID.Dig;
            DustType = -1;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Radiance Projector");
            AddMapEntry(new Color(255, 197, 97), name);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<ProjectorTileEntity>().Hook_AfterPlacement, -1, 0, false);
            TileObjectData.addTile(Type);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (TryGetTileEntityAs(i, j, out ProjectorTileEntity entity))
            {
                Tile tile = Main.tile[i, j];
                if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
                {
                    Vector2 basePosition = entity.TileEntityWorldCenter() - Main.screenPosition + TileDrawingZero;
                    Texture2D baseTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/ProjectorBase").Value;
                    Texture2D glowTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/ProjectorGlow").Value;
                    Texture2D braceTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/ProjectorBraces").Value;
                    Color color = Lighting.GetColor(i, j);
                    if (entity.inventory != null && !entity.GetSlot(0).IsAir && entity.LensPlaced is not null)
                    {
                        Texture2D glassTexture = ModContent.Request<Texture2D>(RadianceSets.ProjectorLensTexture[entity.LensPlaced.type]).Value;
                        Main.spriteBatch.Draw(glassTexture, basePosition + new Vector2(0, -20), null, color, 0, glassTexture.Size() / 2, 1, SpriteEffects.None, 0);
                    }
                    if (entity.inventory != null && !entity.GetSlot(1).IsAir && entity.ContainerPlaced != null && entity.ContainerPlaced.HasMiniTexture)
                    {
                        Texture2D texture = ModContent.Request<Texture2D>(entity.ContainerPlaced.extraTextures[BaseContainer_TextureType.Mini]).Value;
                        Main.spriteBatch.Draw(texture, basePosition + new Vector2(0, 5), null, color, 0, new Vector2(texture.Width / 2, texture.Height / 2 - (texture.Height / 2 % 2) + 1), 1, SpriteEffects.None, 0);
                    }
                    Main.spriteBatch.Draw(baseTexture, basePosition, null, color, 0, baseTexture.Size() / 2, 1, SpriteEffects.None, 0);
                    if (entity.transmutator != null)
                    {
                        if (entity.transmutator.craftingTimer > 0)
                        {
                            Color glowColor = Color.White * EaseOutCirc(entity.transmutator.craftingTimer / 120);
                            Main.spriteBatch.Draw(glowTexture, basePosition, null, glowColor, 0, baseTexture.Size() / 2, 1, SpriteEffects.None, 0);
                            for (int h = 0; h < 2; h++)
                                RadianceDrawing.DrawBeam(basePosition + Main.screenPosition + Vector2.UnitY * 6, basePosition + Main.screenPosition - Vector2.UnitY * 20, h == 1 ? (Color.White * 0.3f * (entity.transmutator.craftingTimer / 120)) : (CommonColors.RadianceColor1 * 0.3f * (entity.transmutator.craftingTimer / 120)), h == 1 ? 8 : 12);
                        }
                        if (entity.transmutator.projectorBeamTimer > 0)
                        {
                            for (int h = 0; h < 2; h++)
                            {
                                Color beamColor = h == 1 ? (Color.White * (entity.transmutator.projectorBeamTimer / 60)) : (CommonColors.RadianceColor1 * (entity.transmutator.projectorBeamTimer / 60));
                                RadianceDrawing.DrawBeam(basePosition + Main.screenPosition - Vector2.UnitY * 20, basePosition + Main.screenPosition - Vector2.UnitY * 48, beamColor, h == 1 ? 8 : 12);
                            }
                        }
                    }
                    if (entity.HasTransmutator)
                        Main.spriteBatch.Draw(braceTexture, basePosition - Vector2.UnitY * 12, null, color, 0, braceTexture.Size() / 2, 1, SpriteEffects.None, 0);
                }
            }
            return false;
        }

        public override void MouseOver(int i, int j)
        {
            if (TryGetTileEntityAs(i, j, out ProjectorTileEntity entity))
            {
                int cursorItem = entity.GetSlot(1).IsAir ? ModContent.ItemType<StandardRadianceCell>() : entity.GetSlot(1).type;
                if (Main.tile[i, j].TileFrameY == 0)
                    cursorItem = entity.GetSlot(0).IsAir ? ModContent.ItemType<ShimmeringGlass>() : entity.GetSlot(0).type;

                Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().canSeeLensItems = true;
                Main.LocalPlayer.SetCursorItem(cursorItem);
                entity.AddHoverUI();
            }
        }

        public override bool RightClick(int i, int j)
        {
            if (TryGetTileEntityAs(i, j, out ProjectorTileEntity entity) && !Main.LocalPlayer.ItemAnimationActive)
            {
                Vector2 position = entity.TileEntityWorldCenter();
                Item selItem = GetPlayerHeldItem();
                if (Main.tile[i, j].TileFrameY == 0)
                {
                    if (RadianceSets.ProjectorLensID[selItem.type] != (int)ProjectorLensID.None || entity.LensPlaced is not null)
                    {
                        int effectItem = RadianceSets.ProjectorLensID[selItem.type] != -1 ? selItem.type : entity.LensPlaced.type;
                        int dust = RadianceSets.ProjectorLensDust[effectItem];

                        entity.DropItem(0, position, out _);
                        if (RadianceSets.ProjectorLensID[selItem.type] != -1)
                            entity.SafeInsertItemIntoSlot(0, selItem, out _, true, true);

                        SoundStyle playedSound = RadianceSets.ProjectorLensSound[effectItem];
                        if (playedSound == default)
                            playedSound = Radiance.projectorLensTink;

                        SoundEngine.PlaySound(playedSound, position);
                        SpawnLensDust(MultitileOriginWorldPosition(i, j) + new Vector2(10, -10), dust);
                        return true;
                    }
                }
                else
                {
                    if (selItem.ModItem is BaseContainer || entity.ContainerPlaced is not null)
                    {
                        entity.DropItem(1, position, out _);
                        if (selItem.ModItem is BaseContainer)
                            entity.SafeInsertItemIntoSlot(1, selItem, out _, true, true);

                        SoundEngine.PlaySound(SoundID.Tink, position);
                        return true;
                    }
                }
            }
            return false;
        }

        public static void SpawnLensDust(Vector2 pos, int dust)
        {
            for (int i = 0; i < 20; i++)
            {
                int d = Dust.NewDust(pos, 8, 28, dust);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 0.1f;
                Main.dust[d].scale = 1.7f;
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (TryGetTileEntityAs(i, j, out ProjectorTileEntity entity))
            {
                if (entity.LensPlaced is not null)
                {
                    Vector2 position = entity.TileEntityWorldCenter();
                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/LensPop"), entity.TileEntityWorldCenter());
                    SpawnLensDust(MultitileOriginWorldPosition(i, j) + new Vector2(10, -10), RadianceSets.ProjectorLensDust[entity.LensPlaced.type]);
                    entity.DropAllItems(position);
                }
                ModContent.GetInstance<ProjectorTileEntity>().Kill(i, j);
            }
        }
    }

    public class ProjectorTileEntity : RadianceUtilizingTileEntity, IInventory, IInterfaceableRadianceCell, ISpecificStackSlotInventory, IPostSetupContentLoadable
    {
        public ProjectorTileEntity() : base(ModContent.TileType<Projector>(), 0, new() { 5, 6 }, new(), usesItemImprints: true)
        {
            inventorySize = 2;
            this.ConstructInventory();
        }

        public TransmutatorTileEntity transmutator;
        public bool HasTransmutator => transmutator != null;
        public Item LensPlaced 
        {
            get
            {
                Item item = this.GetSlot(0);
                if (!item.IsAir && RadianceSets.ProjectorLensID[item.type] != (int)ProjectorLensID.None)
                    return item;

                return null;
            }
        }
        public BaseContainer ContainerPlaced => this.GetSlot(1).ModItem as BaseContainer;
        public Item[] inventory { get; set; }
        public int inventorySize { get; set; }
        public byte[] inputtableSlots => new byte[] { 0, 1 };

        public byte[] outputtableSlots => new byte[] { 1 };

        public Dictionary<int, int> allowedStackPerSlot => new Dictionary<int, int>()
        {
            [0] = 1,
            [1] = 1
        };

        public bool TryInsertItemIntoSlot(Item item, byte slot, bool overrideValidInputs, bool ignoreItemImprint)
        {
            if ((!ignoreItemImprint && !itemImprintData.ImprintAcceptsItem(item)) || (!overrideValidInputs && !inputtableSlots.Contains(slot)))
                return false;

            if (slot == 0)
                return RadianceSets.ProjectorLensID[item.type] != 0;

            return item.ModItem is BaseContainer;
        }

        public void PostSetupContentLoad()
        {
            // fish
            TransmutatorTileEntity.PostTransmutateItemEvent += GiveFishUnlock;
            RadianceSets.ProjectorLensTexture[ItemID.SpecularFish] = "Radiance/Content/Tiles/Transmutator/SpecularFish_Transmutator";
            RadianceSets.ProjectorLensID[ItemID.SpecularFish] = (int)ProjectorLensID.Fish;
            RadianceSets.ProjectorLensDust[ItemID.SpecularFish] = DustID.FrostDaggerfish;
            RadianceSets.ProjectorLensSound[ItemID.SpecularFish] = new SoundStyle($"{nameof(Radiance)}/Sounds/FishSplat");
            RadianceSets.ProjectorLensPreOrderedUpdateFunction[ItemID.SpecularFish] = (projector) => projector.transmutator.radianceModifier *= 25f;
        }
        private void GiveFishUnlock(TransmutatorTileEntity transmutator, TransmutationRecipe recipe)
        {
            if (RadianceSets.ProjectorLensID[transmutator.projector.LensPlaced.type] == (int)ProjectorLensID.Fish)
                UnlockSystem.transmutatorFishUsed = true;
        }

        public override void PreOrderedUpdate()
        {
            if (ContainerPlaced is not null)
                ContainerPlaced.SetTileEntityRadiance(this);
            else
                maxRadiance = storedRadiance = 0;

            if (TryGetTileEntityAs(Position.X, Position.Y - 1, out TransmutatorTileEntity entity))
                transmutator = entity;
            else
                transmutator = null;
        }

        protected override HoverUIData GetHoverData()
        {
            List<HoverUIElement> data = new List<HoverUIElement>();
            if (maxRadiance > 0)
                data.Add(new RadianceBarUIElement("RadianceBar", storedRadiance, maxRadiance, Vector2.UnitY * 40));

            return new HoverUIData(this, this.TileEntityWorldCenter(), data.ToArray());
        }

        public override void SaveExtraExtraData(TagCompound tag)
        {
            this.SaveInventory(tag);
        }

        public override void LoadExtraExtraData(TagCompound tag)
        {
            this.LoadInventory(tag);
        }
    }

    #endregion Projector

    #region Assembly

    public class AssemblableProjector : ModTile
    {
        public override string Texture => "Radiance/Content/Tiles/Transmutator/Projector";

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.CoordinateHeights = new int[3] { 16, 16, 18 };
            HitSound = SoundID.Item52;
            DustType = -1;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Radiance Projector");
            AddMapEntry(new Color(255, 197, 97), name);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<AssemblableProjectorTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (TryGetTileEntityAs(i, j, out AssemblableProjectorTileEntity entity))
            {
                entity.Draw(spriteBatch, entity.stage);
            }
            return false;
        }

        public override void MouseOver(int i, int j)
        {
            if (TryGetTileEntityAs(i, j, out AssemblableProjectorTileEntity entity))
                entity.DrawHoverUIAndMouseItem();
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (TryGetTileEntityAs(i, j, out AssemblableProjectorTileEntity entity) && !player.ItemAnimationActive)
                entity.ConsumeMaterials(player);

            return false;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (TryGetTileEntityAs(i, j, out AssemblableProjectorTileEntity entity))
            {
                entity.DropUsedItems();
                Point origin = GetTileOrigin(i, j);
                ModContent.GetInstance<AssemblableProjectorTileEntity>().Kill(origin.X, origin.Y);
            }
        }
    }

    public class AssemblableProjectorTileEntity : AssemblableTileEntity
    {
        public AssemblableProjectorTileEntity() : base(
            ModContent.TileType<AssemblableProjector>(),
            ModContent.GetInstance<ProjectorTileEntity>(),
            ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/AssemblableProjector").Value,
            new()
            {
                (CommonItemGroups.SilverBars, 4),
                (CommonItemGroups.Woods, 12),
                (CommonItemGroups.SilverBars, 6),
                ([ModContent.ItemType<ShimmeringGlass>()], 6),
            }
            )
        { }
        public override void Load()
        {
            BlueprintLoader.AddBlueprint(() => (
                nameof(Projector) + "Blueprint",
                ModContent.ItemType<ProjectorItem>(),
                ModContent.GetInstance<AssemblableProjectorTileEntity>(),
                new Color(190, 247, 219),
                1,
                UnlockCondition.UnlockedByDefault));
        }
        public override void OnStageIncrease(int stage)
        {
            if (stage < StageCount - 1)
            {
                for (int i = 0; i < 40; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Position.ToVector2() * 16 + Main.rand.NextVector2Square(0, 8) + new Vector2(2 + (i % 2 == 0 ? 20 : 0), Main.rand.NextFloat(48) - 4), DustID.Smoke);
                    dust.velocity *= 0.1f;
                    dust.scale = 1.5f;
                    dust.fadeIn = 1.2f;
                }
                SoundEngine.PlaySound(SoundID.Tink, this.TileEntityWorldCenter());
            }
            else
            {
                for (int i = 0; i < 60; i++)
                {
                    Dust dust = Main.dust[Dust.NewDust(Position.ToVector2() * 16 - Vector2.One * 2, Width * 16, Height * 16, DustID.Smoke)];
                    dust.velocity *= 0.4f;
                    dust.scale = 1.5f;
                    dust.fadeIn = 1.2f;
                }
                SoundEngine.PlaySound(SoundID.Item37, this.TileEntityWorldCenter());
            }
        }
    }

    #endregion Assembly

    public class ProjectorItem : BaseTileItem
    {
        public ProjectorItem() : base("ProjectorItem", "Radiance Projector", "Provides Radiance to a Transmutator above\nRequires a Radiance-focusing lens to be inserted in order to function", "Projector", 1, Item.sellPrice(0, 0, 10, 0), ItemRarityID.Green)
        {
        }
    }
}