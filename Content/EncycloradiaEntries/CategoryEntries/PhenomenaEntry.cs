using Microsoft.Xna.Framework;
using Radiance.Core;
using Radiance.Core.Systems;
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
                    CommonSnippets.BWSnippet("A strange, mistifying"),
                    new CustomTextSnippet("symbol", CommonColors.PhenomenaColor, CommonColors.PhenomenaColorDark),
                    CommonSnippets.BWSnippet("of unknown meaning and origin. |"),
                    CommonSnippets.phenomenaSnippet,
                    CommonSnippets.BWSnippet("are any objects or happenings related to the"),
                    new CustomTextSnippet("void.", CommonColors.VoidColor, CommonColors.VoidColorDark),
                    CommonSnippets.BWSnippet("|"),
                    CommonSnippets.BWSnippet("Within this section you will find the limited, scarce info about the workings of the"),
                    new CustomTextSnippet("void.", CommonColors.VoidColor, CommonColors.VoidColorDark),
                }
            });
            AddToEntry(this, new CategoryPage() { category = EntryCategory.Phenomena });
        }
    }
}