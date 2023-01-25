using Microsoft.Xna.Framework;
using Terraria.ID;
using Radiance.Core.Systems;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Utilities.CommonColors;
using Terraria;
using Radiance.Core;
using static Radiance.Core.Systems.UnlockSystem;

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
                text = new CustomTextSnippet[] { new CustomTextSnippet("Welcome to the", Color.White, Color.Black),
                new CustomTextSnippet("Encycloradia. |", RadianceColor1, RadianceColorDark),
                new CustomTextSnippet("Click on a category to the right in order to view its entries. |", Color.White, Color.Black),
                new CustomTextSnippet("If an entry is", Color.White, Color.Black),
                new CustomTextSnippet("locked,", LockedColor, LockedColorDark),
                new CustomTextSnippet("you will be unable to view it until it is unlocked. |", Color.White, Color.Black),
                new CustomTextSnippet("Tip of the Day:", RadianceColor1, RadianceColorDark),
                Tips[Main.rand.Next(Tips.Length)]
                }
            });
            AddToEntry(this, new MiscPage()
            {
                type = "Title"
            });
        }
        public CustomTextSnippet[] Tips = {
            new CustomTextSnippet("If two rays intersect, they will both glow red and have their transfer rate significantly reduced. Plan around this!", Color.White, Color.Black),
            new CustomTextSnippet("Most apparatuses will cease to function if powered wire is running through them.", Color.White, Color.Black)
        };
    }
}
