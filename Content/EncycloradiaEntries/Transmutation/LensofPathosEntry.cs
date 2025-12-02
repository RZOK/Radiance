using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Items.Tools.Misc;
using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries.Transmutation
{
    public class LensofPathosEntry : EncycloradiaEntry
    {
        public LensofPathosEntry()
        {
            fastNavInput = "ULRU";
            incomplete = UnlockCondition.UnlockedByDefault;
            unlock = UnlockCondition.UnlockedByDefault;
            category = EntryCategory.Transmutation;
            icon = ModContent.ItemType<LensofPathos>();
            visible = EntryVisibility.Visible;
            pages = [
                new TextPage(),
                new RecipePage()
                {
                    items = new List<(int, int)>()
                    {
                        (ItemID.CobaltBar, 5),
                        (ModContent.ItemType<ShimmeringGlass>(), 5)
                    },
                    station = GetItem(ItemID.MythrilAnvil),
                    result = GetItem(ModContent.ItemType<AlchemicalLens>())
                }
            ];
        }
    }
}