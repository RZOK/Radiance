using Radiance.Content.Items;
using Terraria.ModLoader;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Content.EncycloradiaEntries.Phenomena
{
    public class HardLightEntry : EncycloradiaEntry
    {
        public HardLightEntry()
        {
            displayName = "Hard Light";
            tooltip = "Placeholder Text";
            fastNavInput = "RRUU";
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Phenomena;
            icon = ModContent.ItemType<KnowledgeScroll>();
            visible = true;

            AddToEntry(this, new TextPage()
            {
                text =
                @"The caveats of \y Radiance \r lie in its rather transient state when residing outside of a glass container, leaving any practical uses of its material properties to be rather limited outside of tight environments that are exceedingly well controlled. Even then, it only features one quirk that can be taken advantage of, but it is one quirk that proves to be invaluable. | " +
                @"When a significant amount of \y Radiance \r is compressed into a particularly tight space, it will begin to \b crystalize \r around the walls of its enclosure, eventually resulting in a solid container entirely surrounding the inner \y Radiance. \r This form of \y Radiance \r is known as \y Cold Hard Light (CHL), \r and it possesses a few traits that would make it desirable to eventually create, once the knowledge of how to perform such a task has graced you. | " +
                @"The \y Radiance \r trapped inside of \y CHL \r functions to keep the crystal shell supported and dense, making it a highly desirable material to use for situations that require high resilience to the elements. If the exterior of it manages to crack or open up somehow, all of the \y Radiance \r within will instantly disperse into the atmosphere, causing the shell to instantly crumple in on itself. The sheer pressure from the exterior of \y CHL \r 'collapsing' will immediately proceed to pulverize said crystal material back into standard glowing \y Radiance, \r which will likewise also dissipate. | " +
                @"Despite the name, \y CHL \r typically rests in a moderate temperature, due to its ability to transfer and absorb heat from its surroundings at an unnatural pace, which may serve as an important trait for future creations. | " +
                @"The surface of \y CHL \r takes on the appearance of large, crystalline pieces of broken glass melded together in a flush, smooth manner. A subtle amber glow radiates off of the material at all times. When light shines through a mass of \y CHL, \r it will shine through it as if there was no object in between the source of the light and the opposite side. Various individuals have tested the taste of \y CHL \r by licking formations of it, and have reported that it tastes like 'vaguely lemon-flavored glass.' | " +
                @"You may have already had the thought that if there's a cold-prefixed \y Hard Light, \r then there must be a normal variant too, right? |" +
                @"Wrong. | " +
                @"Instead, the other variant of it is a sharp contrast, taking the form of \y Searing Hard Light, \r an ephemeral, plasma-esque material that exists exclusively at laughably high temperatures. \y SHL \r manifests when \y Radiance \r is simultaneously compressed and superheated, resulting in the entirety of it jetting out in a thin pane through any egresses it possibly can. | " +
                @"The temperature of \y SHL \r clocks in at around six thousand degrees Kelvin, while the air around the edges of a formation come in at just about three thousand less. | "
            });
        }
    }
}