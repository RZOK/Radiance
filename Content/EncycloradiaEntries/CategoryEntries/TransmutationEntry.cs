using Microsoft.Xna.Framework;
using Radiance.Core;
using Radiance.Utilities;
using Terraria.ID;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Content.EncycloradiaEntries
{
    public class TransmutationEntry : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Transmutation;
            visible = false;
        }

        public override void PageAssembly()
        {
            AddToEntry(this,
            new TextPage()
            {
                text = 
                @"A pair of \t objects, \r one greater than the other. | " +
                @"\t Transmutation \r is the process of converting one item into another via a concentrated infusion of \y Radiance. \r | " +
                @"Within this section you will find information about \t transmutating \r items with the aptly named Transmutator." 
            });
            AddToEntry(this, new CategoryPage() { category = EntryCategory.Transmutation });
        }
    }
}