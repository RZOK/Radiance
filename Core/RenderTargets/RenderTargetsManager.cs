namespace Radiance.Core
{
    public class RenderTargetsManager : ModSystem
    {
        private Vector2 oldScreenSize;
        public static float RTSize = 1f;
        public static bool NoViewMatrixPrims = false;

        public override void PostUpdateEverything()
        {
            CheckScreenSize();
        }

        public delegate void ResizeRenderTargetDelegate();

        public static event ResizeRenderTargetDelegate ResizeRenderTargetDelegateEvent;

        public void CheckScreenSize()
        {
            if (!Main.dedServ && !Main.gameMenu)
            {
                Vector2 newScreenSize = new Vector2(Main.screenWidth, Main.screenHeight);
                if (oldScreenSize != newScreenSize)
                {
                    ResizeRenderTargetDelegateEvent?.Invoke();
                }
                oldScreenSize = newScreenSize;
            }
        }

        public delegate void DrawToRenderTargetsDelegate();

        public static event DrawToRenderTargetsDelegate DrawToRenderTargetsDelegateEvent;

        private void DrawToRenderTargets(On_Main.orig_CheckMonoliths orig)
        {
            if (Main.spriteBatch != null && Main.graphics.GraphicsDevice != null && !Main.gameMenu)
                DrawToRenderTargetsDelegateEvent?.Invoke();
            orig();
        }

        public override void Load()
        {
            On_Main.CheckMonoliths += DrawToRenderTargets;
        }

        public override void Unload()
        {
            ResizeRenderTargetDelegateEvent = null;
        }
    }
}