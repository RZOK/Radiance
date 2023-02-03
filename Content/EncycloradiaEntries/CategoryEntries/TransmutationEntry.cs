using Microsoft.Xna.Framework;
using Radiance.Core;
using Radiance.Utilities;
using Terraria.ID;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Content.EncycloradiaEntries
{
    public class TransmutationEntry : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Transmutation;
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
                    "A pair of".BWSnippet(),
                    "objects,".DarkColorSnippet(CommonColors.TransmutationColor),
                    "one greater than the other. |".BWSnippet(),
                    CommonSnippets.transmutationSnippet,
                    "is the act of converting one item into another via a concentrated infusion of".BWSnippet(),
                    CommonSnippets.radianceSnippetPeriod,
                    "|".BWSnippet(),
                    "Within this section you will find information about".BWSnippet(),
                    "transmutating".DarkColorSnippet(CommonColors.TransmutationColor),
                    "items with the aptly named Transmutator.".BWSnippet(),
                }
            });
            AddToEntry(this, new CategoryPage() { category = EntryCategory.Transmutation });
        }
    }
}