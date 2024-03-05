using Radiance.Content.Tiles;
using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries.Influencing
{
    public class GlowstalkEntry : EncycloradiaEntry
    {
        public GlowstalkEntry()
        {
            fastNavInput = "UDRR";
            incomplete = UnlockCondition.unlockedByDefault;
            unlock = UnlockCondition.unlockedByDefault;
            category = EntryCategory.Influencing;
            icon = ModContent.ItemType<GlowstalkItem>();
            visible = EntryVisibility.Visible;

            pages = [
                new TextPage(), 
                new RecipePage()
                {
                    items = new Dictionary<int, int>()
                    {
                        { ModContent.ItemType<GlowstalkItem>(), 1 },
                        { ItemID.PotSuspended, 1 }
                    },
                    station = GetItem(ItemID.None),
                    result = GetItem(ModContent.ItemType<HangingGlowstalkItem>()),
                }
            ];
        }
    }
}