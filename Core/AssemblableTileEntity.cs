using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Radiance.Core
{
    public abstract class AssemblableTileEntity : ModTileEntity
    {
        public int CurrentStage = 0;
        public readonly int ParentTile;
        public int StageCount;
        public Texture2D Texture;
        public Dictionary<int, int> StageMaterials;
        public ushort TileToTurnInto;
        public ModTileEntity EntityToTurnInto;
        public int Width => TileObjectData.GetTileData(ParentTile, 0).Width;
        public int Height => TileObjectData.GetTileData(ParentTile, 0).Height;

        public AssemblableTileEntity(int parentTile, ushort tileToTurnInto, ModTileEntity entityToTurnInto, int stageCount, Texture2D texture, Dictionary<int, int> stageMaterials)
        {
            ParentTile = parentTile;
            StageCount = stageCount;
            Texture = texture;
            StageMaterials = stageMaterials;
            EntityToTurnInto = entityToTurnInto;
            TileToTurnInto = tileToTurnInto;
        }

        public void ConsumeMaterials(Player player, int stage)
        {
            int item = StageMaterials.Keys.ToList()[stage];
            for (int i = 0; i < 58; i++)
            {
                int amountLeft = StageMaterials[item];
                Dictionary<int, int> slotsToPullFrom = new Dictionary<int, int>();
                if (player.inventory[i].type == item)
                {
                    slotsToPullFrom.Add(i, Math.Min(amountLeft, player.inventory[i].stack));
                    amountLeft -= Math.Clamp(amountLeft, 0, player.inventory[i].stack);
                    if (amountLeft == 0)
                    {
                        foreach (int slot in slotsToPullFrom.Keys)
                        {
                            player.inventory[slot].stack -= slotsToPullFrom[slot];
                            if (player.inventory[slot].stack == 0)
                                player.inventory[slot].TurnToAir();
                        }
                        CurrentStage++;
                        return;
                    }
                }
            }
        }
        public override void Update()
        {
            if(CurrentStage == StageCount - 1)
            {
                Kill(Position.X, Position.Y);
                ModContent.Find<ModTileEntity>(nameof(EntityToTurnInto)).Place(Position.X, Position.Y);
            }
        }
        public override void SaveData(TagCompound tag)
        {
            if (CurrentStage > 0)
                tag["CurrentStage"] = CurrentStage;
        }
        public override void LoadData(TagCompound tag)
        {
            CurrentStage = tag.GetInt("CurrentStage");
        }
        public void DrawHoverUI()
        {
        }

        public void Draw(SpriteBatch spriteBatch, int stage, bool preview = false)
        {
            Rectangle frame = new Rectangle(stage * Width + 2 * Math.Sign(stage), 0, Width * 16, Height * 16);
            spriteBatch.Draw(Texture, Position.ToVector2() * 16, frame, Lighting.GetColor(Position.ToPoint()), 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }
        public void DrawPreview(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, CurrentStage + 1, true);
        }
    }
}