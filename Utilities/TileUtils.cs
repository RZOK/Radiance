using Microsoft.Xna.Framework;
using MonoMod.Utils;
using Radiance.Core;
using System.ComponentModel.DataAnnotations;
using Terraria;
using Terraria.DataStructures;
using Terraria.ObjectData;

namespace Radiance.Utilities
{
    public static partial class RadianceUtils
    {
        public static Point GetTileOrigin(this Point point) => GetTileOrigin(point.X, point.Y);
        public static Point GetTileOrigin(int i, int j)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			TileObjectData data = TileObjectData.GetTileData(tile);
            Point frame;
			if (data != null)
			{
				Point reduce = new Point(0, 0);
				if (data.StyleHorizontal)
					reduce.X = tile.TileFrameX / 18 / data.Width;
				else
					reduce.Y = tile.TileFrameY / 18 / data.Height;

				frame = new(tile.TileFrameX / 18 - reduce.X * data.Width, tile.TileFrameY / 18 - reduce.Y * data.Height);
			}
			else
				frame = new(tile.TileFrameX / 18, tile.TileFrameY / 18);

            Point coord = new(i, j);
			return coord - frame;
		}

		public static Vector2 MultitileOriginWorldPosition(int i, int j) => GetTileOrigin(i, j).ToVector2() * 16;
		public static Vector2 MultitileWorldCenter(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            TileObjectData data = TileObjectData.GetTileData(tile);
            Point origin = GetTileOrigin(i, j);
			return (origin.ToVector2() + new Vector2(data.Width, data.Height) / 2) * 16;
        }
		public static Vector2 TileEntityWorldCenter(this ImprovedTileEntity entity) => (new Vector2(entity.Position.X, entity.Position.Y) + (new Vector2(entity.Width, entity.Height) / 2)) * 16;
        public static bool TryGetTileEntityAs<T>(int i, int j, out T entity) where T : TileEntity
		{
            Point origin = GetTileOrigin(i, j);
			if (TileEntity.ByPosition.TryGetValue(new Point16(origin.X, origin.Y), out TileEntity existing) && existing is T existingAsT)
			{
				entity = existingAsT;
				return true;
			}

			entity = null;
			return false;
		}
		public static void ToggleTileEntity(int i, int j)
		{
            if (TryGetTileEntityAs(i, j, out ImprovedTileEntity entity))
            {
                if (Framing.GetTileSafely(i, j).TileFrameY == 0 && Framing.GetTileSafely(i, j).TileFrameX == 0)
                    entity.enabled = !entity.enabled;
            }
        }
	}
}