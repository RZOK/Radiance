using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Content.EncycloradiaEntries
{
    public class InfluencingEntry : EncycloradiaEntry
    {
        public InfluencingEntry()
        {
            displayName = "Influencing";
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Influencing;
            visible = false;
            AddToEntry(this,
            new TextPage()
            {
                text =
                    @"A \i flower \r blooming from the soil, bearing new life into the world it exists in. | " +
                    @"\i Influencing \r is the art of manipulating \y Radiance \r with cells, rays, pedestals, and other similar means. | " +
                    @"Within this section you will find anything and everything directly related to moving, storing, and generating \y Radiance \r in and throughout \a Apparatuses \r and \n Instruments."
            });
            AddToEntry(this, new CategoryPage() { category = EntryCategory.Influencing });
        }
    }
}