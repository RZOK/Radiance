using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace Radiance.Core
{
    public static class StabilityHelper
    {
        public static void ResetStabilizers()
        {
            var entities = TileEntity.ByID.Values.Where(x => x is ImprovedTileEntity entity).OrderBy(x => x is StabilizerTileEntity).ToList();
            foreach (ImprovedTileEntity entity in entities)
            {
                if (entity.usesStability && entity.stability > 0)
                {
                    entity.stability = 0;
                    continue;
                }
                if (entity is StabilizerTileEntity stabilizer)
                {
                    if (stabilizer.StabilizerRange > 0 && stabilizer.StabilityLevel > 0)
                    {
                        var entitiesInRange = entities.Where(x =>
                            Math.Abs(entity.Position.X - x.Position.X) <= stabilizer.StabilizerRange &&
                            Math.Abs(entity.Position.Y - x.Position.Y) <= stabilizer.StabilizerRange &&
                            Math.Abs(entity.Position.X - x.Position.X) + ((ImprovedTileEntity)x).Width - 1 <= stabilizer.StabilizerRange &&
                            Math.Abs(entity.Position.Y - x.Position.Y) + ((ImprovedTileEntity)x).Height - 1 <= stabilizer.StabilizerRange);

                        var entitiesToStabilize = entitiesInRange.Where(x => ((ImprovedTileEntity)x).usesStability);
                        int stabilizersInRange = entitiesInRange.Where(x => x is StabilizerTileEntity sb && sb.StabilityLevel > 0).Count() - 1;
                        foreach (ImprovedTileEntity e in entitiesToStabilize)
                        {
                            float realStabilityLevel = (float)stabilizer.StabilityLevel;
                            for (int i = 0; i < stabilizersInRange; i++)
                            {
                                realStabilityLevel *= 0.67f;
                            }
                            if (realStabilityLevel > 0)
                                e.stability += (int)(realStabilityLevel / entitiesToStabilize.Count());
                        }
                    }
                }
            }
        }
    }
}