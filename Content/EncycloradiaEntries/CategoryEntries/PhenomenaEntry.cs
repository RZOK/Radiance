using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Content.EncycloradiaEntries
{
    public class PhenomenaEntry : CategoryEntry
    {
        public PhenomenaEntry() : base("Phenomena", EntryCategory.Phenomena)
        {
            AddPageToEntry(new TextPage()
            {
                text =
                    "A swirling &hgalaxy&r containing abundant knowledge.&n&n" +
                    "&hPhenomena&r does not envelop a set of items or mechanics, but rather serves as the location for additional information that does not directly affect your experience.&n&n" +
                    "Within this category you will find various pages of lore and explanation for the workings of this world."
            });
        }
    }
}