using System.Collections.Generic;

namespace Radiance.Core
{
    public class RadianceGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public int formationPickupTimer = 0;
        public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
        {
            if (formationPickupTimer > 0)
                formationPickupTimer--;
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (item.ModItem is IInstrument)
            {
                TooltipLine line = new TooltipLine(item.ModItem.Mod, "InstrumentAlert", "Consumes Radiance from cells in your inventory");
                int index = tooltips.FindIndex(x => x.Name == "Tooltip0" && x.Mod == "Terraria");
                if(index > -1) 
                    tooltips.Insert(index, line);
            }
        }
    }
}
