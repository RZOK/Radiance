using Microsoft.Xna.Framework;
using Radiance.Core;
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
                    new CustomTextSnippet("A", Color.White, Color.Black),
                    new CustomTextSnippet("flower", CommonColors.InfluencingColor, CommonColors.InfluencingColorDark),
                    new CustomTextSnippet("blooming from the soil, bearing new life into the world it exists in. |", Color.White, Color.Black),
                    RadianceUtils.influencingSnippet,
                    new CustomTextSnippet("is the art of manipulating", Color.White, Color.Black),
                    RadianceUtils.radianceSnippet,
                    new CustomTextSnippet("with cells, rays, pedestals, and other similar means. | Within this section you will find anything and everything directly related to moving and storing", Color.White, Color.Black),
                    RadianceUtils.radianceSnippet,
                    new CustomTextSnippet("in and throughout", Color.White, Color.Black),
                    RadianceUtils.apparatusesSnippet,
                    new CustomTextSnippet("and", Color.White, Color.Black),
                    RadianceUtils.instrumentsSnippetPeriod
                }
            });
            AddToEntry(this, new CategoryPage() { category = EntryCategory.Influencing });
        }
    }
}