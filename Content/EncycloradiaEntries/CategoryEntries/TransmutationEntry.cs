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
                    CommonSnippets.BWSnippet("A pair of"),
                    new CustomTextSnippet("objects,", CommonColors.TransmutationColor, CommonColors.TransmutationColor.GetDarkColor()),
                    CommonSnippets.BWSnippet("one greater than the other. |"),
                    CommonSnippets.transmutationSnippet,
                    CommonSnippets.BWSnippet("is the act of converting one item into another via a concentrated infusion of"),
                    CommonSnippets.radianceSnippetPeriod,
                    CommonSnippets.BWSnippet("|"),
                    CommonSnippets.BWSnippet("Within this section you will find information about"),
                    new CustomTextSnippet("transmutating", CommonColors.TransmutationColor, CommonColors.TransmutationColor.GetDarkColor()),
                    CommonSnippets.BWSnippet("items with the aptly named Transmutator."),
                }
            });
            AddToEntry(this, new CategoryPage() { category = EntryCategory.Transmutation });
        }
    }
}