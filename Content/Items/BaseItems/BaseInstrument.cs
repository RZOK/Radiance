using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Core;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Utils;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Radiance.Content.Items.BaseItems
{
    public abstract class BaseInstrument : ModItem
    {
        public abstract float CosumeAmount { get; set; }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            foreach (TooltipLine tooltip in tooltips)
            {
                if (tooltip.Name == "Tooltip0")
                {
                    tooltip.Text = "Consumes Radiance from cells in your inventory"; ;
                }
            }
        }
    }
}