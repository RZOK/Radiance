using Microsoft.Xna.Framework;
using Radiance.Core;
using Radiance.Core.Systems;
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
                    "Two".BWSnippet(),
                    "tools:".DarkColorSnippet(CommonColors.InstrumentsColor),
                    "a spear and a sickle, both invaluable to sustaining and defending life. |".BWSnippet(),
                    CommonSnippets.instrumentsSnippet,
                    "are tools that may require".BWSnippet(),
                    CommonSnippets.radianceSnippet,
                    "from your inventory in order to prove useful. |".BWSnippet(),
                    "Within this section you will find most".BWSnippet(),
                    "Radiance-utilizing".DarkColorSnippet(CommonColors.RadianceColor1),
                    "weapons, tools, accessories, and other items that you may forge.".BWSnippet(),
                }
            });
            AddToEntry(this, new CategoryPage() { category = EntryCategory.Instruments });
        }
    }
}