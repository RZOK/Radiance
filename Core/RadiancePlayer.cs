using Radiance.Content.Items.BaseItems;
using System.Reflection;

namespace Radiance.Core
{
    public static class RadiancePlayerExtensionMethods
    {
        public static float GetRadianceDiscount(this Player player) => player.GetModPlayer<RadiancePlayer>().RadianceMultiplier;   
        public static bool ConsumeRadianceOnHand(this Player player, float amount) => player.GetModPlayer<RadiancePlayer>().ConsumeRadianceOnHand(amount);
        public static bool HasRadiance(this Player player, float consumeAmount) => player.GetModPlayer<RadiancePlayer>().StoredRadianceOnHand >= consumeAmount * player.GetRadianceDiscount();
    }
    public partial class RadiancePlayer : ModPlayer
    {
        public bool debugMode = false;
        public bool canSeeRays = false;
        public bool alchemicalLens = false;
        public float dashTimer = 0;

        private static FieldInfo ConsumedItems;
        public Dictionary<int, int> itemsUsedInLastCraft = new Dictionary<int, int>();
        /// <summary>
        /// The amount of Radiance that the player currently has on them. Set this value with <see cref="ConsumeRadianceOnHand"/>
        /// </summary>
        public float StoredRadianceOnHand { get; private set; }
        public float MaxRadianceOnHand { get; private set; }
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
            LoadEvents();
            LoadOverheal();
            ConsumedItems = typeof(RecipeLoader).GetType().GetField("ConsumedItems", BindingFlags.NonPublic | BindingFlags.Static);
            On_Recipe.Create += SaveConsumedItems;
        }

        public override void Unload()
        {
            UnloadEvents();
            UnloadOverheal();
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
            MaxRadianceOnHand = 0;
            StoredRadianceOnHand = 0;
        }
        public override void PreUpdate()
        {
            MaxRadianceOnHand = 0;
            StoredRadianceOnHand = 0;
            for (int i = 0; i < 58; i++)
            {
                if (Player.inventory[i].ModItem is BaseContainer cell && cell.canAbsorbItems)
                {
                    MaxRadianceOnHand += cell.maxRadiance;
                    StoredRadianceOnHand += cell.storedRadiance;
                }
            }
        }
        public override void FrameEffects()
        {
            if (dashTimer > 10)
                Player.armorEffectDrawShadow = true;
        }
        public bool ConsumeRadianceOnHand(float consumedAmount)
        {
            float radianceLeft = consumedAmount * Player.GetRadianceDiscount();
            if (StoredRadianceOnHand >= radianceLeft)
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
        private void SaveConsumedItems(On_Recipe.orig_Create orig, Recipe self)
        {
            foreach (var item in collection)
            {

            }
        }
    }
}