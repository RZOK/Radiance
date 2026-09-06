using Mono.Cecil.Cil;
using MonoMod.Cil;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Particles;
using Radiance.Core.Research;
using Radiance.Core.Systems;
using ReLogic.Graphics;
using System.Reflection;
using Terraria.Enums;
using Terraria.Graphics.CameraModifiers;
using Terraria.Localization;
using Terraria.ObjectData;
using Terraria.UI;
using static Radiance.Content.Tiles.TelescopeBoostInfo;

namespace Radiance.Content.Tiles
{
    public class ResearchTable : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = false;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Width = 4;
            HitSound = SoundID.Dig;
            DustType = -1;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Research Table");
            AddMapEntry(new Color(61, 50, 88), name);
            TileObjectData.addTile(Type);
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {

        }
        public override void NearbyEffects(int i, int j, bool closer)
        {
            Tile tile = Main.tile[i, j];
            if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
            {
              
            }
        }

        public override void MouseOver(int i, int j)
        {
            Main.LocalPlayer.SetCursorItem(ModContent.ItemType<ResearchTable_Item>());
        }

        public override bool RightClick(int i, int j)
        {
            Vector2 center = MultitileWorldCenter(i, j);
            SoundEngine.PlaySound(SoundID.MenuTick);
            ResearchPlayer researchTablePlayer = Main.LocalPlayer.GetModPlayer<ResearchPlayer>();
            if (researchTablePlayer.tablePosition != center)
            {
                Main.playerInventory = false;
                researchTablePlayer.tablePosition = center;
                ResearchUI.Instance.researchTable.tableVisible = true;
            }
            else
                ResearchUI.Instance.researchTable.CloseTable();

            return true;
        }
    }

    public class ResearchTable_Item : BaseTileItem
    {
        public ResearchTable_Item() : base("ResearchTable_Item", "Research Table", "Allows you to research items", nameof(ResearchTable), 1, Item.sellPrice(0, 0, 50, 0), ItemRarityID.Blue)
        {
        }
    }
}