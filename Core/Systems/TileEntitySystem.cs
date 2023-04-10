using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Core.Systems
{
    public class TileEntitySystem : ModSystem
    {
        public static TileEntitySystem Instance;

        public TileEntitySystem()
        {
            Instance = this;
        }

        public Dictionary<ModTileEntity, Point> TileEntitiesToPlace;

        public override void Load()
        {
            TileEntitiesToPlace = new Dictionary<ModTileEntity, Point>();
        }

        public override void Unload()
        {
            TileEntitiesToPlace = null;
            Instance = null;
        }

        public override void PostUpdateEverything()
        {
            foreach (var entity in TileEntitiesToPlace)
            {
                ModTileEntity entityToPlace = ModContent.Find<ModTileEntity>(entity.Key.FullName);
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, entity.Value.X, entity.Value.Y, entityToPlace.type);

                entityToPlace.Place(entity.Value.X, entity.Value.Y);
            }
            TileEntitiesToPlace.Clear();
        }
    }
}