namespace Radiance.Core.Systems
{
    public class MeterPlayer : ModPlayer
    {
        public Dictionary<MeterInfo, MeterVisual> activeMeters = new Dictionary<MeterInfo, MeterVisual>();
        // example meter
        //public override void Load()
        //{
        //    Texture2D tex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/ExtraTextures/TestMeterIcon", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        //    MeterInfo.Register("TestMeter2", () => true, () => 30f, () => Main.GameUpdateCount / 16 % 90f, (percentComplete) => 
        //    {
        //        Main.NewText(percentComplete);
        //        if(percentComplete < 1)
        //            return Color.Lerp(CommonColors.InfluencingColor, CommonColors.TransmutationColor, percentComplete);
        //        if (percentComplete < 2)
        //            return Color.Lerp(CommonColors.TransmutationColor, CommonColors.ApparatusesColor, percentComplete - 1f);
        //        return Color.Lerp(CommonColors.ApparatusesColor, CommonColors.InstrumentsColor, percentComplete - 2f);
        //    }
        //    , tex);
        //}
        public override void PostUpdate()
        {
            foreach (MeterInfo meter in MeterInfo.loadedMeters.Values)
            {
                if (activeMeters.TryGetValue(meter, out MeterVisual meterVisual))
                {
                    if (meter.active())
                    {
                        if (meterVisual.timer < MeterVisual.METER_VISUAL_TIMER_MAX)
                            meterVisual.timer++;
                    }
                    else if (meterVisual.timer > 0)
                        meterVisual.timer--;
                }
                else if(meter.active())
                    activeMeters.Add(meter, new MeterVisual());
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
        public Vector2 position;
    }
}