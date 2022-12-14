using Microsoft.Xna.Framework;
using Radiance.Core;
using Radiance.Utilities;
using Terraria.ID;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Content.EncycloradiaEntries
{
    public class InstrumentsEntry : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
            displayName = "Instruments";
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Instruments;
            icon = ItemID.ManaCrystal;
            visible = false;
        }

        public override void PageAssembly()
        {
            AddToEntry(this,
            new TextPage()
            {
                text = new CustomTextSnippet[] 
                {
                    new CustomTextSnippet("Two", Color.White, Color.Black),
                    new CustomTextSnippet("tools:", RadianceUtils.InstrumentsColor, RadianceUtils.InstrumentsColorDark),
                    new CustomTextSnippet("a spear and a sickle, both invaluable to sustaining and defending life. NEWLINE NEWLINE", Color.White, Color.Black),
                    RadianceUtils.instrumentsSnippet,
                    new CustomTextSnippet("are tools that may require", Color.White, Color.Black),
                    RadianceUtils.radianceSnippet,
                    new CustomTextSnippet("from your inventory in order to prove useful. NEWLINE NEWLINE", Color.White, Color.Black),
                    new CustomTextSnippet("Within this section you will find most", Color.White, Color.Black),
                    new CustomTextSnippet("Radiance-utilizing", RadianceUtils.RadianceColor1, RadianceUtils.RadianceColorDark),
                    new CustomTextSnippet("weapons, tools, accessories, and other items that you may forge.", Color.White, Color.Black),
                }
            });
            AddToEntry(this, new CategoryPage() { category = EntryCategory.Instruments });
        }
    }
}