using Radiance.Core.Encycloradia;

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