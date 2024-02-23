namespace Radiance.Core.Interfaces
{
    public interface IInstrument
    {
        public float consumeAmount { get; }
    }
    public class InstrumentGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (item.ModItem is IInstrument)
            {
                TooltipLine line = new TooltipLine(item.ModItem.Mod, "InstrumentAlert", "Consumes Radiance from cells in your inventory");
                int index = tooltips.FindIndex(x => x.Name == "Tooltip0" && x.Mod == "Terraria");
                if (index > -1)
                    tooltips.Insert(index, line);
            }
        }
    }
}