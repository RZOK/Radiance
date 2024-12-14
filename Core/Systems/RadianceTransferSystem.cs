using System.Diagnostics;

namespace Radiance.Core.Systems
{
    public class RadianceTransferSystem : ModSystem
    {
        public static List<RadianceRay> rays = new List<RadianceRay>();
        public static Dictionary<Point16, RadianceRay> byPosition = new Dictionary<Point16, RadianceRay>();

        public static bool shouldUpdateRays = true;
        public override void ClearWorld()
        {
            rays.Clear();
            byPosition.Clear();
            shouldUpdateRays = true;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            if (rays != null && rays.Count > 0)
                tag[nameof(rays)] = rays;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            rays = tag.Get<List<RadianceRay>>(nameof(rays));
            foreach (RadianceRay ray in rays)
            {
                byPosition[ray.startPos] = ray;
                byPosition[ray.endPos] = ray;
            }
        }
        public override void PostUpdateEverything()
        {
            if (rays is not null && rays.Count > 0)
            {
                if(shouldUpdateRays)
                {
                    foreach (RadianceRay ray in rays)
                    {
                        ray.TryGetIO(out ray.inputTE, out ray.outputTE, out _, out _);

                        Tile startPosTile = Framing.GetTileSafely(ray.startPos);
                        Tile endPosTile = Framing.GetTileSafely(ray.endPos);

                        int startPosTileType = startPosTile.HasTile ? startPosTile.TileType : 0;
                        int endPosTileType = endPosTile.HasTile ? endPosTile.TileType : 0;

                        if (ray.startPos == ray.endPos || !ray.PickedUp && ray.inputTE is null && ray.outputTE is null && !RadianceSets.RayAnchorTiles[startPosTileType] && !RadianceSets.RayAnchorTiles[endPosTileType])
                            ray.disappearing = true;

                        ray.interferred = ray.interferredVisual = ray.HasIntersection();
                    }
                    foreach (RadianceRay ray in rays)
                    {
                        if(!ray.PickedUp && !ray.disappearing && ray.inputTE is null && ray.outputTE is not null)
                            ray.SetInputToEndOfFixtureChain();
                    }
                    shouldUpdateRays = false;
                }

                List<RadianceRay> raysToRemove = new List<RadianceRay>();
                foreach (RadianceRay ray in rays)
                {
                    if (ray.active)
                        ray.Update();
                    else
                    {
                        raysToRemove.Add(ray);
                        byPosition.Remove(ray.startPos);
                        byPosition.Remove(ray.endPos);
                        shouldUpdateRays = true;
                    }
                }
                rays.RemoveAll(raysToRemove.Contains);
            }
        }
    }
}