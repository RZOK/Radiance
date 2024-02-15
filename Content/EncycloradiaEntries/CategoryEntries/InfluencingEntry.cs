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
                    @"A \i flower \r blooming from the soil, bearing new life into the world it exists in. | " +
                    @"\i Influencing \r is the art of manipulating \y Radiance \r with cells, rays, pedestals, and other similar means. | " +
                    @"Within this section you will find anything and everything directly related to moving, storing, and generating \y Radiance \r in and throughout \a Apparatuses \r and \n Instruments."
            });
        }
    }
}