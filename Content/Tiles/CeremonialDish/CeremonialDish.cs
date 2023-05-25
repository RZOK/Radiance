using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Core;
using Radiance.Core.Interfaces;
using Radiance.Utilities;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles.CeremonialDish
{
    public class CeremonialDish : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.CoordinateHeights = new int[2] { 16, 18 };
            HitSound = SoundID.Dig;
            DustType = -1;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Alluring Dish");
            AddMapEntry(new Color(0, 188, 207), name);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<CeremonialDishTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }

        public override void HitWire(int i, int j)
        {
            RadianceUtils.ToggleTileEntity(i, j);
        }
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out CeremonialDishTileEntity entity))
            {
                Tile tile = Framing.GetTileSafely(i, j);
                string texPath = entity.GetFirstSlotWithItem(out _) ? CeremonialDishTileEntity.filledTexture : CeremonialDishTileEntity.emptyTexture;
                Texture2D texture = ModContent.Request<Texture2D>(texPath).Value;
                if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
                {
                    Color tileColor = Lighting.GetColor(i, j);
                    Vector2 mainPosition = new Vector2(i, j) * 16 + new Vector2(entity.Width * 8, entity.Height * 16) + RadianceUtils.tileDrawingZero - Main.screenPosition;
                    Vector2 origin = new Vector2(texture.Width / 2, texture.Height);
                    Main.spriteBatch.Draw
                    (
                        texture,
                        mainPosition,
                        null,
                        tileColor,
                        0,
                        origin,
                        1,
                        SpriteEffects.None,
                        0
                    );
                }
            }
            return false;
        }

        public override bool RightClick(int i, int j)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out CeremonialDishTileEntity entity))
            {
                Item item = RadianceUtils.GetPlayerHeldItem();
                byte slot = (byte)(item.type == ItemID.Grubby ? 0 : item.type == ItemID.Sluggy ? 1 : item.type == ItemID.Buggy ? 2 : 3);
                bool success = false;
                if (slot == 3 && entity.GetFirstSlotWithItem(out byte dropSlot))
                    entity.DropItem(dropSlot, new Vector2(i * 16, j * 16));
                if (slot != 3)
                {
                    if (entity.GetSlot(slot).stack == entity.GetSlot(slot).maxStack)
                        entity.DropItem(slot, new Vector2(i * 16, j * 16));
                    entity.SafeInsertItemIntoSlot(slot, ref item, out success);
                }
                if (success)
                    SoundEngine.PlaySound(SoundID.MenuTick);

                return true;
            }
            return false;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (RadianceUtils.TryGetTileEntityAs(i, j, out CeremonialDishTileEntity entity))
            {
                player.noThrow = 2;
                player.cursorItemIconEnabled = true;
                player.cursorItemIconID = ItemID.Grubby;
                entity.AddHoverUI();
            }
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out CeremonialDishTileEntity entity))
                entity.DropAllItems(entity.TileEntityWorldCenter());

            Point16 origin = RadianceUtils.GetTileOrigin(i, j);
            ModContent.GetInstance<CeremonialDishTileEntity>().Kill(origin.X, origin.Y);
        }
    }

    public class CeremonialDishTileEntity : ImprovedTileEntity, IInventory
    {
        public CeremonialDishTileEntity() : base(ModContent.TileType<CeremonialDish>(), 1) { }

        public Item[] inventory { get; set; }
        public byte[] inputtableSlots => new byte[] { 0, 1, 2 };
        public byte[] outputtableSlots => Array.Empty<byte>();
        internal const string emptyTexture = "Radiance/Content/Tiles/CeremonialDish/CeremonialDishEmpty";
        internal const string filledTexture = "Radiance/Content/Tiles/CeremonialDish/CeremonialDishFilled";
        public override void Load()
        {
            ModContent.Request<Texture2D>(emptyTexture, AssetRequestMode.ImmediateLoad);
            ModContent.Request<Texture2D>(filledTexture, AssetRequestMode.ImmediateLoad);
        }
        public override void OrderedUpdate()
        {
            this.ConstructInventory(3);
        }

        protected override HoverUIData ManageHoverUI()
        {
            List<HoverUIElement> data = new List<HoverUIElement>()
            {
                new CeremonialDishUIElement("GrubbyCount", 0, new Vector2(-40, -30)),
                new CeremonialDishUIElement("SluggyCount", 1, new Vector2(0, -40)),
                new CeremonialDishUIElement("BuggyCount", 2, new Vector2(40, -30)),
            };

            return new HoverUIData(this, this.TileEntityWorldCenter(), data.ToArray());
        }
        public override void SaveData(TagCompound tag)
        {
            this.SaveInventory(ref tag);
        }

        public override void LoadData(TagCompound tag)
        {
            this.LoadInventory(ref tag, 3);
            base.LoadData(tag);
        }
    }
    public class CeremonialDishUIElement : HoverUIElement
    {
        public byte slot = 0;

        public CeremonialDishUIElement(string name, byte slot, Vector2 targetPosition) : base(name)
        {
            this.targetPosition = targetPosition;
            this.slot = slot;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            CeremonialDishTileEntity entity = parent.entity as CeremonialDishTileEntity;
            if (entity != null)
            {
                Color grubbyColor = new Color(139, 86, 218);
                Color sluggyColor = new Color(218, 182, 86);
                Color buggyColor = new Color(183, 59, 82);
                RadianceDrawing.DrawSoftGlow(elementPosition, (slot == 0 ? grubbyColor : slot == 1 ? sluggyColor : buggyColor) * timerModifier, Math.Max(0.3f * (float)Math.Abs(RadianceUtils.SineTiming(100)), 0.32f), RadianceDrawing.DrawingMode.Default);
                RadianceDrawing.DrawSoftGlow(elementPosition, Color.White * timerModifier, Math.Max(0.2f * (float)Math.Abs(RadianceUtils.SineTiming(100)), 0.22f), RadianceDrawing.DrawingMode.Default);
                RadianceDrawing.DrawHoverableItem(Main.spriteBatch, entity.GetSlot(slot).type, realDrawPosition, entity.GetSlot(slot).stack, Color.White * timerModifier);
            }
        }
    }
    public class CeremonialDishItem : BaseTileItem
    {
        public CeremonialDishItem() : base("CeremonialDishItem", "Alluring Dish", "Attracts Wyvern Hatchlings when proper bait is placed inside", "CeremonialDish", 1, Item.sellPrice(0, 1, 0, 0), ItemRarityID.Pink) { }
    }
}