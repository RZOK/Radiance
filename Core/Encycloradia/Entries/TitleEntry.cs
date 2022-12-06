using Terraria.GameContent;
using Microsoft.Xna.Framework;
using Terraria.ID;
using static Radiance.Core.Systems.UnlockSystem;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using Terraria.UI.Chat;
using Humanizer;

namespace Radiance.Core.Encycloradia.Entries
{
    public class TitleEntry : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
            name = "TitleEntry";
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
                text = new CustomTextSnippet[] { new CustomTextSnippet("Lorem ipsum dolor sit amet, consectetur adipiscing elit.", new Color(255, 0, 103), new Color(85, 0, 34)),
                new CustomTextSnippet("Duis vitae posuere sem. Proin euismod sit amet velit vel fermentum.", new Color(103, 255, 0), new Color(34, 85, 0)),
                new CustomTextSnippet("Integer non magna varius, rhoncus quam id, ullamcorper diam.", new Color(0, 103, 255), new Color(0, 34, 85)),
                new CustomTextSnippet("Aenean dapibus ullamcorper turpis ac scelerisque.", new Color(255, 103, 0), new Color(85, 34, 0)),
                new CustomTextSnippet("In hac habitasse platea dictumst.", new Color(0, 255, 103), new Color(0, 85, 34)),
                }
            });
            AddToEntry(this, new MiscPage()
            {
                type = "Title"
            });
        }
    }
}
