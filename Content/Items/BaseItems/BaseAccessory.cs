namespace Radiance.Content.Items.BaseItems
{
    public abstract class BaseAccessory : ModItem
    {
        public override sealed void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<BaseAccessoryPlayer>().accessories[FullName] = this;
            SafeUpdateAccessory(player, hideVisual);
        }

        public virtual void SafeUpdateAccessory(Player player, bool hideVisual)
        { }
    }

    public static class BaseAccessoryPlayerExtensions
    {
        public static bool Equipped<T>(this Player player) where T : BaseAccessory => player.GetModPlayer<BaseAccessoryPlayer>().accessories[ItemLoader.GetItem(ModContent.ItemType<T>()).FullName] is not null;

        public static T GetEquipped<T>(this Player player) where T : BaseAccessory => (T)player.GetModPlayer<BaseAccessoryPlayer>().accessories[ItemLoader.GetItem(ModContent.ItemType<T>()).FullName];

        public static bool TryGetEquipped<T>(this Player player, out T accessory) where T : BaseAccessory
        {
            accessory = (T)player.GetModPlayer<BaseAccessoryPlayer>().accessories[ItemLoader.GetItem(ModContent.ItemType<T>()).FullName];
            return accessory is not null;
        }

        public static float GetTimer<T>(this Player player, int timerNumber = 0) where T : ModItem =>
            player.GetModPlayer<BaseAccessoryPlayer>().timers[ItemLoader.GetItem(ModContent.ItemType<T>()).FullName + timerNumber];

        public static void SetTimer<T>(this Player player, int value, int timerNumber = 0) where T : ModItem =>
            player.GetModPlayer<BaseAccessoryPlayer>().timers[ItemLoader.GetItem(ModContent.ItemType<T>()).FullName + timerNumber] = value;

        public static void IncrementTimer<T>(this Player player, int amount = 1, int timerNumber = 0) where T : ModItem =>
            player.GetModPlayer<BaseAccessoryPlayer>().timers[ItemLoader.GetItem(ModContent.ItemType<T>()).FullName + timerNumber] += amount;
    }

    public class BaseAccessoryPlayer : ModPlayer
    {
        public Dictionary<string, BaseAccessory> accessories;
        public Dictionary<string, float> timers;

        public override void Initialize()
        {
            accessories = new Dictionary<string, BaseAccessory>();
            timers = new Dictionary<string, float>();

            foreach (Item item in ContentSamples.ItemsByType.Values)
            {
                if (item.ModItem is BaseAccessory baseAccessory && !baseAccessory.GetType().IsAbstract)
                {
                    accessories.Add(baseAccessory.FullName, null);
                    if (item.ModItem is IModPlayerTimer modPlayerTimer && !modPlayerTimer.GetType().IsAbstract)
                    {
                        for (int i = 0; i < modPlayerTimer.timerCount; i++)
                        {
                            timers.Add(baseAccessory.FullName + i.ToString(), 0);
                        }
                    }
                }
            }
        }

        public override void ResetEffects()
        {
            accessories.Keys.ToList().ForEach(x => accessories[x] = null);
        }

        public override void UpdateDead() => ResetEffects();

        public override void Unload()
        {
            accessories = null;
        }
    }
}