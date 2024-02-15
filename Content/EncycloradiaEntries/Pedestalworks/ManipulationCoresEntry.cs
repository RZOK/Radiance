using Radiance.Content.Items.PedestalItems;
using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries.Pedestalworks
{
    public class ManipulationCoresEntry : EncycloradiaEntry
    {
        public ManipulationCoresEntry()
        {
            displayName = "Manipulation Cores";
            tooltip = "Material Energy";
            fastNavInput = "DLLL";
            incomplete = UnlockCondition.unlockedByDefault;
            unlock = UnlockCondition.unlockedByDefault;
            category = EntryCategory.Pedestalworks;
            icon = ModContent.ItemType<OrchestrationCore>();
            visible = EntryVisibility.Visible;

            AddPageToEntry(new TextPage()
            {
                text =
                @"Now that you have transformed your world by slaying the Wall of Flesh, a slew of new capabilities are available with the knowledge now open to your mind. Three such useful creations are the \b Orchestration Core, \r the \b Annihilation Core, \r and the \b Formation Core. \r Each core has its own unique interaction with items in the world when powered by a small supply of \y Radiance \r atop a pedestal. | " +
                @"Firstly, the \b Orchestration Core \r will act to transport items over distances. If an item is dropped near a pedestal that has an \b Orchestration Core \r atop it, and that pedestal is linked by a ray to output to another pedestal that also has a proper core, the dropped items will quickly be relocated to that pedestal, at the cost of a tiny amount of \y Radiance. \r \b Orchestration Cores \r will attempt to first output to a pedestal linked by the top-right output slot, before trying the bottom-left one. | " +
                @"\b Orchestration Cores \r are able to \b chain \r together, allowing you to instantly send an item to the end of a long sequence of connected cores. Each core in a chain requires the necessary amount of \y Radiance \r and  | " +
                @"\b Orchestration Cores, \r like the rest of the cores, also come with the ability to output \y Radiance, \r meaning you can power an entire line of them without any extra unnecessary rays. | " +
                @"\b Annihilation Cores \r are significantly less useful than their pink sibling, working to remove expendable items from the world. Whenever an item of blue rarity or lower is dropped near the core, it will spend a minimal amount of \y Radiance \r in order to entirely disintegrated it from this plane. | " +
                @"While lava may already serve a similar function to this flavor of core, this one has definite benefits that may not be immediately obvious at first glance. | " +
                @"First, \b Annihilation Cores \r are significantly less messy than lava, as they do not spill into a hot mess over the floor, or pose any risk of harming you. | " +
                @"The ability to toggle the activity of items atop pedestals with wiring also proves to be an advantage, as it allows you to disable it all at once with a single lever flip, as opposed to trying to scoop up the deleting matter with a metal bucket. |" +
                @"Lastly, \b Annihilation Cores \r will delete a slightly higher tier of item, meaning any common drops that are thought to be worth more than they actually are valued at to you will also be destroyed. | " +
                @"The uses of the third core are much more niche than the others, but it has no less reason to be mentioned. The \b Formation Core \r will pick up nearby dropped items and place them onto adjacent, unoccupied inventories. Chests, barrels, and the like do not count as inventories for the core, but things such as Pedestals and Transmutators do. The core will try to prioritize moving an item to an inventory directly to the right of it, before checking for a left one. One idea for a use for the \b Formation Core \r could be to create a cell-feeding contraption that uses wiring to knock off a full Radiance Cell, \r which is then immediately replaced with a new, empty one by using the core on a pedestal directly to the left or right. | " +
                @"All cores have a very small internal \y Radiance \r capacity that they will drawn from, and all will display their effective radius upon hovering over their pedestal."
            });
            AddPageToEntry(new TransmutationPage() { recipe = TransmutationRecipeSystem.FindRecipe("OrchestrationCore") });
            AddPageToEntry(new TransmutationPage() { recipe = TransmutationRecipeSystem.FindRecipe("AnnihilationCore") });
            AddPageToEntry(new TransmutationPage() { recipe = TransmutationRecipeSystem.FindRecipe("FormationCore") });
        }
    }
}