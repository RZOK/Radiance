using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries
{
    public abstract class CategoryEntry : EncycloradiaEntry
    {
        public CategoryEntry(string name, EntryCategory category)
        {
            displayName = name;
            incomplete = UnlockCondition.unlockedByDefault;
            unlock = UnlockCondition.unlockedByDefault;
            this.category = category;
            visible = EntryVisibility.NotVisible;
        }
    }
}