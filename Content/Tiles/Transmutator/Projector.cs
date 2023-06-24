﻿using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Items.RadianceCells;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ObjectData;

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
                    Vector2 basePosition = entity.TileEntityWorldCenter() - Main.screenPosition + tileDrawingZero;
                    Texture2D baseTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/ProjectorBase").Value;
                    Texture2D glowTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/ProjectorGlow").Value;
                    Texture2D braceTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/ProjectorBraces").Value;
                    Color color = Lighting.GetColor(i, j);
                    if (entity.inventory != null && !entity.GetSlot(0).IsAir && entity.lensID != ProjectorLensID.None)
                    {
                        string modifier = entity.lensID.ToString();
                        Texture2D glassTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/Lens" + modifier).Value;

                        Main.spriteBatch.Draw
                        (
                            glassTexture,
                            basePosition + new Vector2(0, -20),
                            null,
                            color,
                            0,
                            glassTexture.Size() / 2,
                            1,
                            SpriteEffects.None,
                            0
                        );
                    }
                    if (entity.inventory != null && !entity.GetSlot(1).IsAir && entity.ContainerPlaced != null && entity.ContainerPlaced.MiniTexture != null)
                    {
                        Main.spriteBatch.Draw
                        (
                            entity.ContainerPlaced.MiniTexture,
                            basePosition + new Vector2(0, 5),
                            null,
                            color,
                            0,
                            new Vector2(entity.ContainerPlaced.MiniTexture.Width / 2, entity.ContainerPlaced.MiniTexture.Height / 2 - (entity.ContainerPlaced.MiniTexture.Height / 2 % 2) + 1),
                            1,
                            SpriteEffects.None,
                            0
                        );
                    }
                    Main.spriteBatch.Draw(baseTexture, basePosition, null, color, 0, baseTexture.Size() / 2, 1, SpriteEffects.None, 0);
                    if (entity.transmutator != null)
                    {
                        if (entity.transmutator.craftingTimer > 0)
                        {
                            Color glowColor = Color.White * EaseOutCirc(entity.transmutator.craftingTimer / 120);
                            Main.spriteBatch.Draw(glowTexture, basePosition, null, glowColor, 0, baseTexture.Size() / 2, 1, SpriteEffects.None, 0);
                            for (int h = 0; h < 2; h++)
                                RadianceDrawing.DrawBeam(basePosition + Main.screenPosition + Vector2.UnitY * 6, basePosition + Main.screenPosition - Vector2.UnitY * 20, h == 1 ? (Color.White * 0.3f * (entity.transmutator.craftingTimer / 120)).ToVector4() : (CommonColors.RadianceColor1 * 0.3f * (entity.transmutator.craftingTimer / 120)).ToVector4(), 0.1f, h == 1 ? 8 : 12, RadianceDrawing.SpriteBatchData.WorldDrawingData);
                        }
                        if (entity.transmutator.projectorBeamTimer > 0)
                        {
                            for (int h = 0; h < 2; h++)
                            {
                                RadianceDrawing.DrawBeam(basePosition + Main.screenPosition - Vector2.UnitY * 20, basePosition + Main.screenPosition - Vector2.UnitY * 48, h == 1 ? (Color.White * (entity.transmutator.projectorBeamTimer / 60)).ToVector4() : (CommonColors.RadianceColor1 * (entity.transmutator.projectorBeamTimer / 60)).ToVector4(), 0.1f, h == 1 ? 8 : 12, RadianceDrawing.SpriteBatchData.WorldDrawingData);
                            }
                        }
                    }
                    if (entity.hasTransmutator)
                        Main.spriteBatch.Draw(braceTexture, basePosition - Vector2.UnitY * 12, null, color, 0, braceTexture.Size() / 2, 1, SpriteEffects.None, 0);
                }
            }
            return false;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            RadianceInterfacePlayer mp = player.GetModPlayer<RadianceInterfacePlayer>();
            if (TryGetTileEntityAs(i, j, out ProjectorTileEntity entity))
            {
                player.noThrow = 2;
                player.cursorItemIconEnabled = true;
                if (entity.inventory != null)
                {
                    if (Main.tile[i, j].TileFrameY == 0)
                        player.cursorItemIconID = entity.GetSlot(0).IsAir ? ModContent.ItemType<ShimmeringGlass>() : entity.GetSlot(0).type;
                    else
                        player.cursorItemIconID = entity.GetSlot(1).IsAir ? ModContent.ItemType<StandardRadianceCell>() : entity.GetSlot(1).type;
                }
                entity.AddHoverUI();
            }
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (TryGetTileEntityAs(i, j, out ProjectorTileEntity entity) && !player.ItemAnimationActive)
            {
                Item selItem = GetPlayerHeldItem();
                if (Main.tile[i, j].TileFrameY == 0)
                {
                    if (selItem.ModItem as IProjectorLens != null || entity.lens != null)
                    {
                        int dust = selItem.ModItem as IProjectorLens == null ? entity.lens.DustID : (selItem.ModItem as IProjectorLens).DustID;
                        bool success = false;
                        entity.DropItem(0, new Vector2(i * 16, j * 16));
                        if (selItem.ModItem as IProjectorLens != null)
                            entity.SafeInsertItemIntoSlot(0, ref selItem, out success, 1);
                        SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/LensPop"), new Vector2(i * 16 + entity.Width * 8, j * 16 + -entity.Height * 8));
                        SpawnLensDust(MultitileOriginWorldPosition(i, j) + new Vector2(10, -10), dust);
                        return true;
                    }
                }
                else
                {
                    if (selItem.ModItem as BaseContainer != null || entity.ContainerPlaced != null)
                    {
                        bool success = false;
                        entity.DropItem(1, new Vector2(i * 16, j * 16));
                        if (selItem.ModItem as BaseContainer != null)
                            entity.SafeInsertItemIntoSlot(1, ref selItem, out success, 1);
                        SoundEngine.PlaySound(SoundID.Tink, new Vector2(i * 16 + entity.Width * 8, j * 16 + entity.Height * 8));
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
                if (entity.lensID != ProjectorLensID.None)
                {
                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/LensPop"), new Vector2(i * 16 + entity.Width * 8, j * 16 + -entity.Height * 8));
                    SpawnLensDust(MultitileOriginWorldPosition(i, j) - (Vector2.UnitY * 2) + (Vector2.UnitX * 10), (entity.GetSlot(0).ModItem as IProjectorLens).DustID);
                    entity.DropAllItems(new Vector2(i * 16, j * 16));
                }
                Point origin = GetTileOrigin(i, j);
                ModContent.GetInstance<ProjectorTileEntity>().Kill(origin.X, origin.Y);
            }
        }
    }

    public class ProjectorTileEntity : RadianceUtilizingTileEntity, IInventory, IInterfaceableRadianceCell
    {
        public ProjectorTileEntity() : base(ModContent.TileType<Projector>(), 0, new() { 5, 6 }, new()) { }

        public TransmutatorTileEntity transmutator;
        public bool hasTransmutator => transmutator != null;
        public IProjectorLens lens => this.GetSlot(0).ModItem as IProjectorLens;
        public ProjectorLensID lensID => lens != null ? lens.ID : ProjectorLensID.None;
        public BaseContainer ContainerPlaced => this.GetSlot(1).ModItem as BaseContainer;

        public Item[] inventory { get; set; }

        public byte[] inputtableSlots => new byte[] { 0, 1 };

        public byte[] outputtableSlots => new byte[] { 1 };

        public override void OrderedUpdate()
        {
            this.ConstructInventory(2);

            if (Main.tile[Position.X, Position.Y - 1].TileType == ModContent.TileType<Transmutator>() && Main.tile[Position.X, Position.Y - 1].TileFrameX == 0)
            {
                if (TryGetTileEntityAs(Position.X, Position.Y - 1, out TransmutatorTileEntity entity))
                    transmutator = entity;
                else
                    transmutator = null;
            }
            else
                transmutator = null;

            this.GetRadianceFromItem();
        }
        protected override HoverUIData ManageHoverUI()
        {
            List<HoverUIElement> data = new List<HoverUIElement>();
            if (maxRadiance > 0)
                data.Add(new RadianceBarUIElement("RadianceBar", currentRadiance, maxRadiance, Vector2.UnitY * 40));

            return new HoverUIData(this, this.TileEntityWorldCenter(), data.ToArray());
        }
        public override void SaveExtraData(TagCompound tag)
        {
            this.SaveInventory(tag);
        }

        public override void LoadExtraData(TagCompound tag)
        {
            this.LoadInventory(tag, 2);
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
                entity.Draw(spriteBatch, entity.CurrentStage);
            }
            return false;
        }

        public override void MouseOver(int i, int j)
        {
            if (TryGetTileEntityAs(i, j, out AssemblableProjectorTileEntity entity))
                entity.DrawHoverUI();
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
            ModContent.TileType<Projector>(),
            ModContent.GetInstance<ProjectorTileEntity>(),
            4,
            ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/AssemblableProjector").Value,
            new List<(int, int)>()
            {
                (9, 12),
                (21, 6),
                (ModContent.ItemType<ShimmeringGlass>(), 6),
            }
            )
        { }

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
        public ProjectorItem() : base("ProjectorItem", "Radiance Projector", "Provides Radiance to a Transmutator above\nRequires a Radiance-focusing lens to be installed in order to function", "Projector", 1, Item.sellPrice(0, 0, 10, 0), ItemRarityID.Green) { }
    }

    public class ProjectorBlueprint : BaseTileItem
    {
        public override string Texture => "Radiance/Content/ExtraTextures/Blueprint";
        public ProjectorBlueprint() : base("ProjectorBlueprint", "Mysterious Blueprint", "Begins the assembly of an arcane machine", "AssemblableProjector", 1, Item.sellPrice(0, 0, 5, 0), ItemRarityID.Blue) { }
    }
}