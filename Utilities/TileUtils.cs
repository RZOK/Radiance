using Terraria.ObjectData;

namespace Radiance.Utilities
{
    public static partial class RadianceUtils
    {
        public static Point GetTileOrigin(this Point point) => GetTileOrigin(point.X, point.Y);
        public static Point GetTileOrigin(int x, int y)
        {
            Tile tile = Main.tile[x, y];

            int frameX = 0;
            int frameY = 0;

            if (tile.HasTile)
            {
                int style = 0;
                int alt = 0;
                TileObjectData.GetTileInfo(tile, ref style, ref alt);
                TileObjectData data = TileObjectData.GetTileData(tile.TileType, style, alt);

                if (data != null)
                {
                    int size = 16 + data.CoordinatePadding;

                    frameX = tile.TileFrameX % (size * data.Width) / size;
                    frameY = tile.TileFrameY % (size * data.Height) / size;
                }
            }

            return new Point(x - frameX, y - frameY);
        }


        public static Vector2 MultitileOriginWorldPosition(int i, int j) => GetTileOrigin(i, j).ToVector2() * 16;
		public static Vector2 MultitileWorldCenter(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            TileObjectData data = TileObjectData.GetTileData(tile);
            Point origin = GetTileOrigin(i, j);
			if (data != null)
				return (origin.ToVector2() + new Vector2(data.Width, data.Height) / 2) * 16;
			return Vector2.One * -1f;
			
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