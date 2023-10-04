using Radiance.Core.Systems;

namespace Radiance.Core.TileEntities
{
    public abstract class AssemblableTileEntity : ImprovedTileEntity
    {
        public int CurrentStage = 0;
        public int StageCount;
        public Texture2D Texture;
        public List<(int item, int stack)> StageMaterials;
        public int TileToTurnInto;
        public ModTileEntity EntityToTurnInto;

        public AssemblableTileEntity(int parentTile, int tileToTurnInto, ModTileEntity entityToTurnInto, int stageCount, Texture2D texture, List<(int, int)> stageMaterials, float updateOrder = 1, bool usesStability = false) : base(parentTile, updateOrder, usesStability)
        {
            StageCount = stageCount;
            Texture = texture;
            StageMaterials = stageMaterials;
            EntityToTurnInto = entityToTurnInto;
            TileToTurnInto = tileToTurnInto;
        }

        public void ConsumeMaterials(Player player)
        {
            int item = StageMaterials[CurrentStage].item;
            Dictionary<int, int> slotsToPullFrom = new Dictionary<int, int>();
            int amountLeft = StageMaterials[CurrentStage].stack;
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
                TileEntitySystem.Instance.TileEntitiesToPlace.Add(EntityToTurnInto, Position.ToPoint());
                for (int i = 0; i < Width * Height; i++)
                {
                    Tile tile = Framing.GetTileSafely(Position.X + i % Width, Position.Y + i / Width);
                    tile.TileType = (ushort)TileToTurnInto;
                }
            }
        }
        public void DrawHoverUIAndMouseItem()
        {
            AddHoverUI();
            Main.LocalPlayer.SetCursorItem(StageMaterials[CurrentStage].item);
        }
        protected override HoverUIData ManageHoverUI()
        {
            string str = "x" + StageMaterials[CurrentStage].stack.ToString() + " required";
            List<HoverUIElement> data = new List<HoverUIElement>()
                {
                    new TextUIElement("MaterialCount", str, Color.White, -Vector2.UnitY * 40),
                    new ItemUIElement("MaterialIcon", StageMaterials[CurrentStage].item, new Vector2((-FontAssets.MouseText.Value.MeasureString(str).X - Item.GetDrawHitbox(StageMaterials[CurrentStage].item, null).Width) / 2 - 2, -42))
                };
            return new HoverUIData(this, this.TileEntityWorldCenter(), data.ToArray());
        }

        public void DropUsedItems()
        {
            for (int i = 0; i < CurrentStage; i++)
            {
                Item.NewItem(new EntitySource_TileBreak(Position.X, Position.Y), Position.X * 16, Position.Y * 16, Width * 16, Height * 16, StageMaterials[i].item, StageMaterials[i].stack);
            }
        }

        public sealed override void SaveExtraData(TagCompound tag)
        {
            tag[nameof(CurrentStage)] = CurrentStage;
            SaveExtraExtraData(tag);
        }
        public virtual void SaveExtraExtraData(TagCompound tag) { }

        public sealed override void LoadExtraData(TagCompound tag)
        {
            CurrentStage = tag.GetInt(nameof(CurrentStage));
            LoadExtraExtraData(tag);
        }
        public virtual void LoadExtraExtraData(TagCompound tag) { }

        public void Draw(SpriteBatch spriteBatch, int stage, bool preview = false)
        {
            Rectangle frame = new Rectangle(stage * (Width * 16 + 2) * Math.Sign(stage), 0, Width * 16, Height * 16);
            spriteBatch.Draw(Texture, Position.ToVector2() * 16 - Main.screenPosition + tileDrawingZero, frame, Lighting.GetColor(Position.ToPoint()), 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }

        public void DrawPreview(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, CurrentStage + 1, true);
        }
    }
}