using Radiance.Content.Items;
using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries.Phenomena
{
    public class EncycloradiaWelcomeEntry : EncycloradiaEntry
    {
        public EncycloradiaWelcomeEntry()
        {
            displayName = "Welcome to the Encycloradia";
            tooltip = "Always there for you";
            fastNavInput = "UDRR";
            incomplete = UnlockCondition.unlockedByDefault;
            unlock = UnlockCondition.unlockedByDefault;
            category = EntryCategory.Phenomena;
            icon = ModContent.ItemType<KnowledgeScroll>();
            visible = EntryVisibility.Visible;

            AddPageToEntry(new TextPage()
            {
                text =
                @"Welcome to the \y Encycloradia! \r | " +
                @"This book is not a tangible object, despite the odd quirk of being able to sense the covers of \y Hard Light \r that bind it together upon shutting your eyes. This receptacle of knowledge exists purely within your mindscape, but is no less useful than any other tome. | " +
                @"The book is divided into six different categories, with each category holding numerous entries for every item and mechanic related to \y Radiance. \r Most entries will initially remain in a \g locked \r state, only revealing their secrets once certain enlightening requirements have been fulfilled. | " +
                @"The \y Encycloradia \r is designed in such a way that there should be no need for a wiki in order to experience the mod in its entirety. Aside from a few trace easter eggs, nothing is kept secret from you. All of the information that you would need about an item, tile, or mechanic will be included in its associated entry. | " +
                @"If you are just starting out for the first time, make sure to look through each category and unlocked entry in order to get an idea as to how you should progress on your journey. | " +
                @"Enjoy!"
            });
        }
    }
}