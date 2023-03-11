using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Radiance.Core.Interfaces;

namespace Radiance.Content.Items.ProjectorLenses
{
    public class ShimmeringGlass : ModItem, IProjectorLens
    {
        ProjectorLensID IProjectorLens.ID => ProjectorLensID.Flareglass;
        int IProjectorLens.DustID => DustID.GoldFlame;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flareglass");
            Tooltip.SetDefault("'Glimmers in the light'");
            SacrificeTotal = 20;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 26;
            Item.maxStack = 999;
            Item.value = Item.sellPrice(0, 0, 4);
            Item.rare = ItemRarityID.Blue;
        }
    }
}