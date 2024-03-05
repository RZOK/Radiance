using Radiance.Content.Items.PedestalItems;
using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries
{
    public class TestEntry : EncycloradiaEntry
    {
        public TestEntry()
        {
            fastNavInput = "ULDR";
            incomplete = UnlockCondition.downedEyeOfCthulhu;
            unlock = UnlockCondition.downedEyeOfCthulhu;
            category = EntryCategory.Influencing;
            visible = EntryVisibility.Visible;

            pages = [
                new TextPage(),
                new TextPage(),
                new TextPage(),
                new TextPage(),
                new TextPage(),
                new TextPage(),
                new TransmutationPage() { recipe = TransmutationRecipeSystem.FindRecipe(nameof(OrchestrationCore)) },
            ];
        }
    }
}