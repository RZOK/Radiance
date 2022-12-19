using Terraria.GameContent;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using Radiance.Content.Items.RadianceCells;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.ProjectorLenses;
using static Radiance.Core.Systems.UnlockSystem;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using Radiance.Core;
using Radiance.Utilities;
using Microsoft.Xna.Framework;
using Terraria;

namespace Radiance.Content.EncycloradiaEntries
{
    public class TestEntry : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
            name = "TestEntry";
            displayName = "Test Entry";
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
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
                text = new CustomTextSnippet[] { new CustomTextSnippet("Test Entry", Main.DiscoColor, Main.DiscoColor * 0.2f),}
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
