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
                    new CustomTextSnippet("symbol", CommonColors.PhenomenaColor, CommonColors.PhenomenaColorDark),
                    new CustomTextSnippet("of unknown meaning and origin. |", Color.White, Color.Black),
                    RadianceUtils.phenomenaSnippet,
                    new CustomTextSnippet("are any objects or happenings related to the", Color.White, Color.Black),
                    new CustomTextSnippet("void.", CommonColors.VoidColor, CommonColors.VoidColorDark),
                    new CustomTextSnippet("|", Color.White, Color.Black),
                    new CustomTextSnippet("Within this section you will find the limited, scarce info about the workings of the", Color.White, Color.Black),
                    new CustomTextSnippet("void.", CommonColors.VoidColor, CommonColors.VoidColorDark),
                }
            });
            AddToEntry(this, new CategoryPage() { category = EntryCategory.Phenomena });
        }
    }
}