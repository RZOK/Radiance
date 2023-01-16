using Microsoft.Xna.Framework;
using Radiance.Core;
using Radiance.Utilities;
using Terraria.ID;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Content.EncycloradiaEntries
{
    public class PhenomenaEntry : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Phenomena;
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
                    new CustomTextSnippet("A strange, mistifying", Color.White, Color.Black),
                    new CustomTextSnippet("symbol", RadianceUtils.PhenomenaColor, RadianceUtils.PhenomenaColorDark),
                    new CustomTextSnippet("of unknown meaning and origin. NEWLINE", Color.White, Color.Black),
                    RadianceUtils.phenomenaSnippet,
                    new CustomTextSnippet("are any objects or happenings related to the", Color.White, Color.Black),
                    new CustomTextSnippet("void.", RadianceUtils.VoidColor, RadianceUtils.VoidColorDark),
                    new CustomTextSnippet("NEWLINE NEWLINE", Color.White, Color.Black),
                    new CustomTextSnippet("Within this section you will find the limited, scarce info about the workings of the", Color.White, Color.Black),
                    new CustomTextSnippet("void.", RadianceUtils.VoidColor, RadianceUtils.VoidColorDark),
                }
            });
            AddToEntry(this, new CategoryPage() { category = EntryCategory.Phenomena });
        }
    }
}