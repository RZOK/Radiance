using Terraria.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.ProjectorLenses;
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

        public override void HitWire(int i, int j)
        {
            RadianceUtils.ToggleTileEntity(i, j);
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (RadianceUtils.TryGetTileEntityAs(i, j, out PedestalTileEntity entity) && Main.myPlayer == player.whoAmI && !player.ItemAnimationActive)
            {
                Item selItem = RadianceUtils.GetPlayerHeldItem();
                entity.DropItem(0, new Vector2(i * 16, j * 16), new EntitySource_TileInteraction(null, i, j));
                entity.SafeInsertItemIntoSlot(0, ref selItem, out bool success, 1);
                if (success)
                    SoundEngine.PlaySound(SoundID.MenuTick);
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
                    Texture2D texture = TextureAssets.Item[entity.GetSlot(0).type].Value;
                    int yCenteringOffset = -Item.GetDrawHitbox(entity.GetSlot(0).type, null).Height / 2 - 10;
                    Vector2 position = new Vector2(i * 16 - (int)Main.screenPosition.X, (float)(j * 16 - (int)Main.screenPosition.Y + yCenteringOffset + 5 * RadianceUtils.SineTiming(30))) + RadianceUtils.tileDrawingZero;
                    Vector2 origin = new Vector2(texture.Width, texture.Height) / 2 + centerOffset;
                    spriteBatch.Draw(texture, position, new Rectangle?(Item.GetDrawHitbox(entity.GetSlot(0).type, null)), tileColor, 0, new Vector2(Item.GetDrawHitbox(entity.GetSlot(0).type, null).Width, Item.GetDrawHitbox(entity.GetSlot(0).type, null).Height) / 2 + centerOffset, 1, SpriteEffects.None, 0);
                    if (entity.containerPlaced != null && entity.containerPlaced.RadianceAdjustingTexture != null)
                    {
                        Texture2D radianceAdjustingTexture = entity.containerPlaced.RadianceAdjustingTexture;

                        float radianceCharge = Math.Min(entity.containerPlaced.currentRadiance, entity.containerPlaced.maxRadiance);
                        float fill = radianceCharge / entity.containerPlaced.maxRadiance;

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
                        Lighting.AddLight(RadianceUtils.GetMultitileWorldPosition(i, j) - centerOffset + new Vector2(0, (float)(yCenteringOffset + 5 * RadianceUtils.SineTiming(30))), Color.Lerp(new Color
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
                List<HoverUIElement> data = new List<HoverUIElement>();
                if (entity.aoeCircleRadius > 0)
                    data.Add(new CircleUIElement(entity.aoeCircleRadius, entity.aoeCircleColor));

                itemTextureType = entity.GetSlot(0).netID;
                if (entity.maxRadiance > 0)
                    data.Add(new RadianceBarUIElement(entity.currentRadiance, entity.maxRadiance, Vector2.UnitY * 40));

                mp.currentHoveredObjects.Add(new HoverUIData(entity, entity.TileEntityWorldCenter(), data.ToArray()));
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
        public PedestalTileEntity() : base(ModContent.TileType<Pedestal>(), 0, new() { 1, 4 }, new() { 2, 3 }) { }
        public BaseContainer containerPlaced => this.GetSlot(0).ModItem as BaseContainer;

        public float actionTimer = 0;
        public Color aoeCircleColor = Color.White;
        public float aoeCircleRadius = 0;

        public Item[] inventory { get; set; }
        public byte[] inputtableSlots => new byte[] { 0 };
        public byte[] outputtableSlots => new byte[] { 0 };

        public override void Update()
        {
            this.ConstructInventory(1);
            maxRadiance = 0;
            currentRadiance = 0;

            aoeCircleColor = Color.White;
            aoeCircleRadius = 0;

            if (!this.GetSlot(0).IsAir)
            {
                if (containerPlaced != null)
                    containerPlaced.OnPedestal(this);

                if (this.GetSlot(0).ModItem as IPedestalItem != null && enabled)
                    PedestalItemEffect();
            }
        }

        public void PedestalItemEffect()
        {
            IPedestalItem item = ((IPedestalItem)this.GetSlot(0).ModItem);
            item.PedestalEffect(this);
            aoeCircleRadius = item.aoeCircleRadius;
            aoeCircleColor = item.aoeCircleColor;
        }

        public void GetRadianceFromItem(BaseContainer container)
        {
            if (container != null)
            {
                maxRadiance = container.maxRadiance;
                currentRadiance = container.currentRadiance;
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
            base.SaveData(tag);
            this.SaveInventory(ref tag);
        }

        public override void LoadData(TagCompound tag)
        {
            base.LoadData(tag);
            this.LoadInventory(ref tag, 1);
        }
    }
    public class PedestalItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Pedestal");
            Tooltip.SetDefault("Right click with an item in hand to place it on the pedestal");
            SacrificeTotal = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 20;
            Item.maxStack = 999;
            Item.value = Item.buyPrice(0, 0, 5, 0);
            Item.rare = ItemRarityID.Blue;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Pedestal>();
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Granite, 10)
                .AddIngredient(ItemID.Marble, 10)
                .AddIngredient<ShimmeringGlass>(2)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}