using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Content.EncycloradiaEntries
{
    public class ApparatusesEntry : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Apparatuses;
            visible = false;
        }

        public override void PageAssembly()
        {
            AddToEntry(this,
            new TextPage()
            {
                text =
                @"A \a contraption \r of unknown potential and workings that remains intriguing to those who gaze upon it. | " +
                @"\a Apparatuses \r are tiles that utilize \y Radiance \r to perform various actions. | " +
                @"Within this section you will find most \y Radiance-utilizing \r tiles that you may create."
            });
            AddToEntry(this, new CategoryPage() { category = EntryCategory.Apparatuses });
        }
    }
}