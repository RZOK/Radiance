using Terraria.GameContent;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using Radiance.Content.Items.RadianceCells;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.ProjectorLenses;
using static Radiance.Core.Systems.UnlockSystem;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.TransmutationRecipeSystem;
using Radiance.Core.Systems;

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
