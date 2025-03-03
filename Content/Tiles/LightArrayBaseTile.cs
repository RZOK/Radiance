using Radiance.Content.Items.BaseItems;
using ReLogic.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using Terraria.ObjectData;
using Terraria.UI;

namespace Radiance.Content.Tiles
{
    public class LightArrayBaseTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = false;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.CoordinateHeights = new int[2] { 16, 18 };
            DustType = -1;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Light Array Base");
            AddMapEntry(new Color(239, 196, 139), name);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<LightArrayBaseTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }

        public override void HitWire(int i, int j)
        {
            ToggleTileEntity(i, j);
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            if (drawData.tileFrameX == 0 && drawData.tileFrameY == 0 && TryGetTileEntityAs(i, j, out LightArrayBaseTileEntity _))
                Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
        }

        public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (TryGetTileEntityAs(i, j, out LightArrayBaseTileEntity entity))
            {
                Texture2D displayTex = ModContent.Request<Texture2D>("Radiance/Content/Tiles/LightArrayBaseTileDisplay").Value;
                Texture2D glowTex = ModContent.Request<Texture2D>("Radiance/Content/Tiles/LightArrayBaseTileGlow").Value;
                Vector2 displayPos = entity.TileEntityWorldCenter() - Vector2.UnitY * (20 + 2 * SineTiming(30, new Point(i, j).GetSmoothTileRNG() * 120)) - Main.screenPosition + TileDrawingZero;
                Vector2 displayScale = new Vector2(EaseInExponent(Math.Clamp(entity.displayTimer / (LightArrayBaseTileEntity.DISPLAY_TIMER_MAX / 4f), 0.2f, 1f), 2f), EaseInOutExponent(Math.Clamp(entity.displayTimer / (LightArrayBaseTileEntity.DISPLAY_TIMER_MAX / 3f) - 1f, 0.15f, 1f), 2.8f)) * 0.95f;

                Vector2 glowPos = entity.TileEntityWorldCenter() - Main.screenPosition + TileDrawingZero;
                bool hasDye = !entity.GetSlot(1).IsAir;

                if (hasDye)
                {
                    spriteBatch.End();
                    RadianceDrawing.SpriteBatchData.TileDrawingData.BeginSpriteBatchFromTemplate(spriteSortMode: SpriteSortMode.Immediate);

                    ArmorShaderData shader = GameShaders.Armor.GetSecondaryShader(entity.GetSlot(1).dye, null);
                    shader.Apply(null, new DrawData(displayTex, displayPos, null, Color.White, 0, displayTex.Size() / 2, displayScale, SpriteEffects.None, 0));
                }
                Tile aboveTileLeft = Framing.GetTileSafely(i, j - 1);
                Tile aboveTileRight = Framing.GetTileSafely(i + 1, j - 1);
                bool drawDisplay = entity.displayTimer > 0;

                if ((aboveTileLeft.HasUnactuatedTile && Main.tileSolid[aboveTileLeft.TileType]) || (aboveTileRight.HasUnactuatedTile && Main.tileSolid[aboveTileRight.TileType]))
                    drawDisplay = false;

                if (drawDisplay)
                {
                    spriteBatch.Draw(displayTex, displayPos, null, Color.White, 0, displayTex.Size() / 2, displayScale, SpriteEffects.None, 0);
                    spriteBatch.Draw(glowTex, glowPos, null, Color.White * 0.5f, 0, glowTex.Size() / 2, 1f, SpriteEffects.None, 0);
                }

                if (hasDye)
                {
                    spriteBatch.End();
                    RadianceDrawing.SpriteBatchData.TileDrawingData.BeginSpriteBatchFromTemplate();
                }
                if (entity.placedLightArray != null)
                {
                    Texture2D holeTex = ModContent.Request<Texture2D>("Radiance/Content/Tiles/LightArrayBaseTileHole").Value;
                    Texture2D coverTex = ModContent.Request<Texture2D>("Radiance/Content/Tiles/LightArrayBaseTileCover").Value;
                    Texture2D arrayTex = ModContent.Request<Texture2D>(entity.placedLightArray.miniTexture).Value;

                    Vector2 coverPosition = entity.TileEntityWorldCenter() + Vector2.UnitY * 4 + TileDrawingZero - Main.screenPosition;
                    Vector2 coverScale = new Vector2(1f, Math.Max(0f, 1f - (entity.insertionTimer / LightArrayBaseTileEntity.INSERTION_TIMER_MAX) * 2f));

                    float arrayScale = Lerp(2f, 1f, EaseInOutExponent(entity.insertionTimer / LightArrayBaseTileEntity.INSERTION_TIMER_MAX, 6));
                    float arrayColorModifier = Lerp(0.3f, 1f, EaseInOutExponent(Math.Max(0, (entity.insertionTimer - 15f) / LightArrayBaseTileEntity.INSERTION_TIMER_MAX), 6f));

                    spriteBatch.Draw(holeTex, coverPosition, null, Lighting.GetColor(i, j), 0, Vector2.UnitX * holeTex.Width / 2f, 1f, SpriteEffects.None, 0);
                    spriteBatch.Draw(coverTex, coverPosition, null, Lighting.GetColor(i, j) * (1f - (entity.insertionTimer / LightArrayBaseTileEntity.INSERTION_TIMER_MAX)), 0, Vector2.UnitX * coverTex.Width / 2f, coverScale, SpriteEffects.None, 0);
                    spriteBatch.Draw(arrayTex, coverPosition + Vector2.UnitY * arrayTex.Height / 2f, null, Lighting.GetColor(i, j) * arrayColorModifier, 0, arrayTex.Size() / 2, arrayScale, SpriteEffects.None, 0);
                }
            }
        }

        public override bool RightClick(int i, int j)
        {
            if (TryGetTileEntityAs(i, j, out LightArrayBaseTileEntity entity) && !Main.LocalPlayer.ItemAnimationActive)
            {
                Item item = GetPlayerHeldItem();
                BaseLightArray heldLightArray = item.ModItem as BaseLightArray;
                bool success = false;

                if ((!entity.redirectedInventory.GetFirstSlotWithItem(out _) && (item.favorited || item.IsAir)) || Main.keyState.PressingShift() || heldLightArray is not null)
                {
                    if(entity.GetSlot(0).ModItem is BaseLightArray insertedLightArray)
                        insertedLightArray.currentBase = null;

                    entity.DropItem(0, entity.TileEntityWorldCenter(), out success);
                    if (heldLightArray is not null)
                    {
                        heldLightArray.currentBase = entity;
                        entity.SafeInsertItemIntoInventory(item, out bool dropInventorySuccess);
                        success |= dropInventorySuccess;
                        entity.displayTimer = entity.insertionTimer = 0;
                    }
                }
                else if (entity.redirectedInventory != entity && item.dye < 1)
                {
                    if (item.IsAir || item.favorited)
                    {
                        byte lastSlot = entity.redirectedInventory.GetSlotsWithItems().Last();
                        entity.redirectedInventory.DropItem(lastSlot, entity.TileEntityWorldCenter(), out success);
                    }
                    else
                        entity.redirectedInventory.SafeInsertItemIntoInventory(item, out success, true);
                }
                else if (!item.favorited && item.dye > 0 && item.type != ItemID.TeamDye)
                {
                    entity.DropItem(1, entity.TileEntityWorldCenter(), out bool dropInventorySuccess);
                    entity.SafeInsertItemIntoInventory(item, out success);
                    success |= dropInventorySuccess;
                }
                //if (LightArrayBaseSystem.lightArrays.TryGetValue(new Point16(i, j), out LightArrayBase lightArray))
                //{
                //    if (!lightArray.lightArrayItem.IsAir)
                //    {
                //        NewItemSpecific(MultitileWorldCenter(i, j), lightArray.lightArrayItem);
                //        success = true;
                //    }
                //    if (item.ModItem is BaseLightArray)
                //    {
                //        lightArray.lightArrayItem = item.Clone();
                //        item.TurnToAir();
                //        success = true;
                //    }
                //}

                if (success)
                    SoundEngine.PlaySound(SoundID.MenuTick);

                return true;
            }
            return false;
        }

        public override void MouseOver(int i, int j)
        {
            if (TryGetTileEntityAs(i, j, out LightArrayBaseTileEntity entity))
            {
                int mouseItem = ModContent.ItemType<LightArrayBaseTileItem>();
                Item heldItem = GetPlayerHeldItem();

                if (entity.placedLightArrayItem != null && !entity.placedLightArrayItem.IsAir)
                    mouseItem = entity.placedLightArrayItem.type;

                if (!heldItem.IsAir && (heldItem.ModItem is BaseLightArray || (heldItem.dye > 0 && heldItem.type != ItemID.TeamDye)))
                    mouseItem = heldItem.type;

                Main.LocalPlayer.SetCursorItem(mouseItem);
                entity.AddHoverUI();

                //if (LightArrayBaseSystem.lightArrays.TryGetValue(new Point16(i, j), out LightArrayBase lightArray))
                //{
                //    foreach (var item in lightArray.items)
                //    {
                //        Main.NewText(item.Name);
                //    }
                //}
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (TryGetTileEntityAs(i, j, out LightArrayBaseTileEntity entity))
                entity.DropAllItems(entity.TileEntityWorldCenter());

            ModContent.GetInstance<LightArrayBaseTileEntity>().Kill(i, j);
        }
    }

    public class LightArrayBaseTileEntity : ImprovedTileEntity, IInventory, IRedirectInterfacableInventory
    {
        public LightArrayBaseTileEntity() : base(ModContent.TileType<LightArrayBaseTile>(), 1)
        {
            inventorySize = 2;
            this.ConstructInventory();
        }

        public Item[] inventory { get; set; }
        public int inventorySize { get; set; }
        public byte[] inputtableSlots => new byte[1] { 0 };
        public byte[] outputtableSlots => new byte[1] { 0 };
        public Item placedLightArrayItem => this.GetSlot(0);
        public BaseLightArray placedLightArray => placedLightArrayItem.ModItem as BaseLightArray;
        public Item placedDye => this.GetSlot(1);
        public bool TryInsertItemIntoSlot(Item item, byte slot, bool overrideValidInputs, bool ignoreItemImprint)
        {
            if ((!overrideValidInputs && !inputtableSlots.Contains(slot)))
                return false;

            if (slot == 0 && placedLightArray != null)
                return false;

            if (slot == 1)
                return item.dye > 0;

            return item.ModItem is not null && item.ModItem is BaseLightArray;
        }
        public IInventory redirectedInventory
        {
            get
            {
                if (placedLightArray != null)
                    return placedLightArray;

                return this;
            }
        }

        public int displayTimer = 0;
        public static readonly float DISPLAY_TIMER_MAX = 40;

        public int insertionTimer = 0;
        public static readonly float INSERTION_TIMER_MAX = 60;

        public override void OrderedUpdate()
        {
            if (placedLightArray != null)
            {
                if(placedLightArray.currentBase == null)
                    placedLightArray.currentBase = this;

                if (insertionTimer < INSERTION_TIMER_MAX)
                    insertionTimer++;

                if (insertionTimer == INSERTION_TIMER_MAX && displayTimer < 60)
                    displayTimer++;

                if (insertionTimer == 40)
                    SoundEngine.PlaySound(new("Radiance/Sounds/LightArrayInsert"), this.TileEntityWorldCenter());
            }
            else
            {
                if (displayTimer > 0)
                    displayTimer = Math.Max(0, displayTimer - 2);

                if (insertionTimer > 0)
                    insertionTimer--;
            }
        }

        protected override HoverUIData GetHoverData()
        {
            List<HoverUIElement> data = new List<HoverUIElement>();
            if (placedLightArray is not null && placedLightArray.GetFirstSlotWithItem(out _))
            {
                data.Add(new LightArrayBaseUIElement("ItemDisplay", placedLightArray.inventory.ToList(), Vector2.UnitY * -8));
            }

            return new HoverUIData(this, this.TileEntityWorldCenter(), data.ToArray());
        }

        public override void SaveExtraData(TagCompound tag)
        {
            this.SaveInventory(tag);
        }

        public override void LoadExtraData(TagCompound tag)
        {
            this.LoadInventory(tag);
        }

        //public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        //{
        //    LightArrayBaseSystem.lightArrays.Add(Position, new LightArrayBase(Position));
        //    //sync lightarraybases here
        //    return base.Hook_AfterPlacement(i, j, type, style, direction, alternate);
        //}
        //public override void OnKill()
        //{
        //    LightArrayBaseSystem.lightArrays.Remove(Position);
        //    //sync lightarraybases here
        //    base.OnKill();
        //}
    }

    public class LightArrayBaseUIElement : HoverUIElement
    {
        public List<Item> items;

        public LightArrayBaseUIElement(string name, List<Item> items, Vector2 targetPosition) : base(name)
        {
            this.items = items.Where(x => !x.IsAir).ToList();
            this.targetPosition = targetPosition;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            int columns = (int)MathF.Ceiling(MathF.Sqrt(items.Count));
            int rows = (int)MathF.Ceiling(items.Count / (float)columns);

            const int distanceBetweenItems = 36;
            int drawWidth = columns * distanceBetweenItems;
            int drawHeight = rows * distanceBetweenItems;
            int padding = 4;
            DrawRadianceInvBG(spriteBatch, (int)realDrawPosition.X - drawWidth / 2 - padding / 2, (int)realDrawPosition.Y - drawHeight - padding / 2, drawWidth + padding, drawHeight + padding, 0.5f * timerModifier, RadianceInventoryBGDrawMode.Default);

            int x = 0;
            int y = 0;
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];
                Vector2 itemPos = new Vector2(realDrawPosition.X - drawWidth / 2 + x * distanceBetweenItems + distanceBetweenItems / 2, realDrawPosition.Y - drawHeight + y * distanceBetweenItems + 26) - Vector2.UnitY * 8 * timerModifier;
                ItemSlot.DrawItemIcon(item, 0, spriteBatch, itemPos, 1f, 32, Color.White * timerModifier);
                if (item.stack > 1)
                    Utils.DrawBorderStringFourWay(Main.spriteBatch, font, item.stack.ToString(), itemPos.X - 14, itemPos.Y + 12, Color.White * timerModifier, Color.Black * timerModifier, Vector2.UnitY * font.MeasureString(item.stack.ToString()).Y / 2, 0.85f);

                x++;
                if (x == columns)
                {
                    x = 0;
                    y++;
                }
            }
        }
    }

    public class LightArrayBaseTileItem : BaseTileItem
    {
        public LightArrayBaseTileItem() : base("LightArrayBaseTileItem", "Light Array Scanner", "Placeholder", "LightArrayBaseTile", 1, Item.sellPrice(0, 0, 0, 0), ItemRarityID.Green)
        {
        }
    }
}