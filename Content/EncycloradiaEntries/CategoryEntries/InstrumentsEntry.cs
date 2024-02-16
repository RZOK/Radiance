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
                    "Two &ntools:&r a spear and a sickle, both invaluable to sustaining and defending life.&n&n" +
                    "&sInstruments&r are tools that may require &yRadiance&r from your inventory in order to prove useful.&n&n" +
                    "Within this section you will find most &yRadiance-involving&r weapons, tools, accessories, and other items that you may forge."
            });
        }
    }
}