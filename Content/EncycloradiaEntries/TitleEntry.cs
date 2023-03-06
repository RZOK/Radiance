using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using Terraria;
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
            visible = false;
        }
        public override void PageAssembly()
        {
            AddToEntry(this, new TextPage() { 
                text = 
                @"Welcome to the \y Encycloradia. \r | " +
                @"Click on a category to the right in order to view its associated entries. | " +
                @"If an entry is \g locked, \r you will be unable to view it until it is unlocked. | " +
                @"\b Tip of the Day: \r " + 
                Tips[Main.rand.Next(Tips.Length)]
            });
            AddToEntry(this, new MiscPage()
            {
                type = "Title"
            });
        }
        public string[] Tips = {
            @"If two rays intersect, they will both glow red and have their transfer rate significantly reduced. Plan around this!",
            @"Most \a Apparatuses \r will cease to function if powered wire is running through the top left tile portion of them.",
            @"Hovering your mouse over an incomplete entry will reveal to you the method of unlocking it.",
            @"The speed of light in a vacuum is 299,792,458 meters per second.",
            @"Blue light is said to help people relax.",
        };
    }
}
