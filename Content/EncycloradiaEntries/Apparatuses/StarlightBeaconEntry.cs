using Radiance.Content.Tiles.StarlightBeacon;
using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries.Apparatuses
{
    public class StarlightBeaconEntry : EncycloradiaEntry
    {
        public StarlightBeaconEntry()
        {
            fastNavInput = "RUDD";
            incomplete = UnlockCondition.unlockedByDefault;
            unlock = UnlockCondition.debugCondition;
            category = EntryCategory.Apparatuses;
            icon = ModContent.ItemType<StarlightBeaconItem>();
            visible = EntryVisibility.Visible;
            pages = [new TextPage()];
        }
    }
}