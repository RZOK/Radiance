using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.UI;

namespace Radiance.Core.Systems
{
    internal class LayerSystem : ModSystem
    {
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            InterfaceDrawer.ModifyInterfaceLayers(layers);
        }
    }
}