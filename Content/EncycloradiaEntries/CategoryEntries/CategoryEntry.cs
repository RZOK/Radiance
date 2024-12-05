using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries
{
    public abstract class CategoryEntry : EncycloradiaEntry
    {
        public CategoryEntry(EntryCategory category)
        {
            incomplete = UnlockCondition.UnlockedByDefault;
            unlock = UnlockCondition.UnlockedByDefault;
            this.category = category;
            visible = EntryVisibility.NotVisible;
        }
    }
}