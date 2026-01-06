using Radiance.Content.Items.BaseItems;
using System.Reflection;

namespace Radiance.Core
{
    public static class RadiancePlayerExtensionMethods
    {
        public static float RadianceMultiplier(this Player player) => player.GetModPlayer<RadiancePlayer>().RadianceMultiplier;

        public static bool ConsumeRadiance(this Player player, float amount) => player.GetModPlayer<RadiancePlayer>().ConsumeRadiance(amount);

        public static bool HasRadiance(this Player player, float consumeAmount) => player.GetModPlayer<RadiancePlayer>().StoredRadianceOnHand >= consumeAmount * player.RadianceMultiplier();
    }

    public partial class RadiancePlayer : ModPlayer
    {
        public bool debugMode = false;
        public bool alchemicalLens = false;
        public float dashDuration = 0;

        private static List<Item> ConsumedItems;
        public Dictionary<int, int> itemsUsedInLastCraft = new Dictionary<int, int>();
        public PlayerDeathReason lastHitSource;
        public int LastUsedReturnType = ItemID.PotionOfReturn;

        /// <summary>
        /// The amount of Radiance that the player currently has on them. Decrease this value with <see cref="ConsumeRadiance"/>
        /// </summary>
        public float StoredRadianceOnHand { get; private set; }

        public float MaxRadianceOnHand { get; private set; }

        private float _radianceMultiplier;

        /// <summary>
        /// The multiplier of Radiance consumed by Instruments
        /// </summary>
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

            ConsumedItems = (List<Item>)typeof(RecipeLoader).GetField("ConsumedItems", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            On_Recipe.Create += MarkItemsUsedForCraft;
        }

        public override void Unload()
        {
            UnloadEvents();
            UnloadOverheal();
        }

        public override void ResetEffects()
        {
            debugMode = false;
            alchemicalLens = false;
            _radianceMultiplier = 1;
        }

        public override void UpdateDead()
        {
            debugMode = false;
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
            if (dashDuration > 10)
                Player.armorEffectDrawShadow = true;
        }

        /// <summary>
        /// Attempts to consume Radiance from the containers in the player's inventory./>
        /// </summary>
        /// <param name="consumedAmount">The amount of Radiance to consume.</param>
        /// <returns>Whether there was sufficient Radiance in the player's inventory and it was consumed.</returns>
        public bool ConsumeRadiance(float consumedAmount)
        {
            float radianceLeft = consumedAmount * Player.RadianceMultiplier();
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

        private void MarkItemsUsedForCraft(On_Recipe.orig_Create orig, Recipe self)
        {
            orig(self);
            RadiancePlayer rp = Main.LocalPlayer.GetModPlayer<RadiancePlayer>();
            foreach (Item item in ConsumedItems)
            {
                if (!rp.itemsUsedInLastCraft.ContainsKey(item.type))
                    rp.itemsUsedInLastCraft[item.type] = item.stack;
                else
                    rp.itemsUsedInLastCraft[item.type] += item.stack;
            }
        }
    }
}