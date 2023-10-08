using MonoMod.ModInterop;
using Terraria.GameInput;
using Terraria.UI;

namespace Radiance.Content.UI.NewEntryAlert
{
    public abstract class RadialUI : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Wire Selection"));
        public override bool Visible => active;

        public bool active;
        public RadialUIElement center;
        public List<RadialUIElement> surroundingElements = new List<RadialUIElement>();
        public Vector2 position;
        public bool MouseOnMenu = false;
        public override void Update(GameTime gameTime)
        {
            Player player = Main.LocalPlayer;

            if ((player.mouseInterface || player.lastMouseInterface) && !MouseOnMenu)
            {
                active = false;
                return;
            }
            if (player.dead || !Main.mouseItem.IsAir)
            {
                active = false;
                MouseOnMenu = false;
                return;
            }
            MouseOnMenu = false;
            if (!Main.mouseRight || !Main.mouseRightRelease || PlayerInput.LockGamepadTileUseButton || player.noThrow != 0 || Main.HoveringOverAnNPC || player.talkNPC != -1)
                return;

            if (active)
                active = false;

            else if (!Main.SmartInteractShowingGenuine)
            {
                active = true;
                position = Main.MouseScreen;
                if (PlayerInput.UsingGamepad && Main.SmartCursorWanted)
                {
                    position = new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            int elementDistance = 45;

            center.DrawElement(spriteBatch, Vector2.Zero);
            for (int i = 0; i < surroundingElements.Count; i++)
            {
                RadialUIElement element = surroundingElements[i];
                element.DrawElement(spriteBatch, Vector2.UnitY.RotatedBy((float)i / surroundingElements.Count * TwoPi) * -elementDistance);
            }
        }
        public abstract void LoadElements();
    }

    public class RadialUIElement
    {
        private string textureString;
        public RadialUI parent;
        public Texture2D Texture => ModContent.Request<Texture2D>(textureString).Value;
        public Action action;
        public Func<bool> enabled;
        public bool redBG = false;
        public static Texture2D BlueBackgroundTexture => TextureAssets.WireUi[0].Value;
        public static Texture2D BlueBackgroundTextureHover => TextureAssets.WireUi[1].Value;
        public static Texture2D RedBackgroundTexture => TextureAssets.WireUi[8].Value;
        public static Texture2D RedBackgroundTextureHover => TextureAssets.WireUi[9].Value;

        public RadialUIElement(RadialUI parent, string texture, Action action, Func<bool> enabled)
        {
            this.parent = parent;
            textureString = texture;
            this.action = action;
            this.enabled = enabled;
        }
        public virtual void DrawElement(SpriteBatch spriteBatch, Vector2 position)
        {
            bool MouseHovering = Vector2.Distance(Main.MouseScreen, position + parent.position) < BlueBackgroundTexture.Width / 2;
            if (MouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;
                parent.MouseOnMenu = true;
                if(Main.mouseLeft && Main.mouseLeftRelease)
                    action.Invoke();
            }
            Texture2D backgroundTexture;
            if (!parent.center.redBG)
            {
                if (MouseHovering)
                    backgroundTexture = BlueBackgroundTextureHover;
                else
                    backgroundTexture = BlueBackgroundTexture;
            }
            else
            {
                if (MouseHovering)
                    backgroundTexture = RedBackgroundTextureHover;
                else
                    backgroundTexture = RedBackgroundTexture;
            }

            Color backgroundColor = Color.White;
            Color elementColor = Color.White;
            if (!enabled.Invoke())
            {
                if (MouseHovering)
                {
                    backgroundColor = new Color(120, 120, 120);
                    elementColor = new Color(200, 200, 200);
                }
                else
                {
                    backgroundColor = new Color(80, 80, 80);
                    elementColor = new Color(100, 100, 100);
                }
            }

            spriteBatch.Draw(backgroundTexture, position + parent.position, null, backgroundColor, 0, BlueBackgroundTexture.Size() / 2, 1f, SpriteEffects.None, 0);
            spriteBatch.Draw(Texture, position + parent.position, null, elementColor, 0, Texture.Size() / 2, 1f, SpriteEffects.None, 0);
        }
    }
}