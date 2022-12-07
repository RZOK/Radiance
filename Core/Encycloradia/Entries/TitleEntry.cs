using Terraria.GameContent;
using Microsoft.Xna.Framework;
using Terraria.ID;
using static Radiance.Core.Systems.UnlockSystem;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Utilities.RadianceUtils;
using Terraria;

namespace Radiance.Core.Encycloradia.Entries
{
    public class TitleEntry : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
            displayName = "Title Page";
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Title;
            icon = TextureAssets.Item[ItemID.ManaCrystal].Value;
        }
        public override void PageAssembly()
        {
            //TransmutationRecipe recipe = TransmutationRecipeSystem.FindRecipe(ItemID.Sapphire + "Flareglass"); todo: make this work by fucking with loading i think
            AddToEntry(this, new TextPage()
            {
                text = new CustomTextSnippet[] { new CustomTextSnippet("Welcome to the", Color.White, Color.Black),
                new CustomTextSnippet("Encycloradia. NEWLINE", RadianceColor1, RadianceColorDark),
                new CustomTextSnippet("Click on a category to the right in order to view its entries. NEWLINE", Color.White, Color.Black),
                new CustomTextSnippet("If an entry is", Color.White, Color.Black),
                new CustomTextSnippet("locked,", LockedColor, LockedColorDark),
                new CustomTextSnippet("you will be unable to view it until it is unlocked. NEWLINE NEWLINE", Color.White, Color.Black),
                new CustomTextSnippet("Tip of the Day:", RadianceColor1, RadianceColorDark),
                Tips[Main.rand.Next(Tips.Length)]
                }
            });
            AddToEntry(this, new MiscPage()
            {
                type = "Title"
            });
        }
        public CustomTextSnippet[] Tips = { 
            new CustomTextSnippet("If two rays intersect, they will both glow red and have a significantly reduced transfer rate. Plan around this!", Color.White, Color.Black),
            new CustomTextSnippet("Most apparatuses will cease to function if powered wire is running through them.", Color.White, Color.Black)
        };
    }
}
