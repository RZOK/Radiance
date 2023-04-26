//using System;
//using System.Linq;
//using Terraria;
//using Terraria.ID;
//using Terraria.DataStructures;
//using Microsoft.Xna.Framework;
//using Terraria.ModLoader;

//namespace Radiance.Core
//{
//    public static class StabilityHandler
//    { 
//        public static void ResetStabilizers()
//        {
//            var entities = TileEntity.ByID.Values.Where(x => x is ImprovedTileEntity entity).OrderBy(x => x is StabilizerTileEntity).ToList();
//            foreach (ImprovedTileEntity entity in entities)
//            {
//                if (entity.usesStability && entity.stability > 0)
//                {
//                    entity.stability = 0;
//                    continue;
//                }
//                if (entity is StabilizerTileEntity stabilizer)
//                {
//                    if (stabilizer.StabilizerRange > 0 && stabilizer.StabilityLevel > 0)
//                    {
//                         var entitiesInRange = entities.Where(x =>
//                            Math.Abs(stabilizer.Position.X - x.Position.X) <= stabilizer.StabilizerRange &&
//                            Math.Abs(stabilizer.Position.Y - x.Position.Y) <= stabilizer.StabilizerRange &&
//                            Math.Abs(stabilizer.Position.X - (x.Position.X + ((ImprovedTileEntity)x).Width - 1)) <= stabilizer.StabilizerRange &&
//                            Math.Abs(stabilizer.Position.Y - (x.Position.Y + ((ImprovedTileEntity)x).Height - 1)) <= stabilizer.StabilizerRange);

//                        var entitiesToStabilize = entitiesInRange.Where(x => ((ImprovedTileEntity)x).usesStability && ((ImprovedTileEntity)x).idealStability > 0);
//                        var stabilizersInRange = entitiesInRange.Where(x => x is StabilizerTileEntity sb && sb.StabilityLevel > 0);
//                        foreach (ImprovedTileEntity e in entitiesToStabilize)
//                        {
//                            float realStabilityLevel = stabilizer.StabilityLevel;
//                            foreach (StabilizerTileEntity ste in stabilizersInRange)
//                            {
//                                if (ste.Position == stabilizer.Position)
//                                    continue;

//                                float distance = Vector2.Distance(stabilizer.Position.ToVector2(), ste.Position.ToVector2()) / Vector2.Distance(stabilizer.Position.ToVector2(), stabilizer.Position.ToVector2() + Vector2.One * stabilizer.StabilizerRange);
//                                realStabilityLevel *= MathHelper.Lerp(0.67f, 1, distance);
//                            }
//                            if (realStabilityLevel > 0)
//                                e.stability += realStabilityLevel / entitiesToStabilize.Count();
//                        }
//                    }
//                }
//            }
//        }
//    }
//    public class StabilitySystem : ModSystem
//    {
//        public override void OnWorldLoad()
//        {
//            StabilityHandler.ResetStabilizers();
//        }
//    }
//}