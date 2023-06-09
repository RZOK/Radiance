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
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.UI;

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

            LocalizedText name = CreateMapEntryName();
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
            if (RadianceUtils.TryGetTileEntityAs(i, j, out PedestalTileEntity entity) && !player.ItemAnimationActive)
            {
                Item selItem = RadianceUtils.GetPlayerHeldItem();
                entity.DropItem(0, new Vector2(i * 16, j * 16));
                entity.SafeInsertItemIntoSlot(0, ref selItem, out bool success, 1);

                if (entity.GetSlot(0).ModItem is IRequiresPedestalStability stabilityInterface)
                    entity.idealStability = stabilityInterface.stabilizationRequirement;

                if (success)
                    SoundEngine.PlaySound(SoundID.MenuTick);
                return true;
            }
            return false;
        }
        public override bool AutoSelect(int i, int j, Item item) => item.ModItem != null && item.ModItem is IPedestalItem;
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out PedestalTileEntity entity))
            {
                Tile tile = Main.tile[i, j];
                Vector2 centerOffset = -Vector2.One * 16;
                if (entity.inventory != null && !entity.GetSlot(0).IsAir && tile.TileFrameX == 0 && tile.TileFrameY == 0)
                {
                    Color tileColor = Lighting.GetColor(i, j - 2);
                    Rectangle drawBox = Item.GetDrawHitbox(entity.GetSlot(0).type, null);
                    int yCenteringOffset = -drawBox.Height / 2 - 10;
                    Vector2 position = new Vector2(i * 16 - (int)Main.screenPosition.X, (float)(j * 16 - (int)Main.screenPosition.Y + yCenteringOffset + 3 * RadianceUtils.SineTiming(30))) + RadianceUtils.tileDrawingZero;
                    Vector2 origin = new Vector2(drawBox.Width, drawBox.Height) / 2 + centerOffset;
                    ItemSlot.DrawItemIcon(entity.GetSlot(0), 0, spriteBatch, position - centerOffset, entity.GetSlot(0).scale, 256, tileColor);
                    if (entity.ContainerPlaced != null && entity.ContainerPlaced.RadianceAdjustingTexture != null)
                    { 
                        Texture2D radianceAdjustingTexture = entity.ContainerPlaced.RadianceAdjustingTexture;

                        float radianceCharge = Math.Min(entity.ContainerPlaced.currentRadiance, entity.ContainerPlaced.maxRadiance);
                        float fill = radianceCharge / entity.ContainerPlaced.maxRadiance;

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
                        Lighting.AddLight(RadianceUtils.MultitileOriginWorldPosition(i, j) - centerOffset + new Vector2(0, (float)(yCenteringOffset + 5 * RadianceUtils.SineTiming(30))), Color.Lerp(new Color
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
                itemTextureType = entity.GetSlot(0).netID;
                entity.AddHoverUI();
            }
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = itemTextureType;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out PedestalTileEntity entity))
                entity.DropAllItems(new Vector2(i * 16, j * 16));

            Point origin = RadianceUtils.GetTileOrigin(i, j);
            ModContent.GetInstance<PedestalTileEntity>().Kill(origin.X, origin.Y);
        }
    }

    public class PedestalTileEntity : RadianceUtilizingTileEntity, IInventory, IInterfaceableRadianceCell
    {
        //Pedestals are updated last to account for absorption boosts
        public PedestalTileEntity() : base(ModContent.TileType<Pedestal>(), 0, new() { 1, 4 }, new() { 2, 3 }, 0.1f, true) { }

        public BaseContainer ContainerPlaced => this.GetSlot(0).ModItem as BaseContainer;

        public float actionTimer = 0;
        public Color aoeCircleColor = Color.White;
        public float aoeCircleRadius = 0;
        public float cellAbsorptionBoost = 0;
        public List<string> CurrentBoosts = new List<string>();

        public Item[] inventory { get; set; }
        public byte[] inputtableSlots => new byte[] { 0 };
        public byte[] outputtableSlots => new byte[] { 0 };
        protected override HoverUIData ManageHoverUI()
        {
            List<HoverUIElement> data = new List<HoverUIElement>();
            if (aoeCircleRadius > 0)
                data.Add(new CircleUIElement("AoECircle", aoeCircleRadius, aoeCircleColor));

            if (maxRadiance > 0)
                data.Add(new RadianceBarUIElement("RadianceBar", currentRadiance, maxRadiance, Vector2.UnitY * 40));
            if (idealStability > 0)
                data.Add(new StabilityBarElement("StabilityBar", stability, idealStability, Vector2.One * -40));

            if (ContainerPlaced != null && cellAbsorptionBoost + ContainerPlaced.absorptionModifier > 1)
            {
                string str = (ContainerPlaced.absorptionModifier + cellAbsorptionBoost).ToString() + "x";
                data.Add(new TextUIElement("AbsorptionModifier", str, CommonColors.RadianceColor1, new Vector2(FontAssets.MouseText.Value.MeasureString(str).X / 2 + 16 - RadianceUtils.SineTiming(33), -20 + RadianceUtils.SineTiming(50))));
            }

            return new HoverUIData(this, this.TileEntityWorldCenter(), data.ToArray());
        }
        public override void PreOrderedUpdate()
        {
            cellAbsorptionBoost = 0;
            maxRadiance = currentRadiance = 0;
            aoeCircleColor = Color.White;
            aoeCircleRadius = 0;
        }
        public override void OrderedUpdate()
        {
            this.ConstructInventory(1);

            if (IsStabilized)
                cellAbsorptionBoost += 0.1f;

            if (!this.GetSlot(0).IsAir)
            {
                ContainerPlaced?.InInterfacableContainer(this);
                if (this.GetSlot(0).ModItem is IRequiresPedestalStability stabilityInterface)
                    idealStability = stabilityInterface.stabilizationRequirement;
                else
                    idealStability = 0;

                if (this.GetSlot(0).ModItem as IPedestalItem != null && enabled)
                    PedestalItemEffect();
            }
            else
                idealStability = 0;

            CurrentBoosts.Clear();
        }
        public void AddCellBoost(string name, float amount)
        {
            if (!CurrentBoosts.Contains(name))
            {
                CurrentBoosts.Add(name);
                cellAbsorptionBoost += amount;
            }
        }
        public void PedestalItemEffect()
        { 
            IPedestalItem item = (IPedestalItem)this.GetSlot(0).ModItem;

            item.PedestalEffect(this);
            aoeCircleRadius = item.aoeCircleRadius;
            aoeCircleColor = item.aoeCircleColor;
        }

        public override void SaveExtraData(TagCompound tag)
        {
            this.SaveInventory(tag);
        }

        public override void LoadExtraData(TagCompound tag)
        {
            this.LoadInventory(tag, 1);
        }
    }

    public class PedestalItem : BaseTileItem
    {
        public PedestalItem() : base("PedestalItem", "Pedestal", "Right click with an item in hand to place it on the pedestal", "Pedestal", 5, 0, ItemRarityID.Blue) { }

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