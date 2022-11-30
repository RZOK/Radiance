using Terraria.GameContent;
using System.Collections.Generic;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using Terraria.ID;
using Terraria.ModLoader;
using Radiance.Content.Items.RadianceCells;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.ProjectorLenses;
using static Radiance.Core.Systems.TransmutationRecipeSystem;
using Radiance.Core.Systems;

namespace Radiance.Core.Encycloradia.Entries
{
    public class TestEntry : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
            name = "TestEntry";
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.None;
            icon = null;
        }
        public override void PageAssembly()
        {
            TransmutationRecipe recipe = TransmutationRecipeSystem.FindRecipe(ItemID.Sapphire.ToString() + "Flareglass");
            List<EncycloradiaPage> list = new()
            {
                new TextPage()
                {
                    number = 0,
                    text = "Wawa"
                },
                new ImagePage()
                {
                    number = 1,
                    texture = TextureAssets.Item[ItemID.ManaCrystal].Value
                },
                new RecipePage()
                {
                    number = 2,
                    items = new Dictionary<int, int>() { { ItemID.Sapphire, 5}, { ItemID.FallenStar, 5 } },
                    station = ItemID.IronAnvil,
                    result = ModContent.ItemType<PoorRadianceCell>()
                },
                new TransmutationPage()
                {
                    number = 3,
                    container = Radiance.Instance.GetContent<StandardRadianceCell>() as BaseContainer,
                    radianceRequired = recipe.requiredRadiance,
                    input = recipe.inputItem,
                    output = recipe.outputItem
                },
            };
            pages = list;
        }
    }
}
