using Radiance.Core.Interfaces;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Radiance.Core
{
    public class RadianceGlobalItem : GlobalItem
    {
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (item.ModItem is IInstrument)
            {
                TooltipLine line = new TooltipLine(item.ModItem.Mod, "InstrumentAlert", "Consumes Radiance from cells in your inventory");
                tooltips.Insert(tooltips.FindIndex(x => x.Name == "Tooltip0" && x.Mod == "Terraria"), line);
            }
        }
    }
}
