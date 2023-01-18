using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Core;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Utilities;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.ObjectModel;

namespace Radiance.Content.Items.Tools.Misc
{
    public class AlchemicalLens : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Alchemical Lens");
            Tooltip.SetDefault("Reveals the Influental Colors of potions in your inventory");
            SacrificeTotal = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.maxStack = 1;
            Item.value = Item.buyPrice(0, 0, 5, 0);
            Item.rare = ItemRarityID.LightRed;
            Item.useTurn = true;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.shootSpeed = 30;
            Item.shoot = ModContent.ProjectileType<ControlRodProjectile>();
            Item.useStyle = ItemUseStyleID.Shoot;
        }
        public override void PostDrawTooltip(ReadOnlyCollection<DrawableTooltipLine> lines)
        {
            Main.player[Main.myPlayer].GetModPlayer<RadiancePlayer>().alchemicalLens = true;
        }
    }
}