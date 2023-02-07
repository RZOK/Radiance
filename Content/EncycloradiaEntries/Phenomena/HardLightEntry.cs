using Terraria.ModLoader;
using Radiance.Content.Items.ProjectorLenses;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using Radiance.Core;
using Radiance.Utilities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ID;
using Terraria;
using System;
using Radiance.Core.Systems;
using static Radiance.Core.Systems.UnlockSystem;
using Radiance.Content.Tiles;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items;

namespace Radiance.Content.EncycloradiaEntries
{
    public class HardLightEntry : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
            displayName = "Hard Light";
            tooltip = "Placeholder Text";
            fastNavInput = "RRUU";
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Phenomena;
            icon = ModContent.ItemType<KnowledgeScroll>();
            visible = true;
        }
        public override void PageAssembly()
        {
            AddToEntry(this, new TextPage() { text = new CustomTextSnippet[] 
            { 
                "The caveats of ".BWSnippet(), 
                "Radiance ".DarkColorSnippet(CommonColors.RadianceColor1),
                "lie in its rather transient state when residing outside of a glass container, leaving any practical uses of its material structure to be rather limited outside of tight environments that are exceedingly well controlled; even then, it only features one quirk that can be taken advantage of, but it is one quirk that proves to be invaluable. |".BWSnippet(),
                "When a significant amount of ".BWSnippet(),
                "Radiance ".DarkColorSnippet(CommonColors.RadianceColor1),
                "is compressed into a particularly tight space, it will begin to ".BWSnippet(),
                "crystalize ".DarkColorSnippet(CommonColors.ContextColor),
                "around the walls of its enclosure, eventually resulting in a solid container entirely surrounding the inner ".BWSnippet(),
                "Radiance. ".DarkColorSnippet(CommonColors.RadianceColor1),
                "This form of ".BWSnippet(),
                "Radiance ".DarkColorSnippet(CommonColors.RadianceColor1),
                "is known as ".BWSnippet(),
                "Cold Hard Light (CHL), ".DarkColorSnippet(CommonColors.RadianceColor1),
                "and it possesses a few traits that would make it desirable to eventually create, once the knowledge of how to perform such a task has graced you. |".BWSnippet(),
                "The ".BWSnippet(),
                "Radiance ".DarkColorSnippet(CommonColors.RadianceColor1),
                "trapped inside of ".BWSnippet(),
                "CHL ".DarkColorSnippet(CommonColors.RadianceColor1),
                "functions to keep the crystal shell supported and dense, making it a highly desirable material to use for situations that require high resilience to the elements. If the exterior of it manages to crack or open up somehow, all of the ".BWSnippet(),
                "Radiance ".DarkColorSnippet(CommonColors.RadianceColor1),
                "within will instantly disperse into the atmosphere, causing the shell to instantly crumple in on itself. The sheer pressure from the exterior of ".BWSnippet(),
                "CHL ".DarkColorSnippet(CommonColors.RadianceColor1),
                "'collapsing' will immediately proceed to pulverize said crystal material back into standard glowing".BWSnippet(),
                "Radiance, ".DarkColorSnippet(CommonColors.RadianceColor1),
                "which will likewise also dissipate. |".BWSnippet(),
                "Despite the name, ".BWSnippet(),
                "CHL ".DarkColorSnippet(CommonColors.RadianceColor1),
                "typically rests in a moderate temperature, due to its ability to transfer and absorb heat from its surroundings at an unnatural pace, which may serve as an important trait for future creations. ".BWSnippet(),
                "The surface of ".BWSnippet(),
                "CHL ".DarkColorSnippet(CommonColors.RadianceColor1),
                "takes on the appearance of large, crystalline pieces of broken glass melded together in a flush, smooth manner. A subtle amber glow radiates off of the material at all times. When light shines through a mass of ".BWSnippet(),
                "CHL, ".DarkColorSnippet(CommonColors.RadianceColor1),
                "it will shine through it as if there was no object in between the source of the light and the opposite side. Various individuals have tested the taste of ".BWSnippet(),
                "CHL ".DarkColorSnippet(CommonColors.RadianceColor1),
                "by licking formations of it, and have reported that it tastes like 'vaguely lemon-flavored glass.' |".BWSnippet(),
                "You may have already been able to guess that if there's a cold variant of ".BWSnippet(),
                "Hard Light, ".DarkColorSnippet(CommonColors.RadianceColor1),
                "then there must be a normal variant too, right? | Wrong. |".BWSnippet(),
                "Instead, the other variant of it is a sharp contrast, taking the form of, ".BWSnippet(),
                "Searing Hard Light, ".DarkColorSnippet(CommonColors.RadianceColor1),
                "an ephemeral, plasma-esque material that exists exclusively at laughably high temperatures. |".BWSnippet(),
                "SHL ".DarkColorSnippet(CommonColors.RadianceColor1),
                "manifests when ".BWSnippet(),
                "Radiance ".DarkColorSnippet(CommonColors.RadianceColor1),
                "is simultaneously compressed and superheated, resulting in the entirety of it jetting out in a thin pane through any egresses it possibly can. |".BWSnippet(),
                "The properties exhibited by ".BWSnippet(),
                "SHL ".DarkColorSnippet(CommonColors.RadianceColor1),
                "are rather obvious. The temperature of a formation itself clocks in at around six thousand degrees Kelvin, while the air around the edges of one come in at just about three thousand less. ".BWSnippet()

            }
            } );
        }
    }
}
