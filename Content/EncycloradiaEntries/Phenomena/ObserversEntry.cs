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

namespace Radiance.Content.EncycloradiaEntries.Phenomena
{
    public class ObserversEntry : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
            displayName = "External Observers";
            tooltip = ":)";
            fastNavInput = "DDDD";
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Phenomena;
            icon = ModContent.ItemType<KnowledgeScroll>();
            visible = true;
        }
        public override void PageAssembly()
        {
            AddToEntry(this, new TextPage()
            {
                text =
                @"You are not alone. | " +
                @"Monstrous beings of shadow exist outside of your dimension in a location called [the void]. Creatures adorned with scarlet eyes that look in every direction, and through every object. They are harmless and simply curious as to what this tiny being of flesh is doing, not that they could manage to affect you anyways. Currently, they are unable to fully work through the trans-dimensional barrier that separates them from your plane, leaving them mostly harmless aside from the occasional scare. | " +
                @"The burst of potential energy released through \t Transmutation \r is enough to shine through the ethereal glass hung between dimensions, letting you peer, into a space you should not be seeing, for a fleeting moment. Rarely will you actually notice one of these beings, as they tend to be rather furtive. You would rather forget about the appearance of the monolithic entities, but sometimes peeking beyond the veil is accidental or unavoidable. | " +
                @"Surely there must be some method that you will discover eventually to draw their interest further, if, for some reason, you wish to see the beasts more often… "
            });
        }
    }
}
