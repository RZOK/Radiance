using System.Collections.Generic;
using Terraria.ID;
using Radiance.Core.Systems;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using Radiance.Core;
using Radiance.Utilities;
using Terraria;
using static Radiance.Core.Systems.TransmutationRecipeSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Content.EncycloradiaEntries
{
    public class TestEntry : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
            fastNavInput = "ULDR";
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Influencing;
            icon = ItemID.ManaCrystal;
            visible = true;
        }
        public override void PageAssembly()
        {
            TransmutationRecipe recipe = TransmutationRecipeSystem.FindRecipe("Flareglass" + ItemID.Sapphire); 
            AddToEntry(this,
            new TextPage()
            {
                text = new CustomTextSnippet[] { "Test Page 1".DarkColorSnippet(CommonColors.InfluencingColor),}
            });
            AddToEntry(this,
            new TextPage()
            {
                text = new CustomTextSnippet[] { "Test Page 2".DarkColorSnippet(CommonColors.TransmutationColor), }
            });
            AddToEntry(this,
            new TextPage()
            {
                text = new CustomTextSnippet[] { "Test Page 3".DarkColorSnippet(CommonColors.ApparatusesColor), }
            });
            AddToEntry(this,
            new TextPage()
            {
                text = new CustomTextSnippet[] { "Test Page 4".DarkColorSnippet(CommonColors.InstrumentsColor), }
            });
            AddToEntry(this,
            new TextPage()
            {
                text = new CustomTextSnippet[] { "Test Page 5".DarkColorSnippet(CommonColors.PedestalworksColor), }
            });
            //AddToEntry(this, new ImagePage()
            //{
            //    texture = TextureAssets.Item[ItemID.ManaCrystal].Value
            //});
            //AddToEntry(this, new TransmutationPage()
            //{
            //    container = Radiance.Instance.GetContent<StandardRadianceCell>() as BaseContainer,
            //    radianceRequired = recipe.requiredRadiance,
            //    input = recipe.inputItem,
            //    output = recipe.outputItem
            //});
        }
    }
}
