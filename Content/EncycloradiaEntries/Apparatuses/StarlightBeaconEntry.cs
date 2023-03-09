using Radiance.Content.Tiles.StarlightBeacon;
using Terraria.ModLoader;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Content.EncycloradiaEntries.Apparatuses
{
    public class StarlightBeaconEntry : EncycloradiaEntry
    {
        public StarlightBeaconEntry()
        {
            displayName = "Starcatcher Beacon";
            tooltip = "WHHHHRRRRRRRRRRR—";
            fastNavInput = "RUDD";
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Apparatuses;
            icon = ModContent.ItemType<StarlightBeaconItem>();
            visible = true;
            AddToEntry(this, new TextPage()
            {
                text =
                @"Collecting stars every night manually in order to feed your cells is such a tedious process. Surely there must be a better way, right? | " +
                @"The \b Starcatcher Beacon \r is an \a Apparatus \r that draws in all Fallen Stars in a massive radius, drastically reducing the amount of effort that must be expended in order to create an ample supply of \y Radiance. \r | " +
                @"The machine requires more than just a supply of \y Radiance, \r however. \b Souls of Flight \r must also be manually inserted into the beacon in order to sustain its function. \r The amount of souls obtained from a single slain wyvern will, on average, provide enough power to collect Fallen Stars equivalent to one fifth of a \y Standard Radiance Cell's \r total capacity. | " +
                @"The beacon operates by creating a coalesced mixture of the two required resources and using it in a manner similar to a fishing hook. When the prescence of a Fallen Star is detected in its radius, it launches an incredibly thin lash of the wispy substance towards the target. The \y Radiance \r in the line attaches to the amount in the star. The other element — the bits of soul — will try to retain its original shape, which ends up pulling it and the \y Radiance \r (and thus the star as well) back towards the source of the line. Unfortunately, the amount of hooking concoction used in an action is not in a stable enough condition to be reutilized, meaning it is entirely consumed. | " +
                @"Because of the secondary requirement to function, the \b Starcatcher Beacon \r acts as more of a means of redirecting where you must put in the effort in order to gather stars, rather than acting as a 'set it and forget it' passive means of \y Radiance \r generation. | " +
                @"In the case that you wish to hear the sweet, sweet whirring noise of the beacon deploying every day, or you just like the look of a beam shooting to the sky, you are able to make a non-functional cosmetic variant."
            });
        }
    }
}
