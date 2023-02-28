using Microsoft.Xna.Framework;
using Radiance.Utilities;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Radiance.Core
{
    public interface IInventory
    {
        Item[] inventory { get; set; } 
        byte[] inputtableSlots { get; }
        byte[] outputtableSlots { get; }
    }
}