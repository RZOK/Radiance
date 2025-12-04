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
            incomplete = UnlockCondition.UnlockedByDefault;
            unlock = UnlockCondition.UnlockedByDefault;
            category = EntryCategory.Influencing;
            icon = ModContent.ItemType<GlowstalkItem>();
            visible = EntryVisibility.Visible;

            pages = [
                new TextPage(),
                new RecipePage(new List<(List<(int item, int stack)> items, int station, int result, int resultStack, string extras)>
                {
                    (new List<(int, int)>()
                    {
                        (ModContent.ItemType<GlowstalkItem>(), 1),
                        (ItemID.PotSuspended, 1)
                    },
                    ItemID.None,
                    ModContent.ItemType<HangingGlowstalkItem>(),
                    1,
                    string.Empty)  
                })
            ];
        }
    }
}