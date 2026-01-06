using Radiance.Content.Items.Materials;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Tiles.Pedestals;
using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries
{
    public class PedestalEntry : EncycloradiaEntry
    {
        public PedestalEntry()
        {
            fastNavInput = "DLUR";
            incomplete = UnlockCondition.UnlockedByDefault;
            unlock = UnlockCondition.UnlockedByDefault;
            category = EntryCategory.Apparatuses;
            visible = EntryVisibility.Visible;
            icon = ModContent.ItemType<RadiantPedestalItem>();

            pages = [
                new TextPage(),
                new RecipePage(new List<(List<(int item, int stack)> items, int station, int result, int resultStack, string extras)>
                {
                    (new List<(int, int)>()
                    {
                        (ItemID.Granite, 10),
                        (ModContent.ItemType<ShimmeringGlass>(), 2)
                    },
                    ItemID.IronAnvil,
                    ModContent.ItemType<GranitePedestalItem>(),
                    1,
                    string.Empty),

                    (new List<(int, int)>()
                    {
                        (ItemID.Marble, 10),
                        (ModContent.ItemType<ShimmeringGlass>(), 2)
                    },
                    ItemID.IronAnvil,
                    ModContent.ItemType<MarblePedestalItem>(),
                    1,
                    string.Empty),

                    (new List<(int, int)>()
                    {
                        (ItemID.SilverBar, 2),
                        (ModContent.ItemType<ShimmeringGlass>(), 2),
                        (ModContent.ItemType<PetrifiedCrystal>(), 5)
                    },
                    ItemID.IronAnvil,
                    ModContent.ItemType<RadiantPedestalItem>(),
                    1,
                    string.Empty)
                })
            ];
        }
    }
}