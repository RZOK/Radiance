using Terraria.ModLoader;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using Radiance.Core.Systems;
using static Radiance.Core.Systems.UnlockSystem;
using Radiance.Content.Items.Accessories;

namespace Radiance.Content.EncycloradiaEntries.Instruments
{
    public class BandofFrugalityEntry : EncycloradiaEntry
    {
        public BandofFrugalityEntry()
        {
            displayName = "Band of Frugality";
            tooltip = "Buy six and two thirds to get one free!";
            fastNavInput = "RLRL";
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Instruments;
            icon = ModContent.ItemType<RingofFrugality>();
            visible = true;

            AddToEntry(this, new TextPage()
            {
                text =
                @"One of the downfalls with early \y Radiance \r production is how limited it is, considering the few stars that you will have and are able to gather in one night. If you find yourself constantly running out of supply for your \n Instruments, \r then you may have use for the \b Band of Frugality, \r | " +
                @"The emblemless jewelry may look plain, but when equipped as an accessory, the \b Band of Frugality \r will act as an intermediary between your \y Radiance \r Cells and your \n Instruments, \r improving the efficiency of consumed \y Radiance \r by roughly fifteen percent. "
            });
            AddToEntry(this, new TransmutationPage() { recipe = TransmutationRecipeSystem.FindRecipe("RingofFrugality_0") });
        }
    }
}
