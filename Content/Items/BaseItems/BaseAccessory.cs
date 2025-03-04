namespace Radiance.Content.Items.BaseItems
{
    public abstract class BaseAccessory : ModItem
    {
        public override sealed void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<BaseAccessoryPlayer>().accessories[FullName] = true;
            SafeUpdateAccessory(player, hideVisual);
        }

        public virtual void SafeUpdateAccessory(Player player, bool hideVisual)
        { }
    }

    public static class BaseAccessoryPlayerExtensions
    {
        public static bool Equipped<T>(this Player player) where T : BaseAccessory => player.GetModPlayer<BaseAccessoryPlayer>().accessories[ItemLoader.GetItem(ModContent.ItemType<T>()).Name];

        public static float GetTimer<T>(this Player player, int timerNumber = 0) where T : ModItem => 
            player.GetModPlayer<BaseAccessoryPlayer>().timers[ItemLoader.GetItem(ModContent.ItemType<T>()).FullName + timerNumber];
        public static void SetTimer<T>(this Player player, int value, int timerNumber = 0) where T : ModItem =>
            player.GetModPlayer<BaseAccessoryPlayer>().timers[ItemLoader.GetItem(ModContent.ItemType<T>()).FullName + timerNumber] = value;
    }

    public class BaseAccessoryPlayer : ModPlayer
    {
        public Dictionary<string, bool> accessories;
        public Dictionary<string, float> timers;

        public override void Initialize()
        {
            accessories = new Dictionary<string, bool>();
            timers = new Dictionary<string, float>();

            Type[] types = typeof(Radiance).Assembly.GetTypes();
            foreach (Type type in types)
            {
                if (type.IsSubclassOf(typeof(BaseAccessory)) && !type.IsAbstract)
                {
                    var item = Activator.CreateInstance(type) as BaseAccessory;
                    accessories.Add(item.FullName, false);
                }
                if (typeof(IModPlayerTimer).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    var impt = Activator.CreateInstance(type) as IModPlayerTimer;
                    var item = Activator.CreateInstance(type) as ModItem;
                    for (int i = 0; i < impt.timerCount; i++)
                    {
                        timers.Add(item.FullName + i.ToString(), 0);
                    }
                }
            }
        }

        public override void ResetEffects() => accessories.Keys.ToList().ForEach(x => accessories[x] = false);

        public override void UpdateDead() => ResetEffects();

        public override void Unload()
        {
            accessories = null;
        }
    }
}