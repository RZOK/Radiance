﻿using Radiance.Content.Items;
using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.EncycloradiaEntries.Phenomena
{
    public class HardLightEntry : EncycloradiaEntry
    {
        public HardLightEntry()
        {
            fastNavInput = "RRUU";
            incomplete = UnlockCondition.unlockedByDefault;
            unlock = UnlockCondition.unlockedByDefault;
            category = EntryCategory.Phenomena;
            icon = ModContent.ItemType<KnowledgeScroll>();
            visible = EntryVisibility.Visible;

            pages = [new TextPage()];
        }
    }
}