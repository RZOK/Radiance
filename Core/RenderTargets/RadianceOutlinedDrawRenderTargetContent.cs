namespace Radiance.Core.RenderTargets
{
    public abstract class RadianceOutlinedDrawRenderTargetContent : ARenderTargetContentByRequest
    {
        protected int width = 84;

        protected int height = 84;

        public Color borderColor = Color.White;

        private EffectPass coloringShader;

        private RenderTarget2D helperTarget;

        public void UseColor(Color color)
        {
            borderColor = color;
        }

        protected override void HandleUseReqest(GraphicsDevice device, SpriteBatch spriteBatch)
        {
            Effect pixelShader = Main.pixelShader;
            coloringShader ??= pixelShader.CurrentTechnique.Passes["ColorOnly"];
            PrepareARenderTarget_AndListenToEvents(ref _target, device, width, height, RenderTargetUsage.PreserveContents);
            PrepareARenderTarget_WithoutListeningToEvents(ref helperTarget, device, width, height, RenderTargetUsage.DiscardContents);
            device.SetRenderTarget(helperTarget);
            device.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null);
            DrawTheContent(spriteBatch);
            spriteBatch.End();
            device.SetRenderTarget(_target);
            device.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null);
            coloringShader.Apply();
            int distance = 2;
            int distance2 = distance * 2;
            for (int i = -distance2; i <= distance2; i += distance)
            {
                for (int j = -distance2; j <= distance2; j += distance)
                {
                    if (Math.Abs(i) + Math.Abs(j) == distance2)
                    {
                        spriteBatch.Draw(helperTarget, new Vector2(i, j), Color.Black);
                    }
                }
            }
            distance2 = distance;
            for (int k = -distance2; k <= distance2; k += distance)
            {
                for (int l = -distance2; l <= distance2; l += distance)
                {
                    if (Math.Abs(k) + Math.Abs(l) == distance2)
                    {
                        spriteBatch.Draw(helperTarget, new Vector2(k, l), borderColor);
                    }
                }
            }
            pixelShader.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(helperTarget, Vector2.Zero, Color.White);
            spriteBatch.End();
            device.SetRenderTarget(null);
            _wasPrepared = true;
        }

        public abstract void DrawTheContent(SpriteBatch spriteBatch);
    }
}