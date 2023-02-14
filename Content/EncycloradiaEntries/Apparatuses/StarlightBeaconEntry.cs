using Terraria.ModLoader;
using Radiance.Content.Items.ProjectorLenses;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using Radiance.Core;
using Radiance.Utilities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ID;
using Terraria;
using System;
using Radiance.Core.Systems;
using static Radiance.Core.Systems.UnlockSystem;
using Radiance.Content.Tiles;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items;
using Radiance.Content.Items.TileItems;

namespace Radiance.Content.EncycloradiaEntries.Apparatuses
{
    public class StarlightBeaconEntry : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
            displayName = "Starcatcher Beacon";
            tooltip = "WHHHHRRRRRRRRRRR—";
            fastNavInput = "RUDD";
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Apparatuses;
            icon = ModContent.ItemType<StarlightBeaconItem>();
            visible = true;
        }
        public override void PageAssembly()
        {
            AddToEntry(this, new TextPage() { text = new CustomTextSnippet[] 
            {
                "Collecting stars every night manually in order to feed your cells is such a tedious process. Surely there must be a better way, right? |".BWSnippet(),
                "The ".BWSnippet(),
                "Starcatcher Beacon ".DarkColorSnippet(CommonColors.ContextColor),
                "is an ".BWSnippet(),
                "Apparatus ".DarkColorSnippet(CommonColors.ApparatusesColor),
                "that draws in all Fallen Stars in a massive radius, drastically reducing the amount of effort that must be expended in order to create an ample supply of ".BWSnippet(),
                "Radiance. |".DarkColorSnippet(CommonColors.RadianceColor1),
                "The machine requires more than just a supply of ".BWSnippet(),
                "Radiance, ".DarkColorSnippet(CommonColors.RadianceColor1),
                "however. ".BWSnippet(),
                "Souls of Flight ".DarkColorSnippet(CommonColors.ContextColor),
                "must also be manually inserted into the beacon in order to sustain its function. ".BWSnippet(),
                "The amount of souls obtained from a single slain wyvern will, on average, provide enough power to collect Fallen Stars equivalent to one fifth of a".BWSnippet(),
                "Standard Radiance Cell's ".DarkColorSnippet(CommonColors.RadianceColor1),
                "total capacity. |".BWSnippet(),
                "The beacon operates by creating a coalesced mixture of the two required resources and using it in a manner similar to a fishing hook. When the prescence of a Fallen Star is detected in its radius, it launches an incredibly thin lash of the wispy substance towards the target. The ".BWSnippet(),
                "Radiance ".DarkColorSnippet(CommonColors.RadianceColor1),
                "in the line attaches to the amount in the star. The other element — the bits of soul — will try to retain its original shape, which ends up pulling it and the ".BWSnippet(),
                "Radiance ".DarkColorSnippet(CommonColors.RadianceColor1),
                "(and thus the star as well) back towards the source of the line. Unfortunately, the amount of hooking concoction used in an action is not in a stable enough condition to be reutilized, meaning it is entirely consumed. |".BWSnippet(),
                "Because of the secondary requirement to function, the ".BWSnippet(),
                "Starcatcher Beacon ".DarkColorSnippet(CommonColors.ContextColor),
                "acts as more of a means of redirecting where you must put in the effort in order to gather stars, rather than acting as a 'set it and forget it' passive means of ".BWSnippet(),
                "Radiance ".DarkColorSnippet(CommonColors.RadianceColor1),
                "generation. |".BWSnippet(),
                "In the case that you wish to hear the sweet, sweet whirring noise of the beacon deploying every day, or you just like the look of a beam shooting to the sky, you are able to make a non-functional cosmetic variant.".BWSnippet(),
            }
            } );
        }
    }
}
