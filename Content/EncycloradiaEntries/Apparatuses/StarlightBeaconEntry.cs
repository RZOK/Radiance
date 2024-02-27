using Radiance.Content.Tiles.StarlightBeacon;
using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries.Apparatuses
{
    public class StarlightBeaconEntry : EncycloradiaEntry
    {
        public StarlightBeaconEntry()
        {
            displayName = "Starcatcher Beacon";
            tooltip = "WHHHHRRRRRRRRRRR—";
            fastNavInput = "RUDD";
            incomplete = UnlockCondition.unlockedByDefault;
            unlock = UnlockCondition.debugCondition;
            category = EntryCategory.Apparatuses;
            icon = ModContent.ItemType<StarlightBeaconItem>();
            visible = EntryVisibility.Visible;
            AddPageToEntry(new TextPage()
            {
                text =
                @"Collecting stars every night manually in order to feed your cells is such a tedious process. Surely there must be a better way, right?&n&n" +
                @"The &bStarcatcher Beacon &ris an &aApparatus &rthat draws in all Fallen Stars in a massive radius, drastically reducing the amount of effort that must be expended in order to create an ample supply of &yRadiance.&r&n&n" +
                @"The machine requires more than just a supply of &yRadiance, &rhowever. &bSouls of Flight &rmust also be manually inserted into the beacon in order to sustain its function. &rThe amount of souls obtained from a single slain wyvern will, on average, provide enough power to collect Fallen Stars equivalent to one fifth of a &yStandard Radiance Cell's &rtotal capacity.&n&n" +
                @"The beacon operates by creating a coalesced mixture of the two required resources and using it in a manner similar to a fishing hook. When the prescence of a Fallen Star is detected in its radius, it launches an incredibly thin lash of the wispy substance towards the target. The &yRadiance &rin the line attaches to the amount in the star. The other element — the bits of soul — will try to retain its original shape, which ends up pulling it and the &yRadiance &r(and thus the star as well) back towards the source of the line. Unfortunately, the amount of hooking concoction used in an action is not in a stable enough condition to be reutilized, meaning it is entirely consumed.&n&n" +
                @"Because of the secondary requirement to function, the &bStarcatcher Beacon &racts as more of a means of redirecting where you must put in the effort in order to gather stars, rather than acting as a 'set it and forget it' passive means of &yRadiance&r generation.&n&n" +
                @"In the case that you wish to hear the sweet, sweet whirring noise of the beacon deploying every day, or you just like the look of a beam shooting to the sky, you are able to make a non-functional cosmetic variant."
            });
        }
    }
}