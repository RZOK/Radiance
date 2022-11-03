using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Common;
using Radiance.Common.Globals;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.TileItems;
using Radiance.Utils;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
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

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Pedestal");
            AddMapEntry(new Color(43, 56, 61), name);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<PedestalTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }
        internal static readonly IList<GlobalItem> globalItems = new List<GlobalItem>();
        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (TileUtils.TryGetTileEntityAs(i, j, out PedestalTileEntity entity))
            {
                Item selItem = player.inventory[player.selectedItem];

                if (player.GetModPlayer<RadiancePlayer>().debugMode)
                {
                    Main.NewText("Selected Item: " + selItem);
                    Main.NewText("Tile Pos: " + (i * 16).ToString() + ", " + (j * 16).ToString(), Color.LightCoral);
                    Main.NewText("Player Pos: " + player.position.ToString(), Color.Cyan);
                    Main.NewText("- - -");
                }
                if (entity.itemPlaced.type != 0)
                {
                    int num = Item.NewItem(new EntitySource_TileEntity(entity), i * 16, j * 16, 1, 1, entity.itemPlaced.type, 1, false, 0, false, false);
                    Item item = Main.item[num];

                    //todo: make this work for transferring all globalitem values from placed item to dropped item
                    //Instanced<GlobalItem>[] globalItemsArray = new Instanced<GlobalItem>[0];
                    //globalItemsArray = globalItems
                    //        .Select(g => new Instanced<GlobalItem>(g.Index, g))
                    //        .ToArray();
                    //foreach (var theitem in globalItemsArray)
                    //{
                    //    Console.WriteLine(theitem);
                    //}
                    GlobalItem globalItem = entity.itemPlaced.GetGlobalItem<RadianceGlobalItem>();
                    item.netDefaults(entity.itemPlaced.netID);
                    item.Prefix(entity.itemPlaced.prefix);
                    item.velocity.Y = Main.rand.Next(-20, -5) * 0.2f;
                    item.velocity.X = Main.rand.Next(-20, 21) * 0.2f;
                    item.position.Y -= item.height;
                    item.newAndShiny = false;
#nullable enable
                    BaseContainer? newContainer = item.ModItem as BaseContainer;
#nullable disable
                    if (entity.containerPlaced != null && newContainer != null)
                    {
                        newContainer.CurrentRadiance = entity.containerPlaced.CurrentRadiance;
                    }
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, num, 0f, 0f, 0f, 0, 0, 0);
                    }
                }
                selItem.stack -= 1;
                entity.itemPlaced = selItem;
            }
            return true;
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (TileUtils.TryGetTileEntityAs(i, j, out PedestalTileEntity entity))
            {
                Tile tile = Main.tile[i, j];
                Vector2 centerOffset = new Vector2(-2, -2) / 2 * 16;
                if (entity.itemPlaced.type != ItemID.None && tile.TileFrameX == 0 && tile.TileFrameY == 0)
                {
                    Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
                    Texture2D texture = Terraria.GameContent.TextureAssets.Item[entity.itemPlaced.type].Value;
                    int yCenteringOffset = -texture.Height / 2 - 10;
                    if (Main.LocalPlayer.GetModPlayer<RadiancePlayer>().debugMode)
                    {
                        DynamicSpriteFont font = Terraria.GameContent.FontAssets.MouseText.Value;
                        DynamicSpriteFontExtensionMethods.DrawString(
                            spriteBatch, 
                            font, 
                            entity.itemPlaced.Name, 
                            new Vector2(i * 16 - (int)Main.screenPosition.X, (float)(j * 16 - (int)Main.screenPosition.Y + yCenteringOffset * 2 + 5 * MathUtils.sineTiming(30))) + zero, 
                            Color.White, 
                            0, 
                            font.MeasureString(entity.itemPlaced.Name) / 2 + centerOffset, 
                            1, 
                            SpriteEffects.None, 
                            0
                            );
                    }
                    Vector2 origin = new Vector2(texture.Width, texture.Height) / 2 + centerOffset;
                    Main.EntitySpriteDraw(
                        texture, 
                        new Vector2(i * 16 - (int)Main.screenPosition.X, (float)(j * 16 - (int)Main.screenPosition.Y + yCenteringOffset + 5 * MathUtils.sineTiming(30))) + zero, 
                        null, 
                        Color.White, 
                        0, 
                        origin, 
                        1, 
                        SpriteEffects.None, 
                        0
                        );
                }
            }
        }
        public override void MouseOver(int i, int j)
        {
            int itemTextureType = ModContent.ItemType<PedestalItem>();
            Player player = Main.LocalPlayer;
            if (TileUtils.TryGetTileEntityAs(i, j, out PedestalTileEntity entity) && entity.itemPlaced.type != 0)
            {
                itemTextureType = entity.itemPlaced.netID;
                player.GetModPlayer<RadiancePlayer>().radianceContainingTileHoverOverCoords = new Vector2(i, j);
                player.GetModPlayer<RadiancePlayer>().hoveringOverRadianceContainingTile = true;
            }
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = itemTextureType;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (TileUtils.TryGetTileEntityAs(i, j, out PedestalTileEntity entity))
            {
                int num = Item.NewItem(new EntitySource_TileEntity(entity), i * 16, j * 16, 1, 1, entity.itemPlaced.type, 1, false, 0, false, false);
                Item item = Main.item[num];

                //todo: make this work for transferring all globalitem values from placed item to dropped item
                //Instanced<GlobalItem>[] globalItemsArray = new Instanced<GlobalItem>[0];
                //globalItemsArray = globalItems
                //        .Select(g => new Instanced<GlobalItem>(g.Index, g))
                //        .ToArray();
                //foreach (var theitem in globalItemsArray)
                //{
                //    Console.WriteLine(theitem);
                //}

                item.netDefaults(entity.itemPlaced.netID);
                item.Prefix(entity.itemPlaced.prefix);
                item.velocity.Y = Main.rand.Next(-10, 0) * 0.2f;
                item.velocity.X = Main.rand.Next(-10, 11) * 0.2f;
                item.position.Y -= item.height;
                item.newAndShiny = false;
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, num, 0f, 0f, 0f, 0, 0, 0);
                }
            }
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 16, ModContent.ItemType<PedestalItem>());
            Point16 origin = TileUtils.GetTileOrigin(i, j);
            ModContent.GetInstance<PedestalTileEntity>().Kill(origin.X, origin.Y);
        }
    }

    public class PedestalTileEntity : RadianceUtilizingTileEntity
    {
        public Item itemPlaced = new Item(0, 1);
#nullable enable
        public BaseContainer? containerPlaced;
#nullable disable
        private float maxRadiance = 0;
        private float currentRadiance = 0;
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
        public override void Update()
        {
            maxRadiance = 0;
            currentRadiance = 0;
            BaseContainer container = itemPlaced.ModItem as BaseContainer;
            if (container != null) GetRadianceFromItem(container);
        }
        public void GetRadianceFromItem(BaseContainer container)
        {
            maxRadiance = container.MaxRadiance;
            currentRadiance = container.CurrentRadiance;
        }
        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                int width = 2;
                int height = 2;
                NetMessage.SendTileSquare(Main.myPlayer, i, j, width, height);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
            }
            Point16 tileOrigin = new Point16(0, 1); //for some reason the position is off if you don't reduce the Y by one
            int placedEntity = Place(i - tileOrigin.X, j - tileOrigin.Y);
            return placedEntity;
        }
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<Pedestal>();
        } 
        public override void OnNetPlace()
        {
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
            }
        }

        public override void SaveData(TagCompound tag)
        {
            if (itemPlaced != null)
                tag["Item"] = itemPlaced;
        }
        public override void LoadData(TagCompound tag)
        {
            itemPlaced = tag.Get<Item>("Item");
        }
    }
}