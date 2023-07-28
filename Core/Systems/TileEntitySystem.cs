using Radiance.Content.Tiles;
using System.Collections.Generic;

namespace Radiance.Core.Systems
{
    public class TileEntitySystem : ModSystem
    {
        public static TileEntitySystem Instance;

        public TileEntitySystem() => Instance = this;
        
        public static List<ImprovedTileEntity> orderedEntities;
        public Dictionary<ModTileEntity, Point> TileEntitiesToPlace;
        public static bool shouldUpdateStability = true;
        public override void Load()
        {
            TileEntitiesToPlace = new Dictionary<ModTileEntity, Point>();
        }

        public override void Unload()
        {
            TileEntitiesToPlace = null;
            Instance = null;
        }
        public override void ClearWorld()
        {
            shouldUpdateStability = true;
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
        public static List<ImprovedTileEntity> TileEntitySearchSoft(Point start, int range) => orderedEntities.Where(x =>
                   Math.Abs(start.X - x.Position.X) <= range &&
                   Math.Abs(start.Y - x.Position.Y) <= range).ToList();
        public static List<ImprovedTileEntity> TileEntitySearchHard(Point start, int range) => orderedEntities.Where(x =>
                   Math.Abs(start.X - x.Position.X) <= range &&
                   Math.Abs(start.Y - x.Position.Y) <= range &&
                   Math.Abs(start.X - (x.Position.X + x.Width - 1)) <= range &&
                   Math.Abs(start.Y - (x.Position.Y + x.Height - 1)) <= range).ToList();
        public override void PreUpdateWorld()
        {
            orderedEntities = TileEntity.ByID.Values.Where(x => x is ImprovedTileEntity).OrderByDescending(x => (x as ImprovedTileEntity).updateOrder).Cast<ImprovedTileEntity>().ToList();
            if (shouldUpdateStability)
                ResetStability();
            
            foreach (var item in orderedEntities)
            {
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
        public static void ResetStability()
        {
            foreach (var te in orderedEntities)
            {
                te.stability = 0;
                if(shouldUpdateStability)
                    te.SetInitialStability();
            }
            foreach (StabilizerTileEntity stabilizer in orderedEntities.Where(x => x is StabilizerTileEntity))
            {
                if (stabilizer.StabilizerRange > 0 && stabilizer.StabilityLevel > 0)
                {
                    var entitiesInRange = TileEntitySearchHard(stabilizer.Position.ToPoint(), stabilizer.StabilizerRange);

                    var entitiesToStabilize = entitiesInRange.Where(x => x.usesStability && x.idealStability > 0);
                    var stabilizersInRange = entitiesInRange.Where(x => x is StabilizerTileEntity sb && sb.StabilityLevel > 0 && x != stabilizer);

                    float realStabilityLevel = stabilizer.StabilityLevel;
                    foreach (StabilizerTileEntity ste in stabilizersInRange)
                    {
                        float distance = Vector2.Distance(stabilizer.Position.ToVector2(), ste.Position.ToVector2()) / Vector2.Distance(stabilizer.Position.ToVector2(), stabilizer.Position.ToVector2() + Vector2.One * stabilizer.StabilizerRange);
                        realStabilityLevel *= Lerp(0.67f, 1, distance);
                    }

                    if (realStabilityLevel > 0)
                    {
                        foreach (ImprovedTileEntity e in entitiesToStabilize)
                        {
                            e.stability += realStabilityLevel / entitiesToStabilize.Count();
                        }
                    }
                }
            }
            shouldUpdateStability = false;
        }
    }
}