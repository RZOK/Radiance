using Radiance.Items.Accessories;
using System.Collections.Specialized;
using System.Linq;

namespace Radiance.Core.Systems
{
    public class MeterPlayer : ModPlayer
    {
        public OrderedDictionary activeMeters = new OrderedDictionary();
        public override void PostUpdate()
        {
            List<MeterInfo> metersToRemove = new List<MeterInfo>();
            foreach (MeterInfo meter in MeterInfo.loadedMeters.Values)
            {
                if (activeMeters.Keys.Cast<MeterInfo>().Contains(meter))
                {
                    MeterVisual visual = (MeterVisual)activeMeters[meter];
                    if (meter.active())
                    {
                        if (visual.timer < MeterVisual.METER_VISUAL_TIMER_MAX)
                            visual.timer++;
                    }
                    else if (visual.timer > 0)
                        visual.timer--;
                    if (visual.timer <= 0)
                        metersToRemove.Add(meter);
                }
                else if(meter.active())
                    activeMeters.Add(meter, new MeterVisual());
            }
            foreach (MeterInfo meter in metersToRemove)
            {
                activeMeters.Remove(meter);
            }
        }
    }

    public struct MeterInfo(string name, Func<bool> active, Func<float> max, Func<float> current, Func<float, Color> colorFunction, Texture2D tex)
    {
        internal static readonly Dictionary<string, MeterInfo> loadedMeters = new Dictionary<string, MeterInfo>();

        public string name = name;
        public Func<bool> active = active;
        public Func<float> max = max;
        public Func<float> current = current;
        public Func<float, Color> colorFunction = colorFunction;
        public Texture2D tex = tex;

        public static void Register(string name, Func<bool> active, Func<float> max, Func<float> current, Func<float, Color> colorFunction, Texture2D tex)
        {
            loadedMeters.Add(name, new MeterInfo(name, active, max, current, colorFunction, tex));
        }
    }

    public class MeterVisual
    {
        public const int METER_VISUAL_TIMER_MAX = 30;
        public float timer = 0;
        public Vector2? position = null;
    }
}