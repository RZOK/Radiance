using Radiance.Content.Items.BaseItems;
using ReLogic.Graphics;
using Steamworks;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using Terraria.ObjectData;
using Terraria.UI;

namespace Radiance.Content.Tiles.Pedestals
{
    public abstract class PedestalTile<T> : ModTile where T : ModTileEntity
    {
        public int itemType;
        public Vector2 trimOffset;
        public PedestalTile(int itemType, Vector2? trimOffset = null) 
        {
            this.itemType = itemType;
            this.trimOffset = trimOffset ?? Vector2.Zero;
        }
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

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<T>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }

        public override void HitWire(int i, int j)
        {
            ToggleTileEntity(i, j);
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (TryGetTileEntityAs(i, j, out PedestalTileEntity entity) && !player.ItemAnimationActive)
            {
                Item selItem = GetPlayerHeldItem();
                byte slot = (byte)((selItem.dye <= 0) ? 0 : 1);  

                entity.DropItem(slot, new Vector2(i * 16, j * 16));
                entity.SafeInsertItemIntoSlot(slot, ref selItem, out bool success, 1);

                if (success)
                    SoundEngine.PlaySound(SoundID.MenuTick);

                return true;
            }
            return false;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (TryGetTileEntityAs(i, j, out PedestalTileEntity entity))
            {
                entity.DrawHoveringItemAndTrim(spriteBatch, i, j, Texture, trimOffset);
            }
        }

        public override void MouseOver(int i, int j)
        {
            int itemTextureType = itemType;
            if (TryGetTileEntityAs(i, j, out PedestalTileEntity entity))
            {
                if (entity.GetSlot(0).type != ItemID.None)
                    itemTextureType = entity.GetSlot(0).type;
                else if (Main.LocalPlayer.GetPlayerHeldItem().dye > 0)
                    itemTextureType = Main.LocalPlayer.GetPlayerHeldItem().type;

                entity.AddHoverUI();
            }
            Main.LocalPlayer.SetCursorItem(itemTextureType);
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (TryGetTileEntityAs(i, j, out PedestalTileEntity entity))
                entity.DropAllItems(new Vector2(i * 16, j * 16));

            Point origin = GetTileOrigin(i, j);
            ModContent.GetInstance<T>().Kill(origin.X, origin.Y);
        }
    }
    public abstract class PedestalTileEntity : RadianceUtilizingTileEntity, IInventory, IInterfaceableRadianceCell
    {
        //Pedestals are updated last to account for absorption boosts
        public PedestalTileEntity(int parentTile) : base(parentTile, 0, new() { 1, 4 }, new() { 2, 3 }, 0.1f, true) { }

        public BaseContainer ContainerPlaced => this.GetSlot(0).ModItem as BaseContainer;
        public Item PaintPlaced => this.GetSlot(1);

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
                data.Add(new TextUIElement("AbsorptionModifier", str, CommonColors.RadianceColor1, new Vector2(FontAssets.MouseText.Value.MeasureString(str).X / 2 + 16 - SineTiming(33), -20 + SineTiming(50))));
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
            this.ConstructInventory(2);

            if (IsStabilized)
                cellAbsorptionBoost += 0.1f;

            if (!this.GetSlot(0).IsAir)
            {
                ContainerPlaced?.InInterfacableContainer(this);
                idealStability = RadianceSets.SetPedestalStability[this.GetSlot(0).type];

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
            this.LoadInventory(tag, 2);
        }

        public void DrawHoveringItemAndTrim(SpriteBatch spriteBatch, int i, int j, string texture, Vector2? trimOffset = null)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            Vector2 tilePosition = Position.ToVector2() * 16 - Main.screenPosition + tileDrawingZero;
            Color tileColor = Lighting.GetColor(Position.ToPoint());
            if (inventory != null)
            {
                if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
                {
                    if (!this.GetSlot(0).IsAir)
                    {
                        Vector2 itemPosition = GetFloatingItemCenter(this.GetSlot(0)) - Main.screenPosition + tileDrawingZero;
                        Color hoveringItemColor = Lighting.GetColor(Position.X, Position.Y - 2);

                        ItemSlot.DrawItemIcon(this.GetSlot(0), 0, spriteBatch, itemPosition, this.GetSlot(0).scale, 256, hoveringItemColor);

                        if (ContainerPlaced != null && ContainerPlaced.RadianceAdjustingTexture != null)
                        {
                            Texture2D radianceAdjustingTexture = ContainerPlaced.RadianceAdjustingTexture;

                            float radianceCharge = Math.Min(ContainerPlaced.currentRadiance, ContainerPlaced.maxRadiance);
                            float fill = radianceCharge / ContainerPlaced.maxRadiance;

                            Main.EntitySpriteDraw(radianceAdjustingTexture, itemPosition, null, Color.Lerp(CommonColors.RadianceColor1 * fill, CommonColors.RadianceColor2 * fill, fill * SineTiming(5)), 0, Item.GetDrawHitbox(this.GetSlot(0).type, null).Size() / 2, 1, SpriteEffects.None, 0);

                            float strength = 0.4f;
                            Lighting.AddLight(itemPosition, Color.Lerp(new Color
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
                            fill * SineTiming(20)).ToVector3());
                        }

                        if (Main.LocalPlayer.GetModPlayer<RadiancePlayer>().debugMode)
                        {
                            DynamicSpriteFont font = FontAssets.MouseText.Value;
                            DynamicSpriteFontExtensionMethods.DrawString
                            (
                                spriteBatch,
                                font,
                                this.GetSlot(0).Name,
                                itemPosition,
                                Color.White,
                                0,
                                font.MeasureString(this.GetSlot(0).Name) / 2,
                                1,
                                SpriteEffects.None,
                                0
                            );
                        }
                    }
                }

                if (tile.TileFrameX == 18 && tile.TileFrameY == 18)
                {
                    if (this.GetSlot(1).dye > 0)
                    {
                        Texture2D fullTexture = ModContent.Request<Texture2D>(texture + "Full").Value;
                        Texture2D extraTexture = ModContent.Request<Texture2D>(texture + "Extra").Value;

                        spriteBatch.End();
                        RadianceDrawing.SpriteBatchData.TileDrawingData.BeginSpriteBatchFromTemplate(spriteSortMode: SpriteSortMode.Immediate);
                        ArmorShaderData shader = GameShaders.Armor.GetSecondaryShader(this.GetSlot(1).dye, null);

                        shader.Apply(null, new DrawData(fullTexture, this.TileEntityWorldCenter() + trimOffset ?? Vector2.Zero, null, tileColor, 0, fullTexture.Size() / 2, 1, SpriteEffects.None, 0));

                        spriteBatch.Draw(extraTexture, tilePosition - trimOffset ?? Vector2.Zero, null, tileColor, 0, extraTexture.Size() / 2, 1, SpriteEffects.None, 0);

                        spriteBatch.End();
                        RadianceDrawing.SpriteBatchData.TileDrawingData.BeginSpriteBatchFromTemplate();
                    }
                }
            }
        }
        public Vector2 GetFloatingItemCenter(Item item)
        {
            int yCenteringOffset = -Item.GetDrawHitbox(item.type, null).Height / 2 - 10;
            return this.TileEntityWorldCenter() + Vector2.UnitY * (yCenteringOffset + 3 * SineTiming(30));
        }
    }
}
