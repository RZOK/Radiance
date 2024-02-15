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
                    @"A \d spire, \r inside of which rests a treasure of power. | " +
                    @"\d Pedestalworks \r is the art of placing objects upon an arcane pedestal and watching as an action is performed, typically in exchange for \y Radiance. \r | " +
                    @"Within this section you will find most objects that have a function when placed upon a pedestal."
            });
        }
    }
}