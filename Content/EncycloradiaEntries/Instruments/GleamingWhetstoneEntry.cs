using Radiance.Content.Items.Accessories;
using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries.Instruments
{
    public class GleamingWhetstoneEntry : EncycloradiaEntry
    {
        public GleamingWhetstoneEntry()
        {
            fastNavInput = "LDRR";
            incomplete = UnlockCondition.UnlockedByDefault;
            unlock = UnlockCondition.UnlockedByDefault;
            category = EntryCategory.Instruments;
            icon = ModContent.ItemType<GleamingWhetstone>();
            visible = EntryVisibility.Visible;
            
            pages = [
                new TextPage(),
                //new TransmutationPage() 
                //{ 
                //    recipe = TransmutationRecipeSystem.FindRecipe(nameof(RingofFrugality)) 
                //}
            ];
        }
    }
}