using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Items.Tools.Misc;
using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries.Transmutation
{
    public class LivingLensEntry : EncycloradiaEntry
    {
        public LivingLensEntry()
        {
            fastNavInput = "ULRU";
            incomplete = UnlockCondition.UnlockedByDefault;
            unlock = UnlockCondition.UnlockedByDefault;
            category = EntryCategory.Transmutation;
            icon = ModContent.ItemType<LivingLens>();
            visible = EntryVisibility.Visible;
            pages = [
                new TextPage(),
                new RecipePage(new List<(List<(int item, int stack)> items, int station, int result, int resultStack, string extras)>
                {
                    (new List<(int, int)>()
                    {
                        (ItemID.CobaltBar, 5),
                        (ModContent.ItemType<ShimmeringGlass>(), 5)
                    },
                    ItemID.MythrilAnvil,
                    ModContent.ItemType<AlchemicalLens>(),
                    1,
                    string.Empty)
                })
            ];
        }
    }
}