using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Tiles;
using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries.Influencing
{
    public class RelayFixtureEntry : EncycloradiaEntry
    {
        public RelayFixtureEntry()
        {
            fastNavInput = "DRUD";
            incomplete = UnlockCondition.UnlockedByDefault;
            unlock = UnlockCondition.DownedEyeOfCthulhu;
            category = EntryCategory.Apparatuses;
            icon = ModContent.ItemType<RelayFixture_Item>();
            visible = EntryVisibility.Visible;

            pages = [
                new TextPage(),
                new RecipePage(new List<(List<(int item, int stack)> items, int station, int result, int resultStack, string extras)>
                {
                    (new List<(int, int)>()
                    {
                        (ItemID.SilverBar, 2),
                        (ModContent.ItemType<ShimmeringGlass>(), 1)
                    },
                    ItemID.IronAnvil,
                    ModContent.ItemType<RelayFixture_Item>(),
                    3,
                    string.Empty)
                })
            ];
        }
    }
}