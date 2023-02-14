using Terraria.ModLoader;
using Radiance.Content.Items.ProjectorLenses;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using Radiance.Core;
using Radiance.Utilities;
using System.Collections.Generic;
using Terraria.ID;
using Radiance.Content.Items.Tools.Misc;
using Terraria;
using System;
using Radiance.Core.Systems;
using static Radiance.Core.Systems.UnlockSystem;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.PedestalItems;

namespace Radiance.Content.EncycloradiaEntries.Pedestalworks
{
    public class ManipulationCoresEntry : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
            displayName = "Manipulation Cores";
            tooltip = "Material Energy";
            fastNavInput = "DLLL";
            incomplete = UnlockBoolean.downedSkeletron;
            unlock = UnlockBoolean.hardmode;
            category = EntryCategory.Transmutation;
            icon = ModContent.ItemType<FormationCore>();
            visible = true;
        }
        public override void PageAssembly()
        {
            AddToEntry(this, new TextPage() { text = new CustomTextSnippet[] 
            { 
                "Now that you have transformed your world by slaying the Wall of Flesh, a slew of new capabilities are available with the objects now open to your mind. Three such useful creations are the ".BWSnippet(),
                "Orchestration Core, ".DarkColorSnippet(CommonColors.ContextColor),
                "the ".BWSnippet(),
				"Orchestration Core,".DarkColorSnippet(CommonColors.ContextColor),
				"and the ".BWSnippet(),
				"Formation Core. ".DarkColorSnippet(CommonColors.ContextColor),
                "Each core has its own unique interaction with items in the world when powered by a small supply of ".BWSnippet(),
                "Radiance ".DarkColorSnippet(CommonColors.RadianceColor1),
                "atop a pedestal. |".BWSnippet(),
                "Firstly, the ".BWSnippet(),
				"Orchestration Core ".DarkColorSnippet(CommonColors.ContextColor),
			} } );
        }
    }
}
