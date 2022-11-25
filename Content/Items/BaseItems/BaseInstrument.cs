using System.Collections.Generic;
using Terraria.ModLoader;

namespace Radiance.Content.Items.BaseItems
{
    public abstract class BaseInstrument : ModItem
    {
        public abstract float CosumeAmount { get; set; }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            foreach (TooltipLine tooltip in tooltips)
            {
                if (tooltip.Name == "Tooltip0")
                {
                    tooltip.Text = "Consumes Radiance from cells in your inventory"; ;
                }
            }
        }
    }
}