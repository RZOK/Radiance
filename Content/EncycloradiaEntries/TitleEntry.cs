using Microsoft.Xna.Framework;
using Terraria.ID;
using Radiance.Core.Systems;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Utilities.CommonColors;
using Terraria;
using Radiance.Core;
using static Radiance.Core.Systems.UnlockSystem;
using Radiance.Utilities;

namespace Radiance.Content.EncycloradiaEntries
{
    public class TitleEntry : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.None;
            visible = false;
        }
        public override void PageAssembly()
        {
            AddToEntry(this, new TextPage()
            {
                text = new CustomTextSnippet[] 
                { 
                    "Welcome to the ".BWSnippet(),
                    "Encycloradia. |".DarkColorSnippet(RadianceColor1),
                    "Click on a category to the right in order to view its entries. |".BWSnippet(),
                    "If an entry is ".BWSnippet(),
                    "locked, ".DarkColorSnippet(LockedColor),
                    "you will be unable to view it until it is unlocked. |".BWSnippet(),
                    "Tip of the Day: ".DarkColorSnippet(ContextColor),
                    Tips[Main.rand.Next(Tips.Length)]
                }
            });
            AddToEntry(this, new MiscPage()
            {
                type = "Title"
            });
        }
        public CustomTextSnippet[] Tips = {
            "If two rays intersect, they will both glow red and have their transfer rate significantly reduced. Plan around this!".BWSnippet(),
            "Most apparatuses will cease to function if powered wire is running through them.".BWSnippet(),
            "Hovering your mouse over an incomplete entry will reveal to you the method of unlocking it.".BWSnippet(),
        };
    }
}
