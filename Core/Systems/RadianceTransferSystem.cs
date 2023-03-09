using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Radiance.Core.Systems
{
    public class RadianceTransferSystem : ModSystem
    {
        public static List<RadianceRay> rays;
        public static RadianceTransferSystem Instance;

        public override void Load()
        {
            Instance = this;
        }
        public override void Unload()
        {
            Instance = null;
        }
        public override void OnWorldLoad()
        {
            rays = new List<RadianceRay>();
        }
        public override void OnWorldUnload()
        {
            rays = null;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            if(rays.Count > 0)
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
                for (int i = 0; i < rays.Count; i++)
                {
                    RadianceRay ray = rays[i];
                    if (ray.active)
                        ray.Update();
                    else
                        rays.Remove(ray);
                }
            }
        }
    }
}