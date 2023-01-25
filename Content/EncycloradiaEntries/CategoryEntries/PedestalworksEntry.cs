using Microsoft.Xna.Framework;
using Radiance.Core;
using Radiance.Core.Systems;
using Radiance.Utilities;
using Terraria.ID;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Content.EncycloradiaEntries
{
    public class PedestalworksEntry : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Pedestalworks;
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
                    new CustomTextSnippet("spire,", CommonColors.PedestalworksColor, CommonColors.PedestalworksColorDark),
                    new CustomTextSnippet("inside of which rests a treasure of power. |", Color.White, Color.Black),
                    RadianceUtils.pedestalworksSnippet,
                    new CustomTextSnippet("is the art of placing objects upon an arcane pedestal and watching as an action is performed, typically in exchange for", Color.White, Color.Black),
                    RadianceUtils.radianceSnippetPeriod,
                    new CustomTextSnippet("|", Color.White, Color.Black),
                    new CustomTextSnippet("Within this section you will find most objects that have a function when placed upon a pedestal.", Color.White, Color.Black)
                }
            });
            AddToEntry(this, new CategoryPage() { category = EntryCategory.Pedestalworks });
        }
    }
}