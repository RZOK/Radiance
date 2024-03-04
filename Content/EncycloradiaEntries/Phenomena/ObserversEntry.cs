using Radiance.Content.Items;
using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Content.EncycloradiaEntries.Phenomena
{
    public class ObserversEntry : EncycloradiaEntry
    {
        public ObserversEntry()
        {
            fastNavInput = "DDDD";
            incomplete = UnlockCondition.unlockedByDefault;
            unlock = UnlockCondition.unlockedByDefault;
            category = EntryCategory.Phenomena;
            icon = ModContent.ItemType<KnowledgeScroll>();
            visible = EntryVisibility.Visible;

            pages = [new TextPage()];
        }
    }
}