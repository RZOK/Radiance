using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries
{
    public class ApparatusesEntry : CategoryEntry
    {
        public ApparatusesEntry() : base(EntryCategory.Apparatuses)
        {
            pages = [new TextPage()];
        }
    }
}