using Radiance.Content.EncycloradiaEntries.Pedestalworks;
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
                text = $"[c:{nameof(ManipulationCoresEntry)}:&dTest Page 6.&r This should be hidden!] And this should not!"
            });
            AddPageToEntry(new TransmutationPage() { recipe = TransmutationRecipeSystem.FindRecipe(nameof(ShimmeringGlass)) });
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