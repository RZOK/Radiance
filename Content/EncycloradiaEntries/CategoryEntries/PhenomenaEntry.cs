using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Content.EncycloradiaEntries
{
    public class PhenomenaEntry : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Phenomena;
            visible = false;
        }

        public override void PageAssembly()
        {
            AddToEntry(this,
            new TextPage()
            {
                text =
                @"A swirling \h galaxy \r containing abundant knowledge. | " +
                @"\h Phenomena \r does not envelop a set of items or mechanics, but rather serves as the location for additional information that does not directly affect your experience. | " +
                @"Within this category you will find various pages of lore and explanation for the workings of this world."
            });
            AddToEntry(this, new CategoryPage() { category = EntryCategory.Phenomena });
        }
    }
}