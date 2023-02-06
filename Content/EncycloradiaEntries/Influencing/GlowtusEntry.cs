using Terraria.ModLoader;
using Radiance.Content.Items.ProjectorLenses;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using Radiance.Core;
using Radiance.Utilities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ID;
using Radiance.Content.Items.Tools.Misc;
using Terraria;
using System;
using Radiance.Core.Systems;
using static Radiance.Core.Systems.UnlockSystem;
using Radiance.Content.Tiles;
using Microsoft.Xna.Framework.Graphics;

namespace Radiance.Content.EncycloradiaEntries
{
    public class GlowtusEntry : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
            displayName = "Glowtuses";
            tooltip = "Green light";
            fastNavInput = "UDRR";
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Influencing;
            icon = ModContent.ItemType<GlowtusItem>();
            visible = true;
        }
        public override void PageAssembly()
        {
            AddToEntry(this, new TextPage() { text = new CustomTextSnippet[] 
            { 
                "Fallen Stars are great, but not without their flaws. They only fall at night, and if you try and throw any to the ground during daytime, they all vanish into a puff of stardust. These problems prove to be definite issues for anyone trying to create ".BWSnippet(),
                "Radiance. |".DarkColorSnippet(CommonColors.RadianceColor1),
                "There exists, however, another way of creating".BWSnippet(),
                "Radiance, ".DarkColorSnippet(CommonColors.RadianceColor1),
                "through the use of natural flora.".BWSnippet(),
                "Glowtuses ".DarkColorSnippet(CommonColors.ContextColor),
                "are unique herbs that grow naturally at extremely high elevations, typically on ".BWSnippet(),
                "unnatural floating landmasses. |".DarkColorSnippet(CommonColors.ContextColor),
                "The strange, starry flowers innately carry trace amounts of ".BWSnippet(),
                "Radiance ".DarkColorSnippet(CommonColors.RadianceColor1),
                "within their structure, allowing them to be disassembled by ".BWSnippet(),
                "Radiance ".DarkColorSnippet(CommonColors.RadianceColor1),
                "Cells ".BWSnippet(),
                "into the glowing goodness, albeit at a faster rate, but with less produced than Fallen Stars. |".BWSnippet(),
                "Like all other usual herbs, ".BWSnippet(),
                "Glowtuses ".DarkColorSnippet(CommonColors.ContextColor),
                "have their own seeds that drop when they are blooming, which may be planted on grass, pots, or Planter Boxes. |".BWSnippet(),
                "The conditions for a ".BWSnippet(),
                "Glowtus ".DarkColorSnippet(CommonColors.ContextColor),
                "to bloom are strange. The exact down-to-the-molecule alignment to various major stars that a specific plant has will determine whether it gets primed by daytime, or nighttime. |".BWSnippet(),
                "A decorative, hanging potted variant of the flower also exists, if it is to your liking for aesthetical purposes.".BWSnippet(),
            }
            } );
        }
    }
}
