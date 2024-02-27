using Radiance.Content.EncycloradiaEntries.Apparatuses;
using Radiance.Content.EncycloradiaEntries.Pedestalworks;
using Radiance.Content.Items.PedestalItems;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries
{
    public class TestEntry : EncycloradiaEntry
    {
        public TestEntry()
        {
            displayName = "Test";
            fastNavInput = "ULDR";
            tooltip = "Example entry tooltip";
            incomplete = UnlockCondition.downedEyeOfCthulhu;
            unlock = UnlockCondition.downedEyeOfCthulhu;
            category = EntryCategory.Influencing;
            visible = EntryVisibility.Visible;

            AddPageToEntry(new TextPage()
            {
                text = "&iTest Page&r"
            });
            AddPageToEntry(new TextPage()
            {
                text = "&tTest Page 2&r"
            });
            AddPageToEntry(new TextPage()
            {
                text = "&aTest Page 3&r"
            });
            AddPageToEntry(new TextPage()
            {
                text = "&sTest Page 4&r"
            });
            AddPageToEntry(new TextPage()
            {
                text = "&dTest Page 5&r"
            });
            AddPageToEntry(new TextPage()
            {
                text = $"This text isn't hidden!&n&n[c:{nameof(StarlightBeaconEntry)}:But this is! This too! And this!]&nBut those ↕ are!&n[c:{nameof(StarlightBeaconEntry)}:Won't see this one either!]&nThis one is real, though!"
            });
            AddPageToEntry(new TransmutationPage() { recipe = TransmutationRecipeSystem.FindRecipe(nameof(OrchestrationCore)) });
            //AddPageToEntry(this, new ImagePage()
            //{
            //    texture = TextureAssets.Item[ItemID.ManaCrystal].Value
            //});
            //AddPageToEntry(this, new TransmutationPage()
            //{
            //    container = Radiance.Instance.GetContent<StandardRadianceCell>() as BaseContainer,
            //    radianceRequired = recipe.requiredRadiance,
            //    input = recipe.inputItem,
            //    output = recipe.outputItem
            //});
        }
    }
}