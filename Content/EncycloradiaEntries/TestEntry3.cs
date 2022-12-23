using Terraria.GameContent;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using Radiance.Content.Items.RadianceCells;
using Radiance.Content.Items.BaseItems;
using static Radiance.Core.Systems.UnlockSystem;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using Radiance.Core.Systems;
using static Radiance.Core.Systems.TransmutationRecipeSystem;

namespace Radiance.Content.EncycloradiaEntries
{
    public class TestEntry3 : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
            displayName = "Test Entry";
            incomplete = UnlockBoolean.downedCultist;
            unlock = UnlockBoolean.downedCultist;
            category = EntryCategory.Influencing;
            icon = ItemID.ManaCrystal;
            visible = true;
        }
        public override void PageAssembly()
        {
            AddToEntry(this,
            new TextPage()
            {
                //text = new(new Terraria.UI.Chat.TextSnippet("Wawa"))
            });
            AddToEntry(this, new ImagePage()
            {
                texture = TextureAssets.Item[ItemID.ManaCrystal].Value
            });
        }
    }
}
