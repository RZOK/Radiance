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
            icon = ItemID.ManaCrystal;
            visible = false;
        }
        public override void PageAssembly()
        {
            AddToEntry(this, new TextPage()
            {
                text = new CustomTextSnippet[] { CommonSnippets.BWSnippet("Welcome to the"),
                new CustomTextSnippet("Encycloradia. |", RadianceColor1, RadianceColor1.GetDarkColor()),
                CommonSnippets.BWSnippet("Click on a category to the right in order to view its entries. |"),
                CommonSnippets.BWSnippet("If an entry is"),
                new CustomTextSnippet("locked,", LockedColor, LockedColor.GetDarkColor()),
                CommonSnippets.BWSnippet("you will be unable to view it until it is unlocked. |"),
                new CustomTextSnippet("Tip of the Day:", RadianceColor1, RadianceColor1.GetDarkColor()),
                Tips[Main.rand.Next(Tips.Length)]
                }
            });
            AddToEntry(this, new MiscPage()
            {
                type = "Title"
            });
        }
        public CustomTextSnippet[] Tips = {
            CommonSnippets.BWSnippet("If two rays intersect, they will both glow red and have their transfer rate significantly reduced. Plan around this!"),
            CommonSnippets.BWSnippet("Most apparatuses will cease to function if powered wire is running through them."),
            CommonSnippets.BWSnippet("Hovering your mouse over an incomplete entry will reveal to you the method of unlocking it."),
        };
    }
}
