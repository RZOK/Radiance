using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
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
        public static List<ImprovedTileEntity> orderedEntities;
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
        public static List<ImprovedTileEntity> TileEntitySearchSoft(ImprovedTileEntity entity, int range) => orderedEntities.Where(x =>
                   Math.Abs(entity.Position.X - x.Position.X) <= range &&
                   Math.Abs(entity.Position.Y - x.Position.Y) <= range).ToList();
        public static List<ImprovedTileEntity> TileEntitySearchHard(ImprovedTileEntity entity, int range) => orderedEntities.Where(x =>
                   Math.Abs(entity.Position.X - x.Position.X) <= range &&
                   Math.Abs(entity.Position.Y - x.Position.Y) <= range &&
                   Math.Abs(entity.Position.X - (x.Position.X + x.Width - 1)) <= range &&
                   Math.Abs(entity.Position.Y - (x.Position.Y + x.Height - 1)) <= range).ToList();
        public override void PreUpdateWorld()
        {
            orderedEntities = TileEntity.ByID.Values.Where(x => x is ImprovedTileEntity).OrderByDescending(x => (x as ImprovedTileEntity).updateOrder).Cast<ImprovedTileEntity>().ToList();
            foreach (var item in orderedEntities)
            {
                if (item.usesStability && item.idealStability > 0)
                    item.stability = 0;
                item.PreOrderedUpdate();
            }
        }
        public override void PostUpdateWorld()
        {
            foreach (var item in orderedEntities)
            {
                item.OrderedUpdate();
            }
        }
    }
}