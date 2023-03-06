using Terraria.ModLoader;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.UnlockSystem;
using Radiance.Content.Items;

namespace Radiance.Content.EncycloradiaEntries.Phenomena
{
    public class ObserversEntry : EncycloradiaEntry
    {
        public ObserversEntry()
        {
            displayName = "External Observers";
            tooltip = ":)";
            fastNavInput = "DDDD";
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Phenomena;
            icon = ModContent.ItemType<KnowledgeScroll>();
            visible = true;

            AddToEntry(this, new TextPage()
            {
                text =
                @"You are not alone. | " +
                @"Monstrous beings of shadow exist outside of your dimension in a location called \b the void \r. Creatures adorned with scarlet eyes that look in every direction, and through every object. Currently, they are unable to fully work through the trans-dimensional barrier that separates them from your plane, leaving them mostly harmless aside from the occasional scare. | " +
                @"The burst of potential energy released through \t Transmutation \r is enough to shine through the ethereal glass hung between dimensions, letting you peer, into a space you should not be seeing, for a fleeting moment. Rarely will you actually notice one of these beings, as they tend to be rather furtive. You would rather forget about the appearance of the monolithic entities, but sometimes peeking beyond the veil is accidental or unavoidable. | " +
                @"Surely there must be some method to draw their interest further, if, for whatever reason, you wish to see the beasts more often… "
            });
        }
    }
}
