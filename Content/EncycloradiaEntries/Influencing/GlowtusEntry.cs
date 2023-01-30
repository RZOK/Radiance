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
                CommonSnippets.BWSnippet("Fallen Stars are great, but not without their flaws. They only fall at night, and if you try and throw any to the ground during daytime, they all vanish into a puff of stardust. These problems prove to be definite issues for anyone trying to create"),
                CommonSnippets.radianceSnippetPeriod,
                CommonSnippets.BWSnippet("| There exists, however, another way of creating"),
                CommonSnippets.radianceSnippetPeriod,
                new CustomTextSnippet("| Glowtuses", CommonColors.ContextColor, CommonColors.ContextColorDark),
                CommonSnippets.BWSnippet("are unique herbs that grow naturally at extremely high elevations, typically on"),
                new CustomTextSnippet("unnatural floating landmasses. |", CommonColors.ContextColor, CommonColors.ContextColorDark),
                CommonSnippets.BWSnippet("The strange, starry flowers innately carry trace amounts of"),
                CommonSnippets.radianceSnippet,
                CommonSnippets.BWSnippet("within their structure, allowing them to be disassembled by"),
                CommonSnippets.radianceSnippet,
                CommonSnippets.BWSnippet("Cells"),
                CommonSnippets.BWSnippet("into the glowing goodness, albeit at a faster rate, but with less produced than Fallen Stars. |"),
                CommonSnippets.BWSnippet("Like all other usual herbs,"),
                new CustomTextSnippet("Glowtuses", CommonColors.ContextColor, CommonColors.ContextColorDark),
                CommonSnippets.BWSnippet("have their own seeds that drop when they are blooming, which may be planted on grass, pots, or Planter Boxes. |"),
                CommonSnippets.BWSnippet("The conditions for a"),
                new CustomTextSnippet("Glowtus", CommonColors.ContextColor, CommonColors.ContextColorDark),
                CommonSnippets.BWSnippet("to bloom are strange. The exact down-to-the-molecule alignment to various major stars that a specific plant has will determine whether it gets primed by daytime, or nighttime. |"),
                CommonSnippets.BWSnippet("A decorative, hanging potted variant of the flower also exists, if it is to your liking for aesthetical purposes."),
            }
            } );
        }
    }
}
