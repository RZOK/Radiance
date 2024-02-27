using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries
{
    public class InfluencingEntry : CategoryEntry 
    {
        public InfluencingEntry() : base("Influencing", EntryCategory.Influencing)
        {
            AddPageToEntry(new TextPage()
            {
                text =
                    $"A &iflower&r blooming from the soil, bearing new life into its broken world.&n&n" +
                    "&iInfluencing&r envelops most manipulation of &yRadiance&r using cells, rays, pedestals, and other similar means.&n&n" +
                    "Within this section you will find anything and everything directly related to moving, storing, and generating &yRadiance&r in and throughout &aApparatuses&r and &sInstruments.&r"
            });
        }
    }
}