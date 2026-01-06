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
            RadianceInterfacePlayer radialUICursorPlayer = Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>();
            float min = 0.75f;
            List<RadialUICursorData> uiData = RadialUICursorSystem.radialUICursorData.OrderByDescending(x => x.priority).ToList();

            if (Main.drawingPlayerChat)
            {
                goOpaque = false;
                min = 0f; 
            }
            if ((!player.cursorItemIconEnabled && (player.mouseInterface || player.lastMouseInterface)) || !uiData.Any(x => x.parent.Active()) || Main.ingameOptionsWindow || Main.InGameUI.IsVisible || player.dead || !Main.mouseItem.IsAir || RadialUICursorSystem.radial.active)
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
            List<RadialUICursorData> uiData = RadialUICursorSystem.radialUICursorData.Where(x => x.parent.Active()).OrderByDescending(x => x.priority).ToList();
            foreach (var data in uiData)
            {
                data.drawElements(spriteBatch, drawOpacity);
                return;
            }
        }
    }
    public class RadialUICursorData
    {
        public RadialUI parent;
        /// <summary>
        /// The priority in which cursor UIs should be drawn in the case that multiple should be active. Higher numbers go first.
        /// </summary>
        public float priority;
        public Action<SpriteBatch, float> drawElements;

        public RadialUICursorData(RadialUI parent, float priority, Action<SpriteBatch, float> drawElements)
        {
            this.parent = parent;
            this.priority = priority;
            this.drawElements = drawElements;
        }
    }
}