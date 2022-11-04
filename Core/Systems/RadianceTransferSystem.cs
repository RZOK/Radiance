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
        public Dictionary<Vector2, Vector2> connectionsDictionary = new Dictionary<Vector2, Vector2>();
        public List<Vector2> inputs = new List<Vector2>();
        public List<Vector2> outputs = new List<Vector2>();
        public static RadianceTransferSystem Instance;
        public override void Load()
        {
            Instance = this;
        }
        public override void OnWorldUnload()
        {
            for (int i = 0; i < Radiance.maxRadianceUtilizingTileEntities + 1; i++)
            {
                Radiance.radianceUtilizingTileEntityIndex[i] = default;
            }
        }
        public bool IsTileEntityReal(RadianceUtilizingTileEntity tileEntity)
        {
            return tileEntity != null && tileEntity.active;
        }
        public void AddInputOutput(Vector2 input, Vector2 output)
        {
            connectionsDictionary.Add(input, output);
        }
        public override void SaveWorldData(TagCompound tag)
        {
            tag["inputs"] = connectionsDictionary.Keys.ToList();
            tag["outputs"] = connectionsDictionary.Values.ToList();
        }

        public override void LoadWorldData(TagCompound tag)
        {
            var inputs = tag.GetList<Vector2>("inputs");
            var outputs = tag.GetList<Vector2>("outputs");
            connectionsDictionary = inputs.Zip(outputs, (k, v) => new { Key = k, Value = v }).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
