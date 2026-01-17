namespace Radiance.Core.Systems
{
    public class RadianceTransferSystem : ModSystem
    {

        public static bool shouldUpdateRays = true;

        public override void ClearWorld()
        {
            RadianceRay.rays.Clear();
            RadianceRay.byPosition.Clear();
            shouldUpdateRays = true;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            if (RadianceRay.rays != null && RadianceRay.rays.Count > 0)
                tag[nameof(RadianceRay.rays)] = RadianceRay.rays;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            RadianceRay.rays = tag.Get<List<RadianceRay>>(nameof(RadianceRay.rays));
            foreach (RadianceRay ray in RadianceRay.rays)
            {
                RadianceRay.byPosition[ray.startPos] = ray;
                RadianceRay.byPosition[ray.endPos] = ray;
            }
        }

        public override void PostUpdateEverything()
        {
            if (RadianceRay.rays is not null && RadianceRay.rays.Count > 0)
            {
                if (shouldUpdateRays)
                {
                    foreach (RadianceRay ray in RadianceRay.rays)
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
                    foreach (RadianceRay ray in RadianceRay.rays)
                    {
                        if (!ray.PickedUp && !ray.disappearing && ray.inputTE is null && ray.outputTE is not null)
                            ray.SetInputToEndOfFixtureChain();
                    }
                    shouldUpdateRays = false;
                }

                List<RadianceRay> raysToRemove = new List<RadianceRay>();
                foreach (RadianceRay ray in RadianceRay.rays)
                {
                    if (ray.active)
                        ray.Update();
                    else
                    {
                        raysToRemove.Add(ray);
                        RadianceRay.byPosition.Remove(ray.startPos);
                        RadianceRay.byPosition.Remove(ray.endPos);
                        shouldUpdateRays = true;
                    }
                }
                RadianceRay.rays.RemoveAll(raysToRemove.Contains);
            }
        }
    }
}