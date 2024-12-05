using Radiance.Content.Items;
using Radiance.Core.Loaders;
using Radiance.Core.Systems;
using ReLogic.Graphics;

namespace Radiance.Core.TileEntities
{
    public abstract class AssemblableTileEntity : ImprovedTileEntity
    {
        public int stage = 0;
        public int NextStage => stage + 1;
        public int StageCount => stageMaterials.Count;
        public Texture2D Texture;
        /// <summary>
        /// The first item in the list will be consumed to place the assemblable tile.
        /// </summary>
        public List<(int[] items, int stack)> stageMaterials;
        public ImprovedTileEntity entityToTurnInto;
        public Dictionary<int, int> itemsConsumed = new Dictionary<int, int> ();

        public AssemblableTileEntity(int parentTile, ImprovedTileEntity entityToTurnInto, Texture2D texture, List<(int[], int)> stageMaterials, float updateOrder = 1, bool usesStability = false) : base(parentTile, updateOrder, usesStability)
        {
            Texture = texture;
            this.stageMaterials = stageMaterials;
            this.entityToTurnInto = entityToTurnInto;
        }

        public void ConsumeMaterials(Player player)
        {
            int[] items = stageMaterials[NextStage].items;
            Dictionary<int, int> slotsToPullFrom = new Dictionary<int, int>();
            int amountLeft = stageMaterials[NextStage].stack;   
            for (int i = 0; i < 58; i++)
            {
                if (items.Contains(player.inventory[i].type))
                {
                    slotsToPullFrom.Add(i, Math.Min(amountLeft, player.inventory[i].stack));
                    amountLeft -= Math.Clamp(amountLeft, 0, player.inventory[i].stack);
                    if (amountLeft == 0)
                    {
                        foreach (var slot in slotsToPullFrom)
                        {
                            Item item = player.inventory[slot.Key];
                            if (!itemsConsumed.ContainsKey(item.type))
                                itemsConsumed[item.type] = slot.Value;
                            else
                                itemsConsumed[item.type] += slot.Value;

                            item.stack -= slotsToPullFrom[slot.Key];
                            if (item.stack <= 0)
                                item.TurnToAir();
                        }
                        stage++;
                        OnStageIncrease(stage);
                        return;
                    }
                }
            }
        }

        public virtual void OnStageIncrease(int stage) { }
        public override void OrderedUpdate()
        {
            if (stage == StageCount - 1)
            {
                Kill(Position.X, Position.Y);
                TileEntitySystem.TileEntitiesToPlace.Add(entityToTurnInto, Position.ToPoint());
                for (int i = 0; i < Width * Height; i++)
                {
                    Tile tile = Framing.GetTileSafely(Position.X + i % Width, Position.Y + i / Width);
                    tile.TileType = (ushort)entityToTurnInto.ParentTile;
                }
            }
        }
        public void DrawHoverUIAndMouseItem()
        {
            AddHoverUI();
            Main.LocalPlayer.SetCursorItem(GetShiftingItemAtTier(NextStage));
        }
        protected override HoverUIData ManageHoverUI()
        {
            List<HoverUIElement> data = new List<HoverUIElement>()
                {
                    new AssemblyHoverElement("MaterialCount", -Vector2.UnitY * (Height * 8f + 24f)), 
                };
            return new HoverUIData(this, this.TileEntityWorldCenter(), data.ToArray());
        }

        public void DropUsedItems()
        {
            foreach (var item in itemsConsumed)
            {
                Item.NewItem(new EntitySource_TileBreak(Position.X, Position.Y), Position.X * 16, Position.Y * 16, Width * 16, Height * 16, item.Key, item.Value);
            }
        }
        public int GetShiftingItemAtTier(int tier)
        {
            if (stageMaterials[tier].items.Length == 1)
                return stageMaterials[tier].items[0];

            return stageMaterials[tier].items[Main.GameUpdateCount / 75 % stageMaterials[tier].items.Length];
    }
        public sealed override void SaveExtraData(TagCompound tag)
        {
            tag[nameof(stage)] = stage;
            tag.Add("itemsConsumed_Keys", itemsConsumed.Keys.ToList());
            tag.Add("itemsConsumed_Values", itemsConsumed.Values.ToList());
            SaveExtraExtraData(tag);
        }
        public virtual void SaveExtraExtraData(TagCompound tag) { }

        public sealed override void LoadExtraData(TagCompound tag)
        {
            stage = tag.GetInt(nameof(stage));
            List<int> itemKeys = (List<int>)tag.GetList<int>("itemsConsumed_Keys");
            if (itemKeys.Count > 0)
            {
                List<int> itemValues = (List<int>)tag.GetList<int>("itemsConsumed_Values");
                itemsConsumed = itemKeys.Zip(itemValues, (k, v) => new { Key = k, Value = v }).ToDictionary(x => x.Key, x => x.Value);
            }
            LoadExtraExtraData(tag);
        }
        public virtual void LoadExtraExtraData(TagCompound tag) { }
        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            Point origin = GetTileOrigin(i, j);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, origin.X, origin.Y, Width, Height);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, origin.X, origin.Y, Type);
            }
            int placedEntity = Place(origin.X, origin.Y);
            TileEntitySystem.shouldUpdateStability = true;
            (ModTileEntity.ByID[placedEntity] as AssemblableTileEntity).ConsumeItemsOnPlace();
            return placedEntity;
        }
        private void ConsumeItemsOnPlace()
        {
            Item item = Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem];
            if (item.ModItem is BlueprintCase blueprintCase)
            {
                BlueprintData selectedData = blueprintCase.selectedData;
                if (selectedData is not null)
                {
                    AssemblableTileEntity entity = selectedData.tileEntity;
                    int[] typesToConsume = entity.stageMaterials[0].items;
                    Dictionary<int, int> slotsToPullFrom = new Dictionary<int, int>();
                    int amountLeft = entity.stageMaterials[0].stack;
                    for (int i = 0; i < 58; i++)
                    {
                        if (typesToConsume.Contains(Main.LocalPlayer.inventory[i].type))
                        {
                            slotsToPullFrom.Add(i, Math.Min(amountLeft, Main.LocalPlayer.inventory[i].stack)); 
                            amountLeft -= Math.Clamp(amountLeft, 0, Main.LocalPlayer.inventory[i].stack);
                            if (amountLeft == 0)
                            {
                                foreach (var slot in slotsToPullFrom)
                                {
                                    Item consumedItem = Main.LocalPlayer.inventory[slot.Key];
                                    if (!itemsConsumed.ContainsKey(consumedItem.type))
                                        itemsConsumed[consumedItem.type] = slot.Value;
                                    else
                                        itemsConsumed[consumedItem.type] += slot.Value;

                                    consumedItem.stack -= slotsToPullFrom[slot.Key];
                                    if (consumedItem.stack <= 0)
                                        consumedItem.TurnToAir();
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }
        public void Draw(SpriteBatch spriteBatch, int stage, bool preview = false)
        {
            Rectangle frame = new Rectangle(stage * (Width * 16 + 2) * Math.Sign(stage), 0, Width * 16, Height * 16);
            spriteBatch.Draw(Texture, Position.ToVector2() * 16 - Main.screenPosition + TileDrawingZero, frame, Lighting.GetColor(Position.ToPoint()), 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }

        public void DrawPreview(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, NextStage, true);
        }
    }
    public class AssemblyHoverElement : HoverUIElement
    {
        public AssemblyHoverElement(string name, Vector2 targetPosition) : base(name)
        {
            this.targetPosition = targetPosition;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (parent.entity is AssemblableTileEntity entity)
            {
                int stage = entity.NextStage;
                if (stage >= entity.StageCount)
                    stage = entity.StageCount - 1;

                DynamicSpriteFont font = FontAssets.MouseText.Value;
                string str = $"x{entity.stageMaterials[stage].stack} required";
                float scale = Math.Clamp(timerModifier + 0.5f, 0.5f, 1);
                //int item = entity.GetShiftingItemAtTier(stage);
                int item = entity.GetShiftingItemAtTier(stage);
                Vector2 stringSize = font.MeasureString(str);
                Vector2 itemSize = GetItemTexture(item).Size();
                float strWidth = stringSize.X * scale;
                float itemWidth = itemSize.X * scale;
                Vector2 itemOffset = new Vector2(1f, -(stringSize.Y - itemSize.Y) / 2f - stringSize.Y / 4f);

                Utils.DrawBorderStringFourWay(Main.spriteBatch, font, str, realDrawPosition.X, realDrawPosition.Y, Color.White * timerModifier, Color.Black * timerModifier, Vector2.UnitX * ((strWidth - itemWidth) / 2f - itemOffset.X), scale);
                RadianceDrawing.DrawHoverableItem(Main.spriteBatch, item, realDrawPosition - itemOffset - Vector2.UnitX * strWidth / 2f, 1, Color.White * timerModifier, scale);
            }
        }
    }
}