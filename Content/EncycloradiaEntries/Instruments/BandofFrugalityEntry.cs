using Radiance.Content.Items.Accessories;
using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries.Instruments
{
    public class BandofFrugalityEntry : EncycloradiaEntry
    {
        public BandofFrugalityEntry()
        {
            displayName = "Band of Frugality";
            tooltip = "Buy six and two thirds to get one free!";
            fastNavInput = "RLRL";
            incomplete = UnlockCondition.unlockedByDefault;
            unlock = UnlockCondition.unlockedByDefault;
            category = EntryCategory.Instruments;
            icon = ModContent.ItemType<RingofFrugality>();
            visible = EntryVisibility.Visible;

            AddPageToEntry(new TextPage()
            {
                text =
                @"One of the downfalls with early \y Radiance \r production is how limited it is, considering the few stars that you will have and are able to gather in one night. If you find yourself constantly running out of supply for your \n Instruments, \r then you may have use for the \b Band of Frugality, \r | " +
                @"The emblemless jewelry may look plain, but when equipped as an accessory, the \b Band of Frugality \r will act as an intermediary between your \y Radiance \r Cells and your \n Instruments, \r improving the efficiency of consumed \y Radiance \r by roughly fifteen percent. "
            });
            AddPageToEntry(new TransmutationPage() { recipe = TransmutationRecipeSystem.FindRecipe(nameof(RingofFrugality)) });
        }
    }
}