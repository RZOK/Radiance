﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.TileItems;
using Radiance.Core;
using Radiance.Core.Interfaces;
using Radiance.Utilities;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles
{
    public class Pedestal : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileContainer[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.CoordinateHeights = new int[2] { 16, 18 };
            HitSound = SoundID.Item52;
            DustType = -1;

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Pedestal");
            AddMapEntry(new Color(43, 56, 61), name);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<PedestalTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (RadianceUtils.TryGetTileEntityAs(i, j, out PedestalTileEntity entity) && Main.myPlayer == player.whoAmI && !player.ItemAnimationActive)
            {
                Item selItem = RadianceUtils.GetPlayerHeldItem();
                bool success = false;
                entity.DropItem(0, new Vector2(i * 16, j * 16), new EntitySource_TileInteraction(null, i, j));
                entity.SafeInsertItemIntoSlot(0, ref selItem, out success, 1);
            }
            return false;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out PedestalTileEntity entity))
            {
                Tile tile = Main.tile[i, j];
                Vector2 centerOffset = new Vector2(-2, -2) / 2 * 16;
                if (entity.inventory != null && !entity.GetSlot(0).IsAir && tile.TileFrameX == 0 && tile.TileFrameY == 0)
                {
                    Color tileColor = Lighting.GetColor(i, j - 2);
                    Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
                    Texture2D texture = TextureAssets.Item[entity.GetSlot(0).type].Value;
                    int yCenteringOffset = -Item.GetDrawHitbox(entity.GetSlot(0).type, null).Height / 2 - 10;
                    Vector2 position = new Vector2(i * 16 - (int)Main.screenPosition.X, (float)(j * 16 - (int)Main.screenPosition.Y + yCenteringOffset + 5 * RadianceUtils.SineTiming(30))) + zero;
                    Vector2 origin = new Vector2(texture.Width, texture.Height) / 2 + centerOffset;
                    spriteBatch.Draw(texture, position, new Rectangle?(Item.GetDrawHitbox(entity.GetSlot(0).type, null)), tileColor, 0, new Vector2(Item.GetDrawHitbox(entity.GetSlot(0).type, null).Width, Item.GetDrawHitbox(entity.GetSlot(0).type, null).Height) / 2 + centerOffset, 1, SpriteEffects.None, 0);
                    if (entity.containerPlaced != null && entity.containerPlaced.RadianceAdjustingTexture != null)
                    {
                        Texture2D radianceAdjustingTexture = entity.containerPlaced.RadianceAdjustingTexture;

                        float radianceCharge = Math.Min(entity.containerPlaced.CurrentRadiance, entity.containerPlaced.MaxRadiance);
                        float fill = radianceCharge / entity.containerPlaced.MaxRadiance;

                        Main.EntitySpriteDraw
                        (
                            radianceAdjustingTexture,
                            position,
                            null,
                            Color.Lerp(CommonColors.RadianceColor1 * fill, CommonColors.RadianceColor2 * fill, fill * RadianceUtils.SineTiming(5)),
                            0,
                            origin,
                            1,
                            SpriteEffects.None,
                            0
                        );
                        float strength = 0.4f;
                        Lighting.AddLight(RadianceUtils.MultitileCenterWorldCoords(i, j) - centerOffset + new Vector2(0, (float)(yCenteringOffset + 5 * RadianceUtils.SineTiming(30))), Color.Lerp(new Color
                        (
                         1 * fill * strength,
                         0.9f * fill * strength,
                         0.4f * fill * strength
                        ), new Color
                        (
                         0.7f * fill * strength,
                         0.65f * fill * strength,
                         0.5f * fill * strength
                        ),
                        fill * RadianceUtils.SineTiming(20)).ToVector3());
                    }

                    if (Main.LocalPlayer.GetModPlayer<RadiancePlayer>().debugMode)
                    {
                        DynamicSpriteFont font = FontAssets.MouseText.Value;
                        DynamicSpriteFontExtensionMethods.DrawString
                        (
                            spriteBatch,
                            font,
                            entity.GetSlot(0).Name,
                            position,
                            Color.White,
                            0,
                            font.MeasureString(entity.GetSlot(0).Name) / 2 + centerOffset,
                            1,
                            SpriteEffects.None,
                            0
                        );
                    }
                }
            }
        }

        public override void MouseOver(int i, int j)
        {
            int itemTextureType = ModContent.ItemType<PedestalItem>();
            Player player = Main.LocalPlayer;
            if (RadianceUtils.TryGetTileEntityAs(i, j, out PedestalTileEntity entity) && entity.GetSlot(0).type != ItemID.None)
            {
                RadianceInterfacePlayer mp = player.GetModPlayer<RadianceInterfacePlayer>();
                if (entity.aoeCircleRadius > 0)
                {
                    mp.aoeCirclePosition = RadianceUtils.MultitileCenterWorldCoords(entity.Position.X, entity.Position.Y) + Vector2.UnitX * entity.Width * 8;
                    mp.aoeCircleColor = entity.aoeCircleColor.ToVector4();
                    mp.aoeCircleScale = entity.aoeCircleRadius;
                    mp.aoeCircleMatrix = Main.GameViewMatrix.ZoomMatrix;
                }
                itemTextureType = entity.GetSlot(0).netID;
                if (entity.MaxRadiance > 0)
                    mp.radianceContainingTileHoverOverCoords = new Vector2(i, j);
            }
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = itemTextureType;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out PedestalTileEntity entity))
                entity.DropAllItems(new Vector2(i * 16, j * 16), new EntitySource_TileBreak(i, j));

            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 16, ModContent.ItemType<PedestalItem>());
            Point16 origin = RadianceUtils.GetTileOrigin(i, j);
            ModContent.GetInstance<PedestalTileEntity>().Kill(origin.X, origin.Y);
        }
    }

    public class PedestalTileEntity : RadianceUtilizingTileEntity, IInventory
    {
        public BaseContainer containerPlaced => inventory[0].ModItem as BaseContainer;
        private float maxRadiance = 0;
        private float currentRadiance = 0;
        public float actionTimer = 0;
        public Color aoeCircleColor = Color.White;
        public float aoeCircleRadius = 0;

        public Item[] inventory { get; set; }
        public byte[] inputtableSlots => new byte[] { 0 };
        public byte[] outputtableSlots => new byte[] { 0 };

        public override float MaxRadiance
        {
            get => maxRadiance;
            set => maxRadiance = value;
        }

        public override float CurrentRadiance
        {
            get => currentRadiance;
            set => currentRadiance = value;
        }

        public override int Width => 2;
        public override int Height => 2;
        public override int ParentTile => ModContent.TileType<Pedestal>();
        public override List<int> InputTiles => new() { 1, 4 };
        public override List<int> OutputTiles => new() { 2, 3 };

        public override void Update()
        {
            this.ConstructInventory(1);
            maxRadiance = 0;
            currentRadiance = 0;

            aoeCircleColor = Color.White;
            aoeCircleRadius = 0;

            if (!this.GetSlot(0).IsAir)
                PedestalItemEffect();
            inputsConnected.Clear();
            outputsConnected.Clear();
        }

        public void PedestalItemEffect()
        {
            if (this.GetSlot(0).ModItem as IPedestalItem != null)
            {
                IPedestalItem item = ((IPedestalItem)this.GetSlot(0).ModItem);
                item.PedestalEffect(this);
                aoeCircleRadius = item.aoeCircleRadius;
                aoeCircleColor = item.aoeCircleColor;
            }
        }

        public void GetRadianceFromItem(BaseContainer container)
        {
            if (container != null)
            {
                maxRadiance = container.MaxRadiance;
                currentRadiance = container.CurrentRadiance;
            }
            else
                maxRadiance = currentRadiance = 0;
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, Width, Height);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
            }
            int placedEntity = Place(i, j - 1);
            return placedEntity;
        }

        public override void SaveData(TagCompound tag)
        {
            this.SaveInventory(ref tag);
        }

        public override void LoadData(TagCompound tag)
        {
            this.LoadInventory(ref tag, 1);
        }
    }
}