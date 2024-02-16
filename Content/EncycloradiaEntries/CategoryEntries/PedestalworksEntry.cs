using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries
{
    public class PedestalworksEntry : CategoryEntry
    {
        public PedestalworksEntry() : base("Pedastalworks", EntryCategory.Pedestalworks)
        {
            AddPageToEntry(new TextPage()
            {
                text =
                    "A &dspire,&r inside of which rests a treasure of power.&n&n" +
                    "&dPedestalworks&r is the art of placing objects upon an arcane pedestal and watching as an action is performed, typically in exchange for &yRadiance.&r&n&n" +
                    "Within this section you will find most objects that have a function when placed upon a pedestal."
            });
        }
    }
}