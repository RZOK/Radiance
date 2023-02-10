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
            visible = false;
        }

        public override void PageAssembly()
        {
            AddToEntry(this,
            new TextPage()
            {
                text = new CustomTextSnippet[] 
                {
                    "A ".BWSnippet(),
                    "flower ".DarkColorSnippet(CommonColors.InfluencingColor),
                    "blooming from the soil, bearing new life into the world it exists in. |".BWSnippet(),
                    "Influencing ".DarkColorSnippet(CommonColors.InfluencingColor),
                    "is the art of manipulating ".BWSnippet(),
                    "Radiance ".DarkColorSnippet(CommonColors.RadianceColor1),
                    "with cells, rays, pedestals, and other similar means. | Within this section you will find anything and everything directly related to moving and storing ".BWSnippet(),
                    "Radiance ".DarkColorSnippet(CommonColors.RadianceColor1),
                    "in and throughout ".BWSnippet(),
                    "Apparatuses ".DarkColorSnippet(CommonColors.ApparatusesColor),
                    "and ".BWSnippet(),
                    "Instruments.".DarkColorSnippet(CommonColors.InstrumentsColor)
                }
            });
            AddToEntry(this, new CategoryPage() { category = EntryCategory.Influencing });
        }
    }
}