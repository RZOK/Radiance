using Radiance.Content.Items.BaseItems;

namespace Radiance.Core
{
    public static class RadiancePlayerExtensionMethods
    {
        public static float GetRadianceDiscount(this Player player) => 1f - Math.Min(0.9f, player.GetModPlayer<RadiancePlayer>().radianceDiscount);   
        public static bool ConsumeRadianceOnHand(this Player player, float amount) => player.GetModPlayer<RadiancePlayer>().ConsumeRadianceOnHand(amount);
        public static bool HasRadiance(this Player player, float consumeAmount) => player.GetModPlayer<RadiancePlayer>().currentRadianceOnHand >= consumeAmount * player.GetRadianceDiscount();
    }
    public partial class RadiancePlayer : ModPlayer
    {
        public bool debugMode = false;
        public bool canSeeRays = false;
        public bool alchemicalLens = false;
        public float dashTimer = 0;
        /// <summary>
        /// Do NOT try to consume Radiance by changing currentRadianceOnHand directly. Use ConsumeRadianceOnHand(float consumedAmount) from RadiancePlayer.cs instead.
        /// </summary>
        public float currentRadianceOnHand { get; private set; }
        public float maxRadianceOnHand { get; private set; }
        /// <summary>
        /// Do NOT try to get Radiance discount by reading directly from radianceDiscount. Use player.GetRadianceDiscount() intead.
        /// </summary>
        public float radianceDiscount { internal get; set; }

        public enum FakePlayerType
        {
            None,   
            Extractinator,
        }
        public FakePlayerType fakePlayerType;
        public override void Load()
        {
            PostUpdateEquipsEvent += UpdateDashTimer;
            LoadEvents();
        }
        public override void Unload()
        {
            PostUpdateEquipsEvent -= UpdateDashTimer;
            UnloadEvents();
        }

        private void UpdateDashTimer(Player player)
        {
            if (player.GetModPlayer<RadiancePlayer>().dashTimer > 0)
                player.GetModPlayer<RadiancePlayer>().dashTimer--;
        }

        public override void ResetEffects()
        {
            debugMode = false;
            canSeeRays = false;
            alchemicalLens = false;
            radianceDiscount = 0;
        }

        public override void UpdateDead()
        {
            debugMode = false;
            canSeeRays = false;
            alchemicalLens = false;
            radianceDiscount = 0;
        }
        public override void PreUpdate()
        {
            maxRadianceOnHand = 0;
            currentRadianceOnHand = 0;
            for (int i = 0; i < 58; i++)
            {
                if (Player.inventory[i].ModItem is BaseContainer cell && cell.mode != BaseContainer.ContainerMode.InputOnly)
                {
                    maxRadianceOnHand += cell.maxRadiance;
                    currentRadianceOnHand += cell.currentRadiance;
                }
            }
        }

        public bool ConsumeRadianceOnHand(float consumedAmount)
        {
            float radianceLeft = consumedAmount * Player.GetRadianceDiscount();
            if (currentRadianceOnHand >= radianceLeft)
            {
                for (int i = 0; i < 58; i++)
                {
                    if (Player.inventory[i].ModItem is BaseContainer cell && cell.currentRadiance > 0)
                    {
                        float minus = Math.Clamp(cell.currentRadiance, 0, radianceLeft);
                        cell.currentRadiance -= minus;
                        radianceLeft -= minus;
                    }
                    if (radianceLeft == 0)
                        return true;
                }
            }
            return false;
        }
        public override void FrameEffects()
        {
            if (dashTimer > 10)
                Player.armorEffectDrawShadow = true;
        }
    }
}