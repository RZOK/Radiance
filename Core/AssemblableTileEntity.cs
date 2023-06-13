using Radiance.Core.Systems;
using System.Collections.Generic;

namespace Radiance.Core
{
    public abstract class AssemblableTileEntity : ImprovedTileEntity
    {
        public int CurrentStage = 0;
        public int StageCount;
        public Texture2D Texture;
        public List<(int, int)> StageMaterials;
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
            int item = StageMaterials[CurrentStage].Item1;
            Dictionary<int, int> slotsToPullFrom = new Dictionary<int, int>();
            int amountLeft = StageMaterials[CurrentStage].Item2;
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
        public void DrawHoverUI() => Main.LocalPlayer.SetCursorItem(StageMaterials[CurrentStage].Item1);
        protected override HoverUIData ManageHoverUI()
        {
            string str = "x" + StageMaterials[CurrentStage].Item2.ToString() + " required";
            List<HoverUIElement> data = new List<HoverUIElement>()
                {
                    new TextUIElement("MaterialCount", str, Color.White, -Vector2.UnitY * 40),
                    new ItemUIElement("MaterialIcon", StageMaterials[CurrentStage].Item1, new Vector2((-FontAssets.MouseText.Value.MeasureString(str).X - Item.GetDrawHitbox(StageMaterials[CurrentStage].Item1, null).Width) / 2 + 4, -42))
                };
            return new HoverUIData(this, this.TileEntityWorldCenter(), data.ToArray());
        }

        public void DropUsedItems()
        {
            for (int i = 0; i < CurrentStage; i++)
            {
                Item.NewItem(new EntitySource_TileBreak(Position.X, Position.Y), Position.X * 16, Position.Y * 16, 32, 16, StageMaterials[i].Item1, StageMaterials[i].Item2);
            }
        }

        public sealed override void SaveData(TagCompound tag)
        {
            if (CurrentStage > 0)
                tag["CurrentStage"] = CurrentStage;
            SaveExtraData(tag);
        }
        public virtual void SaveExtraData(TagCompound tag) { }

        public sealed override void LoadData(TagCompound tag)
        {
            CurrentStage = tag.GetInt("CurrentStage");
            LoadExtraData(tag);
        }
        public virtual void LoadExtraData(TagCompound tag) { }

        public void Draw(SpriteBatch spriteBatch, int stage, bool preview = false)
        {
            Rectangle frame = new Rectangle(stage * (Width * 16 + 2) * Math.Sign(stage), 0, Width * 16, Height * 16);
            spriteBatch.Draw(Texture, Position.ToVector2() * 16 - Main.screenPosition + RadianceUtils.tileDrawingZero, frame, Lighting.GetColor(Position.ToPoint()), 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }

        public void DrawPreview(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, CurrentStage + 1, true);
        }
    }
}