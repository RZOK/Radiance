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
                text = @"\i Test Page 1"
            });
            AddPageToEntry(new TextPage()
            {
                text = @"\t Test Page 2"
            });
            AddPageToEntry(new TextPage()
            {
                text = @"\a Test Page 3"
            });
            AddPageToEntry(new TextPage()
            {
                text = @"\n Test Page 4"
            });
            AddPageToEntry(new TextPage()
            {
                text = @"\d Test Page 5"
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