using Terraria.GameContent;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using Radiance.Content.Items.RadianceCells;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.ProjectorLenses;
using static Radiance.Core.Systems.UnlockSystem;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;

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
                text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque tempus turpis vitae iaculis auctor. Proin eu tortor erat. Nam dictum risus molestie, varius leo sed, sodales augue. Nam sodales imperdiet arcu, quis commodo enim blandit et. Suspendisse at quam sit amet est tristique condimentum. Suspendisse sed urna vel justo eleifend vestibulum. Duis nec gravida nibh. Proin tincidunt ac mauris nec egestas. Integer dictum ac lectus sit amet facilisis.\r\n\r\nNam blandit scelerisque odio, et dictum nisi vehicula accumsan. Nam sagittis justo vel mauris faucibus pharetra. Ut eu felis aliquet, imperdiet lorem nec, dapibus felis. Fusce aliquet sodales elit nec dignissim. Vestibulum pulvinar nunc at dui interdum facilisis. Sed purus neque, aliquet et lobortis id, imperdiet et nisi. Pellentesque fringilla metus a mi pharetra, id cursus ex tincidunt. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Interdum et malesuada fames ac ante ipsum primis in faucibus. "
            });
            AddToEntry(this, new MiscPage()
            {
                type = "Title"
            });
        }
    }
}
