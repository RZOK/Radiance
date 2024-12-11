namespace Radiance.Core.Systems
{
    public class RadianceTransferSystem : ModSystem
    {
        public static List<RadianceRay> rays;
        public static bool shouldUpdateRays = true;
        public override void Load()
        {
            rays = new List<RadianceRay>();
        }

        public override void ClearWorld()
        {
            rays.Clear();
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

                        Point startPosTileCoords = ray.startPos.ToTileCoordinates();
                        Point endPosTileCoords = ray.endPos.ToTileCoordinates();
                        Tile startPosTile = Framing.GetTileSafely(startPosTileCoords);
                        Tile endPosTile = Framing.GetTileSafely(endPosTileCoords);

                        int startPosTileType = startPosTile.HasTile ? startPosTile.TileType : 0;
                        int endPosTileType = endPosTile.HasTile ? endPosTile.TileType : 0;

                        if (!ray.PickedUp && ray.inputTE is null && ray.outputTE is null && !RadianceSets.RayAnchorTiles[startPosTileType] && !RadianceSets.RayAnchorTiles[endPosTileType])
                            ray.disappearing = true;

                        ray.interferred = ray.HasIntersection();
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
                        shouldUpdateRays = true;
                    }
                }
                rays.RemoveAll(raysToRemove.Contains);
            }
        }
    }
}