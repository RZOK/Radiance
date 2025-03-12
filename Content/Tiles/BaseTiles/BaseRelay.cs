using Radiance.Core.Systems;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles
{
    public abstract class BaseRelay : ModTile
    {
        public sealed override void SetStaticDefaults()
        {
            SetExtraStaticDefaults();
            RadianceSets.RayAnchorTiles[Type] = true;
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Hook_AfterPlacement, -1, 0, false);
            TileObjectData.addTile(Type);
        }
        public abstract void SetExtraStaticDefaults();
        public static int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            RadianceTransferSystem.shouldUpdateRays = true;
            return 0;
        }
        public sealed override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            PostDrawExtra(i, j, spriteBatch);

            Tile tile = Main.tile[i, j];
            Vector2 mainPosition = new Vector2(i, j) * 16f + TileDrawingZero;
            if (Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().canSeeRays)
            {
                if (TileIsOutput(tile))
                    RadianceDrawing.DrawRadianceIOSlot(InterfaceDrawer.RadianceIOIndicatorMode.Output, mainPosition + Vector2.One * 8f);
                else if (TileIsInput(tile))
                    RadianceDrawing.DrawRadianceIOSlot(InterfaceDrawer.RadianceIOIndicatorMode.Input, mainPosition + Vector2.One * 8f);
            }
        }
        public virtual void PostDrawExtra(int i, int j, SpriteBatch spriteBatch) { }
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            RadianceTransferSystem.shouldUpdateRays = true;
        }
        public abstract bool TileIsInput(Tile tile);
        public abstract bool TileIsOutput(Tile tile);
        public virtual bool Active(Tile tile) => true;

    }
}