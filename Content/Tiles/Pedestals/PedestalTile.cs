using Microsoft.Xna.Framework.Input;
using Radiance.Content.Items.BaseItems;
using Radiance.Core.Systems;
using ReLogic.Graphics;
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
            if (TryGetTileEntityAs(i, j, out PedestalTileEntity entity) && !Main.LocalPlayer.ItemAnimationActive && !Main.LocalPlayer.mouseInterface)
            {
                Item selItem = Main.LocalPlayer.HeldItem;
                byte slot = (byte)((selItem.dye > 0 && selItem.type != ItemID.TeamDye) ? 1 : 0);

                entity.DropItem(slot, new Vector2(i * 16, j * 16), out bool success);
                if (!selItem.favorited && !selItem.IsAir)
                    entity.SafeInsertItemIntoInventory(selItem, out success, true, true);

                entity.OnItemInsert();

                if (success)
                {
                    entity.actionTimer = 0;
                    TileEntitySystem.shouldUpdateStability = true;
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    return true;
                }
            }
            return false;
        }

        public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
            {
                if (TryGetTileEntityAs(i, j, out PedestalTileEntity entity))
                    entity.DrawHoveringItemAndTrim(spriteBatch, i, j, Texture, trimOffset);
            }
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            if (drawData.tileFrameX == 0 && drawData.tileFrameY == 0 && TryGetTileEntityAs(i, j, out PedestalTileEntity _))
                Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
        }

        public override void MouseOver(int i, int j)
        {
            int itemTextureType = itemType;
            if (TryGetTileEntityAs(i, j, out PedestalTileEntity entity))
            {
                if (entity.GetSlot(0).type != ItemID.None)
                    itemTextureType = entity.GetSlot(0).type;
                else if (Main.LocalPlayer.HeldItem.dye > 0)
                    itemTextureType = Main.LocalPlayer.HeldItem.type;

                entity.AddHoverUI();
            }
            Main.LocalPlayer.SetCursorItem(itemTextureType);
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (TryGetTileEntityAs(i, j, out PedestalTileEntity entity))
                entity.DropAllItems(new Vector2(i * 16, j * 16));

            ModContent.GetInstance<T>().Kill(i, j);
        }
    }

    public abstract class PedestalTileEntity : RadianceUtilizingTileEntity, IInventory, IInterfaceableRadianceCell, ISpecificStackSlotInventory
    {
        // Pedestals are updated last to account for absorption boosts applied earlier
        public PedestalTileEntity(int parentTile) : base(parentTile, 0, new() { 1, 4 }, new() { 2, 3 }, 0.1f, true) 
        {
            inventorySize = 2;
            this.ConstructInventory();
        }

        public BaseContainer ContainerPlaced => this.GetSlot(0).ModItem as BaseContainer;
        public Item PaintPlaced => this.GetSlot(1);

        public float actionTimer = 0;
        public Color aoeCircleColor = Color.White;
        public float aoeCircleRadius = 0;
        public float cellAbsorptionBoost = 0;
        public Dictionary<string, float> CurrentBoosts = new Dictionary<string, float>();

        public Item[] inventory { get; set; }
        public int inventorySize { get; set; }
        public byte[] inputtableSlots => new byte[] { 0 };
        public byte[] outputtableSlots => new byte[] { 0 };

        public Dictionary<int, int> allowedStackPerSlot => new Dictionary<int, int>()
        {
            [0] = 1,
        };

        public bool TryInsertItemIntoSlot(Item item, byte slot, bool overrideValidInputs, bool ignoreItemImprint)
        {
            if (!overrideValidInputs && !inputtableSlots.Contains(slot))
                return false;

            if (item.dye > 0)
                return slot == 1;

            return slot == 0;

        }
        protected override HoverUIData ManageHoverUI()
        {
            List<HoverUIElement> data = new List<HoverUIElement>();
            if (aoeCircleRadius > 0)
                data.Add(new CircleUIElement("AoECircle", aoeCircleRadius, aoeCircleColor));

            if (maxRadiance > 0)
                data.Add(new RadianceBarUIElement("RadianceBar", storedRadiance, maxRadiance, Vector2.UnitY * 40));

            if (idealStability > 0)
                data.Add(new StabilityBarElement("StabilityBar", stability, idealStability, Vector2.One * -40));

            if (ContainerPlaced is not null && ContainerPlaced.canAbsorbItems && cellAbsorptionBoost != 0)
            {
                string str = MathF.Round(1f + cellAbsorptionBoost, 2).ToString() + "x";
                Vector2 offset = new Vector2(-SineTiming(33), SineTiming(50));
                if (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift))
                    offset = Vector2.Zero;

                data.Add(new TextUIElement("AbsorptionModifier", str, CommonColors.RadianceColor1, new Vector2(FontAssets.MouseText.Value.MeasureString(str).X / 2 + 16, -20) + offset));
            }

            return new HoverUIData(this, this.TileEntityWorldCenter(), data.ToArray());
        }

        public override void PreOrderedUpdate()
        {
            cellAbsorptionBoost = 0;
            maxRadiance = storedRadiance = 0;
            aoeCircleColor = Color.White;
            aoeCircleRadius = 0;
            CurrentBoosts.Clear();

            if (!this.GetSlot(0).IsAir && this.GetSlot(0).ModItem is IPedestalItem pedestalItem)
                pedestalItem.PreUpdatePedestal(this);
        }

        public override void OrderedUpdate()
        {
            if (!this.GetSlot(0).IsAir && this.GetSlot(0).ModItem is IPedestalItem pedestalItem)
            {
                if(pedestalItem is BaseContainer container)
                {
                    if (IsStabilized)
                        AddCellBoost("StabilityBoost", .1f);

                    if (container.absorptionAdditiveBoost != 0)
                        AddCellBoost("ContainerBoost", container.absorptionAdditiveBoost);
                }
                CurrentBoosts.Values.ToList().ForEach(x => cellAbsorptionBoost += x);
                pedestalItem.UpdatePedestal(this);
            }
        }

        /// <summary>
        /// Adds a Radiance Cell absorption boost for the tick.
        /// </summary>
        /// <param name="name">The name of the boost. Each unique source should be named differently.</param>
        /// <param name="amount">Decimal amount of the boost. .15f is a 15% additive increase.</param>
        public void AddCellBoost(string name, float amount)
        {
            CurrentBoosts.TryAdd(name, amount);
        }

        public override void SaveExtraExtraData(TagCompound tag)
        {
            this.SaveInventory(tag);
        }

        public override void LoadExtraExtraData(TagCompound tag)
        {
            this.LoadInventory(tag);
        }

        public override void SetIdealStability()
        {
            if (!this.GetSlot(0).IsAir)
                idealStability = RadianceSets.SetPedestalStability[this.GetSlot(0).type];
            else
                idealStability = 0;
        }

        internal void DrawHoveringItemAndTrim(SpriteBatch spriteBatch, int i, int j, string texture, Vector2? trimOffset = null)
        {
            Vector2 tilePosition = Position.ToVector2() * 16 - Main.screenPosition + TileDrawingZero;
            Color tileColor = Lighting.GetColor(Position.ToPoint());
            if (inventory != null)
            {
                if (!this.GetSlot(0).IsAir)
                {
                    Vector2 itemPosition = GetFloatingItemCenter(this.GetSlot(0)) - Main.screenPosition + TileDrawingZero;
                    Color hoveringItemColor = Lighting.GetColor(Position.X, Position.Y - 2);

                    ItemSlot.DrawItemIcon(this.GetSlot(0), 0, spriteBatch, itemPosition, this.GetSlot(0).scale, 256, hoveringItemColor);

                    if (ContainerPlaced is not null && ContainerPlaced.HasRadianceAdjustingTexture)
                    {
                        float radianceCharge = Math.Min(ContainerPlaced.storedRadiance, ContainerPlaced.maxRadiance);
                        float fill = radianceCharge / ContainerPlaced.maxRadiance;

                        float strength = 0.4f * fill;
                        Lighting.AddLight(itemPosition, Color.Lerp(new Color
                        (
                         1 * strength,
                         0.9f * strength,
                         0.4f * strength
                        ), new Color
                        (
                         0.7f * strength,
                         0.65f * strength,
                         0.5f * strength
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
                if (this.GetSlot(1).dye > 0)
                {
                    Texture2D extraTexture = ModContent.Request<Texture2D>(texture + "Extra").Value;

                    spriteBatch.End();
                    RadianceDrawing.SpriteBatchData.TileDrawingData.BeginSpriteBatchFromTemplate(spriteSortMode: SpriteSortMode.Immediate);

                    ArmorShaderData shader = GameShaders.Armor.GetSecondaryShader(this.GetSlot(1).dye, null);
                    shader.Apply(null, new DrawData(extraTexture, this.TileEntityWorldCenter() + trimOffset ?? Vector2.Zero, null, tileColor, 0, extraTexture.Size() / 2, 1, SpriteEffects.None, 0));
                    spriteBatch.Draw(extraTexture, tilePosition - trimOffset ?? Vector2.Zero, null, tileColor, 0, extraTexture.Size() / 2, 1, SpriteEffects.None, 0);

                    spriteBatch.End();
                    RadianceDrawing.SpriteBatchData.TileDrawingData.BeginSpriteBatchFromTemplate();
                }
            }
        }

        public void OnItemInsert()
        {
            if (!this.GetSlot(0).IsAir)
                idealStability = RadianceSets.SetPedestalStability[this.GetSlot(0).type];
        }

        public Vector2 GetFloatingItemCenter(Item item)
        {
            int yCenteringOffset = -Item.GetDrawHitbox(item.type, null).Height / 2 - 10;
            return this.TileEntityWorldCenter() + Vector2.UnitY * (yCenteringOffset + 3 * SineTiming(30));
        }
    }
}