using Microsoft.Xna.Framework;
using Radiance.Content.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Radiance.Core.Systems
{
    public class RadianceTransferSystem : ModSystem
    {
        public List<(int, int)> Coords = new List<(int, int)>();
        public List<RadianceRay> rayList;
        public List<Vector2> inputs = new List<Vector2>();
        public List<Vector2> outputs = new List<Vector2>();
        public static RadianceTransferSystem Instance;
        public override void Load()
        {
            Instance = this;
            rayList = new List<RadianceRay>();
        }
        public override void OnWorldUnload()
        {
            Array.Clear(Radiance.radianceRay);
            Coords.Clear();
        }
        public void ReconstructRays()
        {
            for (int i = 0; i < rayList.Count; i++)
            {
                Radiance.radianceRay[i] = rayList[i];
            }
        }
        public override void SaveWorldData(TagCompound tag)
        {
            tag[nameof(rayList)] = rayList;
        }
        public override void LoadWorldData(TagCompound tag)
        {
            rayList = tag.Get<List<RadianceRay>>(nameof(rayList));
            ReconstructRays();
        }
        public override void PostUpdateTime()
        {
            for (int i = 0; i < Radiance.maxRays; i++)
            {
                if (Radiance.radianceRay[i] != null && Radiance.radianceRay[i].active)
                {
                    Radiance.radianceRay[i].Update();
                }
            }
        }
    }
}
