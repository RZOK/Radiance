using Terraria.GameContent;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using Radiance.Content.Items.RadianceCells;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.ProjectorLenses;
using static Radiance.Core.Systems.UnlockSystem;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;

namespace Radiance.Content.EncycloradiaEntries
{
    public class TestEntry2 : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
            name = "TestEntry2";
            displayName = "Test Entry";
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.downedCultist;
            category = EntryCategory.Influencing;
            icon = ItemID.ManaCrystal;
            visible = true;
        }
        public override void PageAssembly()
        {
            //TransmutationRecipe recipe = TransmutationRecipeSystem.FindRecipe(ItemID.Sapphire + "Flareglass"); todo: make this work by fucking with loading i think
            AddToEntry(this,
            new TextPage()
            {
                //text = new(new Terraria.UI.Chat.TextSnippet("Wawa"))
            });
            AddToEntry(this, new ImagePage()
            {
                texture = TextureAssets.Item[ItemID.ManaCrystal].Value
            });
            AddToEntry(this, new RecipePage()
            {
                items = new Dictionary<int, int>() { { ItemID.Sapphire, 5 }, { ItemID.FallenStar, 5 } },
                station = ItemID.IronAnvil,
                result = ModContent.ItemType<PoorRadianceCell>()
            });
            AddToEntry(this, new TransmutationPage()
            {
                container = Radiance.Instance.GetContent<StandardRadianceCell>() as BaseContainer,
                radianceRequired = 500 /*recipe.requiredRadiance*/,
                input = ItemID.Amethyst /*recipe.inputItem*/,
                output = ModContent.ItemType<ShimmeringGlass>() /*recipe.outputItem*/
            });
        }
    }
}
