using Terraria.GameInput;
using Terraria.UI;

namespace Radiance.Content.UI
{
    public abstract class RadialUI : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Wire Selection"));
        public override bool Visible => visible;

        public bool visible = false;
        public Vector2 position;
        public bool mouseOnMenu = false;
        public override void Update(GameTime gameTime)
        {
            Player player = Main.LocalPlayer;

            if ((player.mouseInterface || player.lastMouseInterface) && !mouseOnMenu)
            {
                visible = false;
                return;
            }
            if (player.dead || !Main.mouseItem.IsAir)
            {
                visible = false;
                mouseOnMenu = false;
                return;
            }
            mouseOnMenu = false;
            if (!Main.mouseRight || !Main.mouseRightRelease || PlayerInput.LockGamepadTileUseButton || player.noThrow != 0 || Main.HoveringOverAnNPC || player.talkNPC != -1)
                return;

            if (visible)
                visible = false;
            else if (!Main.SmartInteractShowingGenuine)
            {
                visible = true;
                position = Main.MouseScreen;
                if (PlayerInput.UsingGamepad && Main.SmartCursorWanted)
                {
                    position = new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
                }
            }

            base.Update(gameTime);
        }
        public abstract List<RadialUIElement> GetElementsToDraw();
        public abstract RadialUIElement GetCenterElement();
        public abstract bool Active();

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Active())
            {
                visible = false;
                return;
            }

            int elementDistance = 45;
            RadialUIElement center = GetCenterElement();
            center?.DrawElement(spriteBatch, Vector2.Zero);

            List<RadialUIElement> visibleElements = GetElementsToDraw();
            for (int i = 0; i < visibleElements.Count; i++)
            {
                RadialUIElement element = visibleElements[i];
                element.DrawElement(spriteBatch, Vector2.UnitY.RotatedBy((float)i / visibleElements.Count * TwoPi) * -elementDistance);
            }
        }

        public virtual void EnableRadialUI()
        {
            visible = true;

            if (PlayerInput.UsingGamepad && Main.SmartCursorWanted)
                position = new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
            else
                position = Main.MouseScreen;
        }
    }

    public class RadialUIElement
    {
        private string textureString;
        public RadialUI parent;
        public Texture2D Texture => ModContent.Request<Texture2D>(textureString).Value;
        /// <summary>
        /// The function to run when the element is clicked.
        /// </summary>
        public Action action;
        public bool redBG;
        public bool enabled;

        public static Texture2D BlueBackgroundTexture => TextureAssets.WireUi[0].Value;
        public static Texture2D BlueBackgroundTextureHover => TextureAssets.WireUi[1].Value;
        public static Texture2D RedBackgroundTexture => TextureAssets.WireUi[8].Value;
        public static Texture2D RedBackgroundTextureHover => TextureAssets.WireUi[9].Value;

        public RadialUIElement(RadialUI parent, string texture, bool enabled, Action action, bool redBG = false)
        {
            this.parent = parent;
            textureString = texture;
            this.action = action;
            this.enabled = enabled;
            this.redBG = redBG;
        }
        public virtual void DrawElement(SpriteBatch spriteBatch, Vector2 position)
        {
            bool MouseHovering = Vector2.Distance(Main.MouseScreen, position + parent.position) < BlueBackgroundTexture.Width / 2;
            if (MouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;
                parent.mouseOnMenu = true;
                if(Main.mouseLeft && Main.mouseLeftRelease)
                    action.Invoke();
            }
            Texture2D backgroundTexture;
            if (redBG)
            {
                if (MouseHovering)
                    backgroundTexture = RedBackgroundTextureHover;
                else
                    backgroundTexture = RedBackgroundTexture;
            }
            else
            {
                if (MouseHovering)
                    backgroundTexture = BlueBackgroundTextureHover;
                else
                    backgroundTexture = BlueBackgroundTexture;
            }

            Color backgroundColor = Color.White;
            Color elementColor = Color.White;
            if (!enabled)
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