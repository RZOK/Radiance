using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries
{
    public class InfluencingEntry : CategoryEntry 
    {
        public InfluencingEntry() : base(EntryCategory.Influencing)
        {
            pages = [new TextPage(), new CategoryPage(category)];
        }
    }
}