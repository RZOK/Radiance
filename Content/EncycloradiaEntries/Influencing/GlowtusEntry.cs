using Radiance.Content.Tiles;
using System.Collections.Generic;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Content.EncycloradiaEntries.Influencing
{
    public class GlowstalkEntry : EncycloradiaEntry
    {
        public GlowstalkEntry()
        {
            displayName = "Glowstalks";
            tooltip = "Green light";
            fastNavInput = "UDRR";
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Influencing;
            icon = ModContent.ItemType<GlowstalkItem>();
            visible = true;

            AddToEntry(this, new TextPage()
            {
                text =
                @"Fallen Stars are great, but not without their flaws. They only fall at night, and if you try and throw any to the ground during daytime, they all vanish into a puff of stardust. These problems prove to be definite issues for anyone trying to create \y Radiance. \r | " +
                @"There exists, however, another way of creating \y Radiance, \r through the use of \b Glowstalks \r — unique herbs that grow naturally at extremely high elevations atop \b unnatural floating landmasses. \r | " +
                @"The strange, starry flowers innately carry trace amounts of \y Radiance \r within their structure, allowing them to be disassembled by \y Radiance \r Cells into the glowing goodness, albeit at a faster rate, but with less produced than Fallen Stars. | " +
                @"Like all other usual herbs, \b Glowstalks \r have their own seeds that drop when they are blooming, which may be planted on grass, pots, or Planter Boxes. | " +
                @"The conditions for a \b Glowstalk \r to bloom are strange. The exact molecular alignment between a plant and specific stars will determine whether it gets primed by daytime, or nighttime. | " +
                @"A decorative, hanging potted variant of the flower also exists, if it is to your liking for aesthetical purposes."
            });
            AddToEntry(this, new RecipePage()
            {
                items = new Dictionary<int, int>()
                {
                    { ModContent.ItemType<GlowstalkItem>(), 1 },
                    { ItemID.PotSuspended, 1 }
                },
                station = GetItem(ItemID.None),
                result = GetItem(ModContent.ItemType<HangingGlowstalkItem>()),
            });
        }
    }
}