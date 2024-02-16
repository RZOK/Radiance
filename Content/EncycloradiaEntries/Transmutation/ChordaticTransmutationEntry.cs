using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries.Transmutation
{
    public class ChordaticTransmutationEntry : EncycloradiaEntry
    {
        public ChordaticTransmutationEntry()
        {
            displayName = "Chordatic Transmutation";
            tooltip = "That shouldn't happen!";
            fastNavInput = "RLUR";
            incomplete = UnlockCondition.transmutatorFishUsed;
            unlock = UnlockCondition.transmutatorFishUsed;
            category = EntryCategory.Transmutation;
            icon = ItemID.SpecularFish;
            visible = EntryVisibility.NotVisibleUntilUnlocked;

            AddPageToEntry(new TextPage()
            {
                text =
                @"It seems as if the carcass of a &bSpecular Fish,&r in all its glassy peculiarity, will slot perfectly into the &bProjector's &rfocusing lens slot.&n&n" +
                @"What's even stranger, however, is that the cavern-dwelling sealife also seems to function as a working Transmutation lens — to a degree.&n&n" +
                @"It is entirely unknown to you how the fish operates in the way that it does. Perhaps the mirror-like scales hold similar qualities to &bFlareglass? &rMayhap the intrinsic arcane energies used for brewing Recall Potions trapped within the fish also resonate with &yRadiance.&r&n&n" +
                @"Saying that a lifeless Specular Fish 'functions' as a focus is a bit of a stretch. While slotted in, the efficiency of the Transmutation will be severely detrimented, consuming far, far more &yRadiance&r than usual."
            });
        }
    }
}