using Terraria.GameContent.UI;
using System.Reflection;
using Terraria.UI;
using static Terraria.GameContent.UI.WiresUI;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Radiance.Content.UI
{
    public class RadialUICursorSystem : ModSystem
    {
        public static List<RadialUICursorData> radialUICursorData;
        public static WiresRadial radial;
        public override void Load()
        {
            if (Main.dedServ)
                return;

            radialUICursorData = new List<RadialUICursorData>();
            radial = (WiresRadial)typeof(WiresUI).GetField("radial", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        }
    }
    public class RadialUICursorPlayer : ModPlayer
    {
        public int realCursorItemType;
        public override void Load()
        {
            IL_Main.DrawInterface_40_InteractItemIcon += GetRealCursorItem;
        }
        private void GetRealCursorItem(ILContext il)
        {
            // in the case that the cursor item isn't set manually, it picks the player's held item (when in range)
            // however, it doesn't save that value anywhere
            // so we have to grab it manually when it's drawn

            ILCursor cursor = new ILCursor(il);
            cursor.EmitDelegate<Action>(() => Main.LocalPlayer.GetModPlayer<RadialUICursorPlayer>().realCursorItemType = 0);
            if (!cursor.TryGotoNext(MoveType.After,
               i => i.MatchLdsfld(typeof(Main), nameof(Main.instance)),
               i => i.MatchLdloc1(),
               i => i.MatchCallvirt(typeof(Main), nameof(Main.LoadItem))
               ))
            {
                LogIlError("RadialUIMouseIndicator real cursor item grab", "Couldn't navigate to after instance.LoadItem(num)");
                return;
            }
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.EmitDelegate<Action<int>>((x) => Main.LocalPlayer.GetModPlayer<RadialUICursorPlayer>().realCursorItemType = x);
        }
    }
    public class RadialUICursor : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Wire Selection"));
        public override bool Visible => !Main.gameMenu;
        private float drawOpacity = 0f;
        public List<RadialUIElement> surroundingElements = new List<RadialUIElement>();
        public override void Update(GameTime gameTime)
        {
            bool goOpaque = true;
            Player player = Main.LocalPlayer;
            RadialUICursorPlayer radialUICursorPlayer = Main.LocalPlayer.GetModPlayer<RadialUICursorPlayer>();
            float min = 0.75f;
            List<RadialUICursorData> uiData = RadialUICursorSystem.radialUICursorData.OrderByDescending(x => x.Priority).ToList();

            if (Main.drawingPlayerChat)
            {
                goOpaque = false;
                min = 0f;
            }
            if ((!player.cursorItemIconEnabled && (player.mouseInterface || player.lastMouseInterface)) || !uiData.Any(x => x.active.Invoke()) || Main.ingameOptionsWindow || Main.InGameUI.IsVisible || player.dead || !Main.mouseItem.IsAir || RadialUICursorSystem.radial.active)
            {
                goOpaque = false;
                drawOpacity = 0f;
                return;
            }
            float clampedOpacity = Utils.Clamp(drawOpacity + 0.05f * (float)goOpaque.ToInt(), min, 1f);
            drawOpacity += 0.05f * (float)Math.Sign(clampedOpacity - drawOpacity);
            if (Math.Abs(drawOpacity - clampedOpacity) < 0.05f)
                drawOpacity = clampedOpacity;

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            List<RadialUICursorData> uiData = RadialUICursorSystem.radialUICursorData.Where(x => x.active.Invoke()).OrderByDescending(x => x.Priority).ToList();
            foreach (var data in uiData)
            {
                data.drawElements(spriteBatch, drawOpacity);
                return;
            }
        }
    }
    public class RadialUICursorData
    {
        public Func<bool> active;
        public Action<SpriteBatch, float> drawElements;
        public float Priority;

        public RadialUICursorData(Func<bool> active, Action<SpriteBatch, float> drawElements, float priority)
        {
            this.active = active;
            this.drawElements = drawElements;
            Priority = priority;
        }
    }
}