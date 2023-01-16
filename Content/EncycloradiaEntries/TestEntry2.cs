using Terraria.GameContent;
using Terraria.ID;
using static Radiance.Core.Systems.UnlockSystem;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;

namespace Radiance.Content.EncycloradiaEntries
{
    public class TestEntry2 : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
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
