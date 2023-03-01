using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Items.TileItems;
using Radiance.Core;
using Radiance.Utilities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles.Transmutator
{
    public class Projector : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.CoordinateHeights = new int[4] { 16, 16, 16, 18 };
            HitSound = SoundID.Item52;
            DustType = -1;

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Projector");
            AddMapEntry(new Color(48, 49, 53), name);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<ProjectorTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out ProjectorTileEntity entity))
            {
                Tile tile = Main.tile[i, j];
                if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
                {
                    Color glowColor = Color.Lerp(new Color(255, 50, 50), new Color(0, 255, 255), entity.deployTimer / 35);
                    Color tileColor = Lighting.GetColor(i, j);
                    Color tileColor2 = Lighting.GetColor(i, j - 2);
                    float deployTimer = entity.deployTimer;
                    Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
                    Texture2D baseTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/ProjectorBase").Value;
                    Texture2D holderTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/ProjectorHolder").Value;
                    Texture2D glowTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/ProjectorGlow").Value;
                    Vector2 basePosition = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y + 32) + zero;
                    if (RadianceUtils.TryGetTileEntityAs(i, j - 2, out TransmutatorTileEntity transEntity))
                    {
                        if (transEntity.glowTime > 0)
                            glowColor = Color.Lerp(new Color(0, 255, 255), CommonColors.RadianceColor1, transEntity.glowTime / 90);
                        if (transEntity.craftingTimer > 0)
                        {
                            RadianceDrawing.DrawSoftGlow(RadianceUtils.MultitileCenterWorldCoords(i, j) + zero + new Vector2(entity.Width, entity.Height) * 8, CommonColors.RadianceColor1 * (transEntity.craftingTimer / 120), 0.3f * (transEntity.craftingTimer / 120), RadianceDrawing.DrawingMode.Tile);
                            RadianceDrawing.DrawSoftGlow(RadianceUtils.MultitileCenterWorldCoords(i, j) + zero + new Vector2(entity.Width, entity.Height) * 8, Color.White * (transEntity.craftingTimer / 120), 0.2f * (transEntity.craftingTimer / 120), RadianceDrawing.DrawingMode.Tile);
                        }
                        if (transEntity.projectorBeamTimer > 0)
                        {
                            RadianceDrawing.DrawBeam(RadianceUtils.MultitileCenterWorldCoords(i, j) + zero + new Vector2(entity.Width, entity.Height) * 8, RadianceUtils.MultitileCenterWorldCoords(i, j) - Vector2.UnitY + zero + new Vector2(entity.Width * 8, -2), Color.White.ToVector4() * transEntity.projectorBeamTimer / 60, 0.5f, 8, RadianceDrawing.DrawingMode.Tile);
                            RadianceDrawing.DrawBeam(RadianceUtils.MultitileCenterWorldCoords(i, j) + zero + new Vector2(entity.Width, entity.Height) * 8, RadianceUtils.MultitileCenterWorldCoords(i, j) - Vector2.UnitY + zero + new Vector2(entity.Width * 8, -2), CommonColors.RadianceColor1.ToVector4() * transEntity.projectorBeamTimer / 60, 0.5f, 6, RadianceDrawing.DrawingMode.Tile);
                        }
                    }
                    if (entity.inventory != null && !entity.GetSlot(0).IsAir && entity.lensID != ProjectorLensID.None)
                    {
                        string modifier = entity.lensID.ToString();
                        Texture2D glassTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/Lens" + modifier).Value;
                        Main.spriteBatch.End();
                        Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, null, Matrix.Identity);

                        Main.spriteBatch.Draw
                        (
                            glassTexture,
                            basePosition - (Vector2.UnitY * (float)(32 * RadianceUtils.EaseInOutQuart(deployTimer / 105))) + (Vector2.UnitX * 12),
                            null,
                            tileColor,
                            0,
                            Vector2.Zero,
                            1,
                            SpriteEffects.None,
                            0
                        );
                        Main.spriteBatch.End();
                        spriteBatch.Begin(default, default, default, default, default, null, Matrix.Identity);
                    }
                    //holder
                    Main.spriteBatch.Draw
                    (
                        holderTexture,
                        basePosition - (Vector2.UnitY * 2) - (Vector2.UnitY * (float)(32 * RadianceUtils.EaseInOutQuart(deployTimer / 105))) + (Vector2.UnitX * 2),
                        null,
                        tileColor2,
                        0,
                        Vector2.Zero,
                        1,
                        SpriteEffects.None,
                        0
                    );
                    //base
                    Main.spriteBatch.Draw
                    (
                        baseTexture,
                        basePosition,
                        null,
                        tileColor,
                        0,
                        Vector2.Zero,
                        1,
                        SpriteEffects.None,
                        0
                    );
                    //glow
                    Main.spriteBatch.Draw
                    (
                        glowTexture,
                        basePosition + new Vector2(10, 2),
                        null,
                        glowColor,
                        0,
                        Vector2.Zero,
                        1,
                        SpriteEffects.None,
                        0
                    );

                    //if (deployTimer > 0)
                    //{
                    //    Vector2 pos = new Vector2(i * 16, j * 16) + zero + new Vector2(entity.Width / 2, 0.7f) * 16 + Vector2.UnitX * 8; //tile world coords + half entity width (center of multitiletile) + a bit of increase
                    //    float mult = (float)Math.Clamp(Math.Abs(RadianceUtils.SineTiming(120)), 0.85f, 1f); //color multiplier
                    //    for (int h = 0; h < 2; h++)
                    //        RadianceDrawing.DrawBeam(pos, new Vector2(pos.X, 0), h == 1 ? new Color(255, 255, 255, entity.beamTimer).ToVector4() * mult : new Color(0, 255, 255, entity.beamTimer).ToVector4() * mult, 0.2f, h == 1 ? 10 : 14, Matrix.Identity);
                    //    RadianceDrawing.DrawSoftGlow(pos - Vector2.UnitY * 2, new Color(0, 255, 255, entity.beamTimer) * mult, 0.25f, Matrix.Identity);
                    //}
                }
            }
            return false;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            RadianceInterfacePlayer mp = player.GetModPlayer<RadianceInterfacePlayer>();
            if (RadianceUtils.TryGetTileEntityAs(i, j, out ProjectorTileEntity entity))
            {
                if (entity.deployed)
                {
                    if (Main.tile[i, j].TileFrameX <= 18 && Main.tile[i, j].TileFrameY <= 18)
                    {
                        player.noThrow = 2;
                        player.cursorItemIconEnabled = true;
                        player.cursorItemIconID = entity.GetSlot(0).IsAir ? ModContent.ItemType<ShimmeringGlass>() : entity.GetSlot(0).type;
                    }
                }
                if (entity.MaxRadiance > 0)
                    mp.radianceContainingTileHoverOverCoords = new Vector2(i, j);
            }
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (Main.tile[i, j].TileFrameX <= 18 && Main.tile[i, j].TileFrameY <= 18)
            {
                if (RadianceUtils.TryGetTileEntityAs(i, j, out ProjectorTileEntity entity) && !player.ItemAnimationActive && entity.deployed)
                {
                    Item selItem = RadianceUtils.GetPlayerHeldItem();
                    if(selItem.ModItem as IProjectorLens != null || entity.lens != null)
                    {
                        int dust = selItem.ModItem as IProjectorLens == null ? entity.lens.DustID : (selItem.ModItem as IProjectorLens).DustID;
                        bool success = false;
                        entity.DropItem(0, new Vector2(i * 16, j * 16), new EntitySource_TileInteraction(null, i, j));
                        if(selItem.ModItem as IProjectorLens != null)
                            entity.SafeInsertItemIntoSlot(0, ref selItem, out success, 1);
                        SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/LensPop"), new Vector2(i * 16 + entity.Width * 8, j * 16 + -entity.Height * 8));
                        SpawnLensDust(RadianceUtils.MultitileCenterWorldCoords(i, j) - (Vector2.UnitY * 2) + (Vector2.UnitX * 10), dust);
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
                int d = Dust.NewDust(pos, 8, 32, dust);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 0.1f;
                Main.dust[d].scale = 1.7f;
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out ProjectorTileEntity entity))
            {
                if (entity.lensID != ProjectorLensID.None)
                {
                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/LensPop"), new Vector2(i * 16 + entity.Width * 8, j * 16 + -entity.Height * 8));
                    SpawnLensDust(RadianceUtils.MultitileCenterWorldCoords(i, j) - (Vector2.UnitY * 2) + (Vector2.UnitX * 10), (entity.GetSlot(0).ModItem as IProjectorLens).DustID);
                    entity.DropAllItems(new Vector2(i * 16, j * 16), new EntitySource_TileBreak(i, j));
                }
                Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 16, ModContent.ItemType<ProjectorItem>());
                Point16 origin = RadianceUtils.GetTileOrigin(i, j);
                ModContent.GetInstance<ProjectorTileEntity>().Kill(origin.X, origin.Y);
            }
        }
    }

    public class ProjectorTileEntity : RadianceUtilizingTileEntity, IInventory
    {
        #region Fields

        private float currentRadiance = 0;
        private float maxRadiance = 0;
        public float deployTimer = 0;
        public bool hasTransmutator => Main.tile[Position.X, Position.Y - 1].TileType == ModContent.TileType<Transmutator>() && Main.tile[Position.X, Position.Y - 1].TileFrameX == 0;
        public bool deployed => deployTimer == 105;
        public IProjectorLens lens => this.GetSlot(0).ModItem as IProjectorLens;
        public ProjectorLensID lensID => lens != null ? lens.ID : ProjectorLensID.None;

        #endregion Fields

        #region Propeties

        public override float CurrentRadiance
        {
            get => currentRadiance;
            set => currentRadiance = value;
        }

        public override float MaxRadiance
        {
            get => maxRadiance;
            set => maxRadiance = value;
        }

        public override int Width => 2;

        public override int Height => 4;
        public override int ParentTile => ModContent.TileType<Projector>();
        public override List<int> InputTiles => new() { 7, 8 };

        public override List<int> OutputTiles => new();

        public Item[] inventory { get; set; }

        public byte[] inputtableSlots => new byte[] { 0 };

        public byte[] outputtableSlots => Array.Empty<byte>();

        #endregion Propeties

        public override void Update()
        {
            this.ConstructInventory(1);
            Vector2 position = new Vector2(Position.X, Position.Y) * 16 + new Vector2(Width / 2, 0.7f) * 16 + Vector2.UnitX * 8;
            if (hasTransmutator)
            {
                if (deployTimer < 105)
                {
                    if (deployTimer == 1)
                        SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/ProjectorLift"), position + new Vector2(Width * 8, -Height * 8));
                    deployTimer++;
                }
                if (RadianceUtils.TryGetTileEntityAs(Position.X, Position.Y - 1, out TransmutatorTileEntity entity))
                    if (!entity.isCrafting)
                        MaxRadiance = CurrentRadiance = 0;
            }
            else
            {
                CurrentRadiance = MaxRadiance = 0;
                if (deployTimer > 0)
                {
                    if (lensID != ProjectorLensID.None)
                    {
                        SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/LensPop"), new Vector2(position.X, position.Y));
                        Projector.SpawnLensDust(RadianceUtils.MultitileCenterWorldCoords(Position.X, Position.Y) - (Vector2.UnitY * 2) + (Vector2.UnitX * 10), (this.GetSlot(0).ModItem as IProjectorLens).DustID);
                        this.DropItem(0, new Vector2(position.X, position.Y), new EntitySource_TileEntity(this));
                        this.SetItemInSlot(0, new Item(0, 1));
                    }
                    if (deployTimer == 104)
                        SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/ProjectorLift"), position + new Vector2(Width * 8, -Height * 8));
                    deployTimer--;
                }
            }
            inputsConnected.Clear();
            outputsConnected.Clear();
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, Width, Height);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
            }
            int placedEntity = Place(i - 1, j - 2);
            return placedEntity;
        }

        public override void SaveData(TagCompound tag)
        {
            if (CurrentRadiance > 0)
                tag["CurrentRadiance"] = CurrentRadiance;
            this.SaveInventory(ref tag);
        }

        public override void LoadData(TagCompound tag)
        {
            CurrentRadiance = tag.Get<float>("CurrentRadiance");
            this.LoadInventory(ref tag, 1);
        }
    }
}