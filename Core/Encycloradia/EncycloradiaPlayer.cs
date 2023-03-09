using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Radiance.Core.Encycloradia
{
    public class EncycloradiaPlayer : ModPlayer
    {
        public List<string> unreadEntires = new List<string>();
        public override void Load()
        {
            unreadEntires = new List<string>();
        }
        public override void Unload()
        {
            unreadEntires = null;
        }
        public override void SaveData(TagCompound tag)
        {
            if (unreadEntires.Count > 0)
                tag[nameof(unreadEntires)] = unreadEntires;
        }
        public override void LoadData(TagCompound tag)
        {
            unreadEntires = (List<string>)tag.GetList<string>(nameof(unreadEntires));
        }
    }
}
