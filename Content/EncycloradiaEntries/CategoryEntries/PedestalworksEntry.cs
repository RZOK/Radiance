using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Content.EncycloradiaEntries
{
    public class PedestalworksEntry : EncycloradiaEntry
    {
        public PedestalworksEntry()
        {
            displayName = "Pedestalworks";
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Pedestalworks;
            visible = false;
            AddToEntry(this,
            new TextPage()
            {
                text =
                    @"A \d spire, \r inside of which rests a treasure of power. | " +
                    @"\d Pedestalworks \r is the art of placing objects upon an arcane pedestal and watching as an action is performed, typically in exchange for \y Radiance. \r | " +
                    @"Within this section you will find most objects that have a function when placed upon a pedestal."
            });
            AddToEntry(this, new CategoryPage() { category = EntryCategory.Pedestalworks });
        }
    }
}