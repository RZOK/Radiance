using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries
{
    public class ApparatusesEntry : CategoryEntry
    {
        public ApparatusesEntry() : base("Apparatuses", EntryCategory.Apparatuses)
        {
            AddPageToEntry(new TextPage()
            {
                text =
                "A &acontraption&r of unknown potential and workings that remains intriguing to those who gaze upon it.&n&n" +
                "&aApparatuses &rare tiles that utilize &yRadiance&r to perform various actions.&n&n" +
                "Within this section you will find most &yRadiance-utilizing&r tiles that you may create."
            });
        }
    }
}