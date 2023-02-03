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
                    CommonSnippets.BWSnippet("A"),
                    new CustomTextSnippet("spire,", CommonColors.PedestalworksColor, CommonColors.PedestalworksColor.GetDarkColor()),
                    CommonSnippets.BWSnippet("inside of which rests a treasure of power. |"),
                    CommonSnippets.pedestalworksSnippet,
                    CommonSnippets.BWSnippet("is the art of placing objects upon an arcane pedestal and watching as an action is performed, typically in exchange for"),
                    CommonSnippets.radianceSnippetPeriod,
                    CommonSnippets.BWSnippet("|"),
                    CommonSnippets.BWSnippet("Within this section you will find most objects that have a function when placed upon a pedestal.")
                }
            });
            AddToEntry(this, new CategoryPage() { category = EntryCategory.Pedestalworks });
        }
    }
}