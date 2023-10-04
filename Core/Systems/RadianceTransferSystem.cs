using System.Collections.Generic;

namespace Radiance.Core.Systems
{
    public class RadianceTransferSystem : ModSystem
    {
        public static List<RadianceRay> rays;
        public static RadianceTransferSystem Instance;
        public override void Load()
        {
            rays = new List<RadianceRay>();
            Instance = this;
        }

        public override void Unload()
        {
            Instance = null;
        }

        public override void ClearWorld()
        {
            rays.Clear();
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
            if (rays != null && rays.Count > 0)
            {
                List<RadianceRay> raysToRemove = new List<RadianceRay>();
                foreach (RadianceRay ray in rays)
                {
                    if (ray.active)
                        ray.Update();
                    else
                        raysToRemove.Add(ray);
                }
                rays.RemoveAll(raysToRemove.Contains);
            }
        }
    }
}