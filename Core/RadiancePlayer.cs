using Radiance.Content.Items.BaseItems;

namespace Radiance.Core
{
    public static class RadiancePlayerExtensionMethods
    {
        public static float GetRadianceDiscount(this Player player) => player.GetModPlayer<RadiancePlayer>().RadianceMultiplier;   
        public static bool ConsumeRadianceOnHand(this Player player, float amount) => player.GetModPlayer<RadiancePlayer>().ConsumeRadianceOnHand(amount);
        public static bool HasRadiance(this Player player, float consumeAmount) => player.GetModPlayer<RadiancePlayer>().storedRadianceOnHand >= consumeAmount * player.GetRadianceDiscount();
    }
    public partial class RadiancePlayer : ModPlayer
    {
        public bool debugMode = false;
        public bool canSeeRays = false;
        public bool alchemicalLens = false;
        public float dashTimer = 0;
        /// <summary>
        /// The amount of Radiance that the player currently has on them. Set this value with <see cref="ConsumeRadianceOnHand"/>
        /// </summary>
        public float storedRadianceOnHand { get; private set; }
        public float maxRadianceOnHand { get; private set; }
        /// <summary>
        /// The multiplier of Radiance consumed by Instruments
        /// </summary>
        private float _radianceMultiplier; 
        public float RadianceMultiplier
        {
            get => Math.Max(0.1f, _radianceMultiplier);
            set => _radianceMultiplier = value;
        }

        public enum FakePlayerType
        {
            None,   
            ExtractinatorSuite,
        }
        public FakePlayerType fakePlayerType;
        public override void Load()
        {
            PostUpdateEquipsEvent += UpdateDashTimer;
            LoadEvents();
            LoadOverheal();
        }
        public override void Unload()
        {
            PostUpdateEquipsEvent -= UpdateDashTimer;
            UnloadEvents();
            UnloadOverheal();
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
            _radianceMultiplier = 1;
        }

        public override void UpdateDead()
        {
            debugMode = false;
            canSeeRays = false;
            alchemicalLens = false;
            _radianceMultiplier = 1;
            maxRadianceOnHand = 0;
            storedRadianceOnHand = 0;
        }
        public override void PreUpdate()
        {
            maxRadianceOnHand = 0;
            storedRadianceOnHand = 0;
            for (int i = 0; i < 58; i++)
            {
                if (Player.inventory[i].ModItem is BaseContainer cell && cell.canAbsorbItems)
                {
                    maxRadianceOnHand += cell.maxRadiance;
                    storedRadianceOnHand += cell.storedRadiance;
                }
            }
        }
        public bool ConsumeRadianceOnHand(float consumedAmount)
        {
            float radianceLeft = consumedAmount * Player.GetRadianceDiscount();
            if (storedRadianceOnHand >= radianceLeft)
            {
                for (int i = 0; i < 58; i++)
                {
                    if (Player.inventory[i].ModItem is BaseContainer cell && cell.storedRadiance > 0)
                    {
                        float minus = Math.Clamp(cell.storedRadiance, 0, radianceLeft);
                        cell.storedRadiance -= minus;
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