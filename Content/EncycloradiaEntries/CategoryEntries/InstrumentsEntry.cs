using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries
{
    public class InstrumentsEntry : CategoryEntry
    {
        public InstrumentsEntry() : base("Instruments", EntryCategory.Instruments)
        {
            AddPageToEntry(new TextPage()
            {
                text =
                @"Two \n tools: \r a spear and a sickle, both invaluable to sustaining and defending life. | " +
                @"\n Instruments \r are tools that may require \y Radiance \r from your inventory in order to prove useful. | " +
                @"Within this section you will find most \y Radiance-involving \r weapons, tools, accessories, and other items that you may forge."
            });
        }
    }
}