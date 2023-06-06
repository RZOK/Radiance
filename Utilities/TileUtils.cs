﻿using Microsoft.Xna.Framework;
using Radiance.Core;
using Terraria;
using Terraria.DataStructures;

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

		public static Vector2 MultitileWorldPosition(int i, int j) => GetTileOrigin(i, j).ToVector2() * 16; 
		public static Vector2 TileEntityWorldCenter(this ImprovedTileEntity entity) => (new Vector2(entity.Position.X, entity.Position.Y) + (new Vector2(entity.Width, entity.Height) / 2)) * 16;
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
            if (TryGetTileEntityAs(i, j, out ImprovedTileEntity entity))
            {
                if (Framing.GetTileSafely(i, j).TileFrameY == 0 && Framing.GetTileSafely(i, j).TileFrameX == 0)
                    entity.enabled = !entity.enabled;
            }
        }
	}
}