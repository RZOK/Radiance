using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Content.EncycloradiaEntries
{
    public class TransmutationEntry : CategoryEntry
    {
        public TransmutationEntry() : base(EntryCategory.Transmutation)
        {
            pages = [new TextPage()];
        }
    }
}