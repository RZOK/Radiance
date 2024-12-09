namespace Radiance.Core.Systems
{
    public class RadianceTransferSystem : ModSystem
    {
        public static List<RadianceRay> rays;
        public static bool shouldUpdateRays = true;
        public static bool checkRayIntersections = false;
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
                        if (!ray.PickedUp && ray.inputTE is null && ray.outputTE is null)
                            ray.disappearing = true;

                        ray.interferred = ray.HasIntersection();
                    }
                    checkRayIntersections = false;
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
                        checkRayIntersections = true;
                    }
                }
                rays.RemoveAll(raysToRemove.Contains);

                if (checkRayIntersections)
                {
                    foreach (RadianceRay ray in rays)
                    {
                        ray.interferred = ray.HasIntersection();
                    }
                }
                checkRayIntersections = false;
            }
        }
    }
}