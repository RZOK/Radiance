using Radiance.Content.Items.PedestalItems;
using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries.Pedestalworks
{
    public class ManipulationCoresEntry : EncycloradiaEntry
    {
        public ManipulationCoresEntry()
        {
            fastNavInput = "DLLL";
            incomplete = UnlockCondition.hardmode;
            unlock = UnlockCondition.hardmode;
            category = EntryCategory.Pedestalworks;
            icon = ModContent.ItemType<OrchestrationCore>();
            visible = EntryVisibility.Visible;

            pages = [
                new TextPage(),
                new TransmutationPage() { recipe = TransmutationRecipeSystem.FindRecipe(nameof(OrchestrationCore)) },
                new TransmutationPage() { recipe = TransmutationRecipeSystem.FindRecipe(nameof(AnnihilationCore)) },
                new TransmutationPage() { recipe = TransmutationRecipeSystem.FindRecipe(nameof(FormationCore)) },
            ];
        }
    }
}