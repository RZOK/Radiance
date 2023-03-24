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
                foreach (TooltipLine tooltip in tooltips)
                {
                    if (tooltip.Name == "Tooltip0")
                    {
                        tooltip.Text = "Consumes Radiance from cells in your inventory";
                    }
                }
            }
        }
    }
}
