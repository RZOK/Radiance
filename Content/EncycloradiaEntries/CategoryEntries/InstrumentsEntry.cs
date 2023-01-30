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
                    CommonSnippets.BWSnippet("Two"),
                    new CustomTextSnippet("tools:", CommonColors.InstrumentsColor, CommonColors.InstrumentsColorDark),
                    CommonSnippets.BWSnippet("a spear and a sickle, both invaluable to sustaining and defending life. |"),
                    CommonSnippets.instrumentsSnippet,
                    CommonSnippets.BWSnippet("are tools that may require"),
                    CommonSnippets.radianceSnippet,
                    CommonSnippets.BWSnippet("from your inventory in order to prove useful. |"),
                    CommonSnippets.BWSnippet("Within this section you will find most"),
                    new CustomTextSnippet("Radiance-utilizing", CommonColors.RadianceColor1, CommonColors.RadianceColorDark),
                    CommonSnippets.BWSnippet("weapons, tools, accessories, and other items that you may forge."),
                }
            });
            AddToEntry(this, new CategoryPage() { category = EntryCategory.Instruments });
        }
    }
}