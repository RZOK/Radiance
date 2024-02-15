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
                @"It seems as if the carcass of a \b Specular Fish, \r in all its glassy peculiarity, will slot perfectly into the \b Projector's \r focusing lens slot. | " +
                @"What's even stranger, however, is that the cavern-dwelling sealife also seems to function as a working Transmutation lens — to a degree. | " +
                @"It is entirely unknown to you how the fish operates in the way that it does. Perhaps the mirror-like scales hold similar qualities to \b Flareglass? \r Mayhap the intrinsic arcane energies used for brewing Recall Potions trapped within the fish also resonate with \y Radiance. \r | " +
                @"Saying that a lifeless Specular Fish 'functions' as a focus is a bit of a stretch. While slotted in, the efficiency of the Transmutation will be severely detrimented, consuming far, far more \y Radiance \r than usual."
            });
        }
    }
}