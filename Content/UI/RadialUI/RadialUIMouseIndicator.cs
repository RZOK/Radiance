using Terraria.GameContent.UI;
using System.Reflection;
using Terraria.UI;
using static Terraria.GameContent.UI.WiresUI;
using MonoMod.Cil;
using Terraria.GameContent.UI.Elements;
using Mono.Cecil.Cil;

namespace Radiance.Content.UI
{
    public class RadialUIMouseIndicatorSystem : ModSystem
    {
        public static List<RadialUIMouseIndicatorData> radialUIMouseIndicatorData = new List<RadialUIMouseIndicatorData>();
        public static WiresRadial radial;
        public override void Load()
        {
            radial = (WiresRadial)typeof(WiresUI).GetField("radial", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            IL_Main.DrawInterface_40_InteractItemIcon += GetRealCursorItem;
        }
        private void GetRealCursorItem(ILContext il)
        {
            // in the case that the cursor item isn't set manually, it picks the player's held item
            // however, it doesn't save that value anywhere
            // so we have to grab it manually when it's drawn

            ILCursor cursor = new ILCursor(il);
            cursor.EmitDelegate<Action>(() => RadialUIMouseIndicator.realCursorItemType = 0);

            //if (!cursor.TryGotoNext(MoveType.After,
            //   i => i.MatchLdsfld(typeof(Main), nameof(Main.instance)),
            //   i => i.MatchLdsfld(typeof(Main), nameof(Main.player)),
            //   i => i.MatchLdsfld(typeof(Main), nameof(Main.myPlayer)),
            //   i => i.MatchLdelemRef(),
            //   i => i.MatchLdfld(typeof(Player), nameof(Player.inventory)),
            //   i => i.MatchLdsfld(typeof(Main), nameof(Main.player)),
            //   i => i.MatchLdsfld(typeof(Main), nameof(Main.myPlayer)),
            //   i => i.MatchLdelemRef(),
            //   i => i.MatchLdfld(typeof(Player), nameof(Player.selectedItem)),
            //   i => i.MatchLdelemRef(),
            //   i => i.MatchLdfld(typeof(Item), nameof(Item.type)),
            //   i => i.MatchCallvirt(out _)
            //   ))
            //{
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
            cursor.EmitDelegate(SetRealCursorItemType);
        }
        public void SetRealCursorItemType(int i)
        {
            RadialUIMouseIndicator.realCursorItemType = i;
        }
    }
    public class RadialUIMouseIndicator : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Wire Selection"));
        public override bool Visible => !Main.gameMenu;
        private float drawOpacity = 0f;
        public List<RadialUIElement> surroundingElements = new List<RadialUIElement>();
        public static int realCursorItemType;
        public override void Update(GameTime gameTime)
        {
            bool goOpaque = true;
            Player player = Main.LocalPlayer;
            float min = 0.75f;
            List<RadialUIMouseIndicatorData> uiData = RadialUIMouseIndicatorSystem.radialUIMouseIndicatorData.OrderByDescending(x => x.Priority).ToList();

            if (Main.drawingPlayerChat)
            {
                goOpaque = false;
                min = 0f;
            }
            if ((!player.cursorItemIconEnabled && (player.mouseInterface || player.lastMouseInterface)) || !uiData.Any(x => x.active.Invoke()) || Main.ingameOptionsWindow || Main.InGameUI.IsVisible || player.dead || !Main.mouseItem.IsAir || RadialUIMouseIndicatorSystem.radial.active)
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
            List<RadialUIMouseIndicatorData> uiData = RadialUIMouseIndicatorSystem.radialUIMouseIndicatorData.OrderByDescending(x => x.Priority).ToList();
            foreach (var data in uiData)
            {
                if (!data.active.Invoke())
                    continue;

                data.drawElements(spriteBatch, drawOpacity);
                return;
            }
        }
    }
    public class RadialUIMouseIndicatorData
    {
        public Func<bool> active;
        public Action<SpriteBatch, float> drawElements;
        public float Priority;

        public RadialUIMouseIndicatorData(Func<bool> active, Action<SpriteBatch, float> drawElements, float priority)
        {
            this.active = active;
            this.drawElements = drawElements;
            Priority = priority;
        }
    }
}