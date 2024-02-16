using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Content.EncycloradiaEntries
{
    public class TransmutationEntry : CategoryEntry
    {
        public TransmutationEntry() : base("Transmutation", EntryCategory.Transmutation)
        {
            AddPageToEntry(new TextPage()
            {
                text =
                    "A pair of &tobjects,&r one greater than the other.&n&n" +
                    "&tTransmutation&r is the process of converting one item into another via a concentrated infusion of &yRadiance.&r&n&n" +
                    "Within this section you will find information about &ttransmutating&r items with the aptly named Transmutator."
            });
        }
    }
}