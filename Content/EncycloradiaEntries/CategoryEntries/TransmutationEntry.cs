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
                    new CustomTextSnippet("A pair of", Color.White, Color.Black),
                    new CustomTextSnippet("objects,", CommonColors.TransmutationColor, CommonColors.TransmutationColorDark),
                    new CustomTextSnippet("one greater than the other. |", Color.White, Color.Black),
                    RadianceUtils.transmutationSnippet,
                    new CustomTextSnippet("is the act of converting one item into another via a concentrated infusion of", Color.White, Color.Black),
                    RadianceUtils.radianceSnippetPeriod,
                    new CustomTextSnippet("|", Color.White, Color.Black),
                    new CustomTextSnippet("Within this section you will find information about", Color.White, Color.Black),
                    new CustomTextSnippet("transmutating", CommonColors.TransmutationColor, CommonColors.TransmutationColorDark),
                    new CustomTextSnippet("items with the aptly named Transmutator.", Color.White, Color.Black),
                }
            });
            AddToEntry(this, new CategoryPage() { category = EntryCategory.Transmutation });
        }
    }
}