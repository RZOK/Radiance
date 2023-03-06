using Radiance.Core.Systems;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Content.EncycloradiaEntries
{
    public class TestEntry : EncycloradiaEntry
    {
        public TestEntry()
        {
            fastNavInput = "ULDR";
            tooltip = "Example entry tooltip";
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.downedPlantera;
            category = EntryCategory.Influencing;
            visible = true;

            AddToEntry(this,
            new TextPage()
            {
                text = @"\i Test Page 1"
            });
            AddToEntry(this,
            new TextPage()
            {
                text = @"\t Test Page 2"
            });
            AddToEntry(this,
            new TextPage()
            {
                text = @"\a Test Page 3"
            });
            AddToEntry(this,
            new TextPage()
            {
                text = @"\n Test Page 4"
            });
            AddToEntry(this,
            new TextPage()
            {
                text = @"\d Test Page 5"
            });
            AddToEntry(this, new TransmutationPage() { recipe = TransmutationRecipeSystem.FindRecipe("Flareglass_0") }
                );
            //AddToEntry(this, new ImagePage()
            //{
            //    texture = TextureAssets.Item[ItemID.ManaCrystal].Value
            //});
            //AddToEntry(this, new TransmutationPage()
            //{
            //    container = Radiance.Instance.GetContent<StandardRadianceCell>() as BaseContainer,
            //    radianceRequired = recipe.requiredRadiance,
            //    input = recipe.inputItem,
            //    output = recipe.outputItem
            //});
        }
    }
}
