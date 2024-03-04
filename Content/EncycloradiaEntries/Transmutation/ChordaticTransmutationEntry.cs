using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries.Transmutation
{
    public class ChordaticTransmutationEntry : EncycloradiaEntry
    {
        public ChordaticTransmutationEntry()
        {
            fastNavInput = "RLUR";
            incomplete = UnlockCondition.transmutatorFishUsed;
            unlock = UnlockCondition.transmutatorFishUsed;
            category = EntryCategory.Transmutation;
            icon = ItemID.SpecularFish;
            visible = EntryVisibility.NotVisibleUntilUnlocked;

            pages = [new TextPage()];
        }
    }
}