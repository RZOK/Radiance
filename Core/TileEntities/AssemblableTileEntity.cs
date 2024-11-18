﻿using Radiance.Content.Items;
using Radiance.Core.Loaders;
using Radiance.Core.Systems;

namespace Radiance.Core.TileEntities
{
    public abstract class AssemblableTileEntity : ImprovedTileEntity
    {
        public int CurrentStage = 0;
        public int NextStage => CurrentStage + 1;
        public int StageCount => StageMaterials.Count;
        public Texture2D Texture;
        /// <summary>
        /// The FIRST item in the list will be consumed to place the assemblable tile.
        /// </summary>
        public List<(int item, int stack)> StageMaterials;
        public ImprovedTileEntity EntityToTurnInto;
        public Dictionary<int, int> itemsConsumed = new Dictionary<int, int> ();

        public AssemblableTileEntity(int parentTile, ImprovedTileEntity entityToTurnInto, Texture2D texture, List<(int, int)> stageMaterials, float updateOrder = 1, bool usesStability = false) : base(parentTile, updateOrder, usesStability)
        {
            Texture = texture;
            StageMaterials = stageMaterials;
            EntityToTurnInto = entityToTurnInto;
        }

        public void ConsumeMaterials(Player player)
        {
            int item = StageMaterials[NextStage].item;
            Dictionary<int, int> slotsToPullFrom = new Dictionary<int, int>();
            int amountLeft = StageMaterials[NextStage].stack;
            for (int i = 0; i < 58; i++)
            {
                if (player.inventory[i].type == item)
                {
                    slotsToPullFrom.Add(i, Math.Min(amountLeft, player.inventory[i].stack));
                    amountLeft -= Math.Clamp(amountLeft, 0, player.inventory[i].stack);
                    if (amountLeft == 0)
                    {
                        foreach (var slot in slotsToPullFrom)
                        {
                            if (!itemsConsumed.ContainsKey(slot.Key))
                                itemsConsumed[slot.Key] = slot.Value;
                            else
                                itemsConsumed[slot.Key] += slot.Value;

                            player.inventory[slot.Key].stack -= slotsToPullFrom[slot.Key];
                            if (player.inventory[slot.Key].stack <= 0)
                                player.inventory[slot.Key].TurnToAir();
                        }
                        CurrentStage++;
                        OnStageIncrease(CurrentStage);
                        return;
                    }
                }
            }
        }

        public virtual void OnStageIncrease(int stage) { }
        public override void OrderedUpdate()
        {
            if (CurrentStage == StageCount - 1)
            {
                Kill(Position.X, Position.Y);
                TileEntitySystem.TileEntitiesToPlace.Add(EntityToTurnInto, Position.ToPoint());
                for (int i = 0; i < Width * Height; i++)
                {
                    Tile tile = Framing.GetTileSafely(Position.X + i % Width, Position.Y + i / Width);
                    tile.TileType = (ushort)EntityToTurnInto.ParentTile;
                }
            }
        }
        public void DrawHoverUIAndMouseItem()
        {
            AddHoverUI();
            Main.LocalPlayer.SetCursorItem(StageMaterials[NextStage].item);
        }
        protected override HoverUIData ManageHoverUI()
        {
            string str = "x" + StageMaterials[NextStage].stack.ToString() + " required";
            List<HoverUIElement> data = new List<HoverUIElement>()
                {
                    new TextUIElement("MaterialCount", str, Color.White, -Vector2.UnitY * 40),
                    new ItemUIElement("MaterialIcon", StageMaterials[NextStage].item, new Vector2((-FontAssets.MouseText.Value.MeasureString(str).X - Item.GetDrawHitbox(StageMaterials[NextStage].item, null).Width) / 2 - 2, -42))
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

        public sealed override void SaveExtraData(TagCompound tag)
        {
            tag[nameof(CurrentStage)] = CurrentStage;
            tag.Add("itemsConsumed_Keys", itemsConsumed.Keys.ToList());
            tag.Add("itemsConsumed_Values", itemsConsumed.Values.ToList());
            SaveExtraExtraData(tag);
        }
        public virtual void SaveExtraExtraData(TagCompound tag) { }

        public sealed override void LoadExtraData(TagCompound tag)
        {
            CurrentStage = tag.GetInt(nameof(CurrentStage));
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
                    int typeToConsume = entity.StageMaterials[0].item;
                    Dictionary<int, int> slotsToPullFrom = new Dictionary<int, int>();
                    int amountLeft = entity.StageMaterials[0].stack;
                    for (int i = 0; i < 58; i++)
                    {
                        if (Main.LocalPlayer.inventory[i].type == typeToConsume)
                        {
                            slotsToPullFrom.Add(i, Math.Min(amountLeft, Main.LocalPlayer.inventory[i].stack)); 
                            amountLeft -= Math.Clamp(amountLeft, 0, Main.LocalPlayer.inventory[i].stack);
                            if (amountLeft == 0)
                            {
                                foreach (var slot in slotsToPullFrom)
                                {
                                    itemsConsumed[Main.LocalPlayer.inventory[slot.Key].type] = slot.Value;
                                    Main.LocalPlayer.inventory[slot.Key].stack -= slotsToPullFrom[slot.Key];
                                    if (Main.LocalPlayer.inventory[slot.Key].stack <= 0)
                                        Main.LocalPlayer.inventory[slot.Key].TurnToAir();
                                }
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
}