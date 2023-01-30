using Microsoft.Xna.Framework;
using Radiance.Core;
using Radiance.Core.Systems;
using Radiance.Utilities;
using Terraria.ID;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Content.EncycloradiaEntries
{
    public class InfluencingEntry : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Influencing;
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
                    CommonSnippets.BWSnippet("A"),
                    new CustomTextSnippet("flower", CommonColors.InfluencingColor, CommonColors.InfluencingColorDark),
                    CommonSnippets.BWSnippet("blooming from the soil, bearing new life into the world it exists in. |"),
                    CommonSnippets.influencingSnippet,
                    CommonSnippets.BWSnippet("is the art of manipulating"),
                    CommonSnippets.radianceSnippet,
                    CommonSnippets.BWSnippet("with cells, rays, pedestals, and other similar means. | Within this section you will find anything and everything directly related to moving and storing"),
                    CommonSnippets.radianceSnippet,
                    CommonSnippets.BWSnippet("in and throughout"),
                    CommonSnippets.apparatusesSnippet,
                    CommonSnippets.BWSnippet("and"),
                    CommonSnippets.instrumentsSnippetPeriod
                }
            });
            AddToEntry(this, new CategoryPage() { category = EntryCategory.Influencing });
        }
    }
}