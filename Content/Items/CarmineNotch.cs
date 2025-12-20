using Terraria.UI;
using Microsoft.Xna.Framework.Input;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.Tools.Misc;

namespace Radiance.Content.Items
{
    public class CarmineNotch : ModItem
    {
        public override void Load()
        {
            RadiancePlayer.KillEvent += AllowDeathRecall;
        }
        public override void Unload()
        {
            RadiancePlayer.KillEvent -= AllowDeathRecall;
        }
        private void AllowDeathRecall(Player player, double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            player.GetModPlayer<CarmineNotch_Player>().canDeathRecall = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Carmine Notch");
            Tooltip.SetDefault("Placeholder Text");
            Item.ResearchUnlockCount = 0;
            LookingGlassNotchData.LoadNotchData
                    (
                    Type,
                    new Color(255, 102, 150),
                    $"{nameof(Radiance)}/Content/ExtraTextures/LookingGlass/LookingGlass_Death",
                    $"{nameof(Radiance)}/Content/ExtraTextures/LookingGlass/LookingGlass_Death_Small",
                    MirrorUse,
                    ChargeCost
                    );
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.LightRed;
        }
        public void MirrorUse(Player player, LookingGlass lookingGlass)
        {
            CarmineNotch_Player cNPlayer = player.GetModPlayer<CarmineNotch_Player>();
            if (cNPlayer.canDeathRecall)
            {
                lookingGlass.PreRecallParticles(player);
                player.Teleport(player.lastDeathPostion - new Vector2(player.width, player.height) / 2f, 12);
                cNPlayer.canDeathRecall = false;
                lookingGlass.PostRecallParticles(player);
            }
        }

        public int ChargeCost(int identicalCount)
        {
            return 10;
        }
    }
    public class CarmineNotch_Player : ModPlayer
    {
        public bool canDeathRecall = false;
    }
}