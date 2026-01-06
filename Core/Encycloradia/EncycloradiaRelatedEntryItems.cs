using Radiance.Content.EncycloradiaEntries.Apparatuses;
using static Radiance.Core.RadianceSets;

namespace Radiance.Core.Encycloradia
{
    internal class EncycloradiaRelatedEntrySystem : ModSystem
    {
        public override void PostSetupContent()
        {
            EncycloradiaRelatedEntry[ItemID.SoulofLight] = nameof(StarlightBeaconEntry);
        }
    }

    internal class EncycloradiaRelatedEntryGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public bool shouldLeadToRelevantEntry = false;

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (shouldLeadToRelevantEntry)
            {
                string text = $"[Click to go to entry [c/3FDEB1:{EncycloradiaSystem.FindEntry(EncycloradiaRelatedEntry[item.type]).GetLocalizedName()}]]";
                tooltips.Add(new TooltipLine(Mod, "HasEntry", text) { OverrideColor = CommonColors.EncycloradiaContextEntryColor });
            }
        }
    }
}