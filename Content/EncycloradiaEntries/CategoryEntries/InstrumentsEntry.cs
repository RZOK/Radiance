using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries
{
    public class InstrumentsEntry : CategoryEntry
    {
        public InstrumentsEntry() : base(EntryCategory.Instruments)
        {
            pages = [new TextPage()];
        }
    }
}