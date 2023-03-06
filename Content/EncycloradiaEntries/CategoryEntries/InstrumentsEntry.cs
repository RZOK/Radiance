using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Content.EncycloradiaEntries
{
    public class InstrumentsEntry : EncycloradiaEntry
    {
        public InstrumentsEntry()
        {
            displayName = "Instruments";
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Instruments;
            visible = false;
            AddToEntry(this,
            new TextPage()
            {
                text =
                @"Two \n tools: \r a spear and a sickle, both invaluable to sustaining and defending life. | " +
                @"\n Instruments \r are tools that may require \y Radiance \r from your inventory in order to prove useful. | " +
                @"Within this section you will find most \y Radiance-involving \r weapons, tools, accessories, and other items that you may forge."
            });
            AddToEntry(this, new CategoryPage() { category = EntryCategory.Instruments });
        }
    }
}