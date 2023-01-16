using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;

namespace Radiance.Utilities
{
	static partial class RadianceUtils
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
	}
}