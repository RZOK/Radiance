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
                @"A \a contraption \r of unknown potential and workings that remains intriguing to those who gaze upon it. | " +
                @"\a Apparatuses \r are tiles that utilize \y Radiance \r to perform various actions. | " +
                @"Within this section you will find most \y Radiance-utilizing \r tiles that you may create."
            });
        }
    }
}