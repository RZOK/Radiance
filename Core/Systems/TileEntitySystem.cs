using Radiance.Content.Tiles;

namespace Radiance.Core.Systems
{
    public class TileEntitySystem : ModSystem
    {
        public static TileEntitySystem Instance;

        public TileEntitySystem() => Instance = this;
        
        public static List<ImprovedTileEntity> orderedEntities;
        public static Dictionary<ModTileEntity, Point> TileEntitiesToPlace;
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
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, entity.Value.X, entity.Value.Y, entity.Key.type);

                entity.Key.Place(entity.Value.X, entity.Value.Y);
            }
            TileEntitiesToPlace.Clear();
        }

        /// <summary>
        /// Searches for each ImprovedTileEntity with a maximum distance of <paramref name="range"/> from <paramref name="start"/>, only caring about the top-left tile of the multitile. 
        /// <para />
        /// Not often used, as most tile entities with an AoE should hard search instead.
        /// </summary>
        /// <param name="start">The tile coordinates to search from.</param>
        /// <param name="range">The distance from 'start' that a tile can be.</param>
        /// <returns>A list of each ImprovedTileEntity in range.</returns>
        public static List<ImprovedTileEntity> TileEntitySearchSoft(Point start, int range) => orderedEntities.Where(x =>
                   Math.Abs(start.X - x.Position.X) <= range &&
                   Math.Abs(start.Y - x.Position.Y) <= range).ToList();

        /// <summary>
        /// Searches for each ImprovedTileEntity with a maximum distance of <paramref name="range"/> from <paramref name="start"/>, making sure each corner is within the bounds as well. 
        /// <para />
        /// Example: <see cref="CinderCrucibleTileEntity.OrderedUpdate"/>
        /// </summary>
        /// <param name="start">The tile coordinates to search from.</param>
        /// <param name="range">The distance from 'start' that a tile can be.</param>
        /// <returns>A list of each ImprovedTileEntity in range.</returns>
        public static List<ImprovedTileEntity> TileEntitySearchHard(Point start, int range) => orderedEntities.Where(x =>
                   Math.Abs(start.X - x.Position.X) <= range &&
                   Math.Abs(start.Y - x.Position.Y) <= range &&
                   Math.Abs(start.X - (x.Position.X + x.Width - 1)) <= range &&
                   Math.Abs(start.Y - (x.Position.Y + x.Height - 1)) <= range).ToList();
        public override void PreUpdateWorld()
        {
            orderedEntities = TileEntity.ByID.Values.Where(x => x is ImprovedTileEntity).OrderByDescending(x => (x as ImprovedTileEntity).updateOrder).Cast<ImprovedTileEntity>().ToList();

            // reset stability of all tile entities. this is true by default and on world clear
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
                // set the ideal stability of things that set it based on unusual factors. pedestals use this so they can set their ideals instantly on world load based on the placed item
                if(shouldUpdateStability)
                    te.SetIdealStability();
            }
            foreach (StabilizerTileEntity stabilizer in orderedEntities.Where(x => x is StabilizerTileEntity))
            {
                if (stabilizer.StabilizerRange > 0 && stabilizer.StabilityLevel > 0)
                {
                    var entitiesInRange = TileEntitySearchHard(stabilizer.Position.ToPoint(), stabilizer.StabilizerRange);

                    var entitiesToStabilize = entitiesInRange.Where(x => x.idealStability > 0);
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