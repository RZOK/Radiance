using Microsoft.Xna.Framework;
using Radiance.Core;
using Radiance.Core.Interfaces;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.StabilizationCrystals
{
    public class StabilizationCrystal : ModItem, IStabilizationCrystal
    {
        public string PlacedTexture => "Radiance/Content/Items/StabilizationCrystals/StabilizationCrystalPlaced";
        public int DustID => Terraria.ID.DustID.BlueCrystalShard;
        public int StabilizationRange => 10;
        public int StabilizationLevel => 15;
        public StabilizeType StabilizationType => StabilizeType.Basic;
        public Color CrystalColor => new Color(0, 150, 255);

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Stabilization Crystal");
            Tooltip.SetDefault("Stabilizes nearby Apparatuses when placed atop a Stabilizer");
            Item.ResearchUnlockCount = 3;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 22;
            Item.maxStack = 999;
            Item.value = Item.sellPrice(0, 0, 1);
            Item.rare = ItemRarityID.Blue;
        }
    }
}