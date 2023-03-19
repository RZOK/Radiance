using Microsoft.Xna.Framework;
using Radiance.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Radiance.Utilities
{
	public static partial class RadianceUtils
	{
		public static Point16 GetTileOrigin(int i, int j)
		{
			Tile tile = Framing.GetTileSafely(i, j);

			Point16 coord = new(i, j);
			Point16 frame = new(tile.TileFrameX / 18, tile.TileFrameY / 18);

			return coord - frame;
		}

		public static Vector2 MultitileCenterWorldCoords(int i, int j) => new Vector2( 
			i * 16,
			j * 16
			) -
			new Vector2(
			Main.tile[i, j].TileFrameX - (2 * Main.tile[i, j].TileFrameX / 18),
			Main.tile[i, j].TileFrameY - (2 * Main.tile[i, j].TileFrameY / 18)
			); //this doesn't actually center the coords it just gets the coords of the top left of a multitile
		public static Vector2 TileEntityWorldCenter(this RadianceUtilizingTileEntity entity) => (new Vector2((float)entity.Position.X, (float)entity.Position.Y) + (new Vector2((float)entity.width, (float)entity.height) / 2)) * 16;

		public static bool TryGetTileEntityAs<T>(int i, int j, out T entity) where T : TileEntity
		{
			Point16 origin = GetTileOrigin(i, j);
			if (TileEntity.ByPosition.TryGetValue(origin, out TileEntity existing) && existing is T existingAsT)
			{
				entity = existingAsT;
				return true;
			}

			entity = null;
			return false;
		}
		public static void ToggleTileEntity(int i, int j)
		{
            if (TryGetTileEntityAs(i, j, out RadianceUtilizingTileEntity entity))
            {
                if (Framing.GetTileSafely(i, j).TileFrameY == 0 && Framing.GetTileSafely(i, j).TileFrameX == 0)
                    entity.enabled = !entity.enabled;
            }
        }
	}
}