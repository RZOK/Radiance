using Humanizer;
using Stubble.Core.Helpers;
using static Radiance.Core.Visuals.RadianceDrawing;

namespace Radiance.Core.Systems
{
    public class ParticleSystem : ILoadable
    {
        private static List<Particle> particlesToAdd;
        private static List<Particle> activeParticles;
        private static Dictionary<RadianceDrawLayer, List<Particle>> particlesByLayer;

        public const int particleLimit = 1000;

        public static Dictionary<RadianceDrawLayer, RenderTarget2D> regularPixelatedTargets;
        public static Dictionary<RadianceDrawLayer, RenderTarget2D> additivePixelatedTargets;

        public void Load(Mod mod)
        {
            if (Main.dedServ)
                return;

            activeParticles = new List<Particle>();
            particlesToAdd = new List<Particle>();
            particlesByLayer = new Dictionary<RadianceDrawLayer, List<Particle>>();
            regularPixelatedTargets = new Dictionary<RadianceDrawLayer, RenderTarget2D>();
            additivePixelatedTargets = new Dictionary<RadianceDrawLayer, RenderTarget2D>();

            foreach (RadianceDrawLayer layer in Enum.GetValues(typeof(RadianceDrawLayer)))
            {
                particlesByLayer[layer] = new List<Particle>();
            }

            On_Main.UpdateParticleSystems += UpdateParticles;
            RenderTargetsManager.ResizeRenderTargetDelegateEvent += ResizeRenderTarget;
            RenderTargetsManager.DrawToRenderTargetsDelegateEvent += DrawToRenderTarget;
            ResizeRenderTarget();

            On_Main.DoDraw_Tiles_NonSolid += DrawParticles_OverUnderTiles;
            On_Main.DrawInventory += DrawParticles_OverInventory;
            On_Main.DrawInfernoRings += DrawParticles_OverDust;
        }

        private void DrawParticles_OverUnderTiles(On_Main.orig_DoDraw_Tiles_NonSolid orig, Main self)
        {
            DrawParticles(Main.spriteBatch, RadianceDrawLayer.UnderTiles);
            orig(self);
            DrawParticles(Main.spriteBatch, RadianceDrawLayer.OverTiles);
        }

        private void DrawParticles_OverInventory(On_Main.orig_DrawInventory orig, Main self)
        {
            orig(self);
            DrawParticles(Main.spriteBatch, RadianceDrawLayer.OverInventory);
        }
        private void DrawParticles_OverDust(On_Main.orig_DrawInfernoRings orig, Main self)
        {
            orig(self);
            DrawParticles(Main.spriteBatch, RadianceDrawLayer.OverDust);
        }



        public void Unload()
        { }

        public enum DrawingMode
        {
            None,
            Regular,
            Additive,
        }

        public void DrawToRenderTarget()
        {
            RenderTargetsManager.NoViewMatrixPrims = true;
            GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;

            if (Main.dedServ || Main.gameMenu || Main.spriteBatch is null || graphicsDevice is null)
            {
                graphicsDevice.SetRenderTargets(null);
                return;
            }
            foreach (RadianceDrawLayer layer in Enum.GetValues(typeof(RadianceDrawLayer)))
            {
                List<Particle> regularlyDrawnParticles = new List<Particle>();
                List<Particle> additiveParticles = new List<Particle>();
                foreach (Particle particle in particlesByLayer[layer])
                {
                    if (particle.Texture == "" || particle == null || !particle.drawPixelated)
                        continue;

                    switch (particle.mode)
                    {
                        case DrawingMode.Regular:
                            regularlyDrawnParticles.Add(particle);
                            break;

                        case DrawingMode.Additive:
                            additiveParticles.Add(particle);
                            break;
                    }
                }

                RenderTarget2D regularRenderTarget = regularPixelatedTargets[layer];
                graphicsDevice.SetRenderTarget(regularRenderTarget);
                graphicsDevice.Clear(Color.Transparent);
                if (regularRenderTarget is not null)
                {
                    if (regularlyDrawnParticles.Count > 0)
                    {
                        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null);
                        foreach (Particle particle in regularlyDrawnParticles)
                        {
                            Vector2 offset = Main.screenPosition;
                            if (particle.screenAnchor)
                                offset = Vector2.Zero;

                            if (particle.specialDraw)
                                particle.SpecialDraw(Main.spriteBatch, particle.position - offset);
                            else
                            {
                                Texture2D texture = ModContent.Request<Texture2D>(particle.Texture).Value;
                                Main.spriteBatch.Draw(texture, particle.position - offset, null, particle.color, particle.rotation, texture.Size() * 0.5f, particle.scale, SpriteEffects.None, 0f);
                            }
                        }
                        Main.spriteBatch.End();
                    }
                }

                RenderTarget2D additivePixelatedTarget = additivePixelatedTargets[layer];
                graphicsDevice.SetRenderTarget(additivePixelatedTarget);
                graphicsDevice.Clear(Color.Transparent);

                if (additivePixelatedTarget is null)
                {
                    graphicsDevice.SetRenderTargets(null);
                    continue;
                }

                if (additiveParticles.Count > 0)
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null);
                    foreach (Particle particle in additiveParticles)
                    {
                        Vector2 offset = Main.screenPosition;
                        if (particle.screenAnchor)
                            offset = Vector2.Zero;

                        if (particle.specialDraw)
                            particle.SpecialDraw(Main.spriteBatch, particle.position - offset);
                        else
                        {
                            Texture2D texture = ModContent.Request<Texture2D>(particle.Texture).Value;
                            Main.spriteBatch.Draw(texture, particle.position - offset, null, particle.color, particle.rotation, texture.Size() * 0.5f, particle.scale, SpriteEffects.None, 0f);
                        }
                    }
                    Main.spriteBatch.End();
                }
                graphicsDevice.SetRenderTargets(null);
            }
            RenderTargetsManager.NoViewMatrixPrims = false; //todo: may not work with screen-anchored particles
        }

        public void ResizeRenderTarget()
        {
            Main.QueueMainThreadAction(() =>
            {
                foreach (RadianceDrawLayer layer in Enum.GetValues(typeof(RadianceDrawLayer)))
                {
                    if(!regularPixelatedTargets.ContainsKey(layer))
                        regularPixelatedTargets[layer] = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
                    if (!additivePixelatedTargets.ContainsKey(layer))
                        additivePixelatedTargets[layer] = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);

                    RenderTarget2D regularRenderTarget = null;
                    RenderTarget2D additiveRenderTarget = null;

                    if (regularRenderTarget != null && !regularRenderTarget.IsDisposed)
                        regularRenderTarget.Dispose();
                    if (additiveRenderTarget != null && !additiveRenderTarget.IsDisposed)
                        additiveRenderTarget.Dispose();

                    regularRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
                    additiveRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
                }
            });
        }

        public static void AddParticle(Particle particle)
        {
            if (Main.gamePaused || Main.dedServ || activeParticles is null || particlesByLayer is null)
                return;

            activeParticles.Add(particle);
            particlesByLayer[particle.layer].Add(particle);
        }

        public static void DelayedAddParticle(Particle particle)
        {
            if (Main.gamePaused || Main.dedServ || activeParticles is null || particlesByLayer is null)
                return;

            particlesToAdd.Add(particle);
        }

        public void UpdateParticles(On_Main.orig_UpdateParticleSystems orig, Main self)
        {
            orig(self);
            List<Particle> particlesToRemove = new List<Particle>();
            if (!Main.dedServ)
            {
                foreach (Particle particle in particlesToAdd)
                {
                    AddParticle(particle);
                }
                particlesToAdd.Clear();
                foreach (Particle particle in activeParticles)
                {
                    if (particle == null)
                        continue;

                    particle.timeLeft--;
                    if (particle.timeLeft <= 0)
                    {
                        particle.active = false;
                        particle.Kill();
                    }
                    else
                    {
                        particle.Update();
                        particle.position += particle.velocity;
                    }
                    if (!particle.active)
                        particlesToRemove.Add(particle);
                }
            }
            foreach (Particle particle in particlesToRemove)
            {
                activeParticles.Remove(particle);
                particlesByLayer[particle.layer].Remove(particle);
            }
        }

        public void DrawParticles(SpriteBatch spriteBatch, RadianceDrawLayer layer)
        {
            List<Particle> regularlyDrawnParticles = new List<Particle>();
            List<Particle> additiveParticles = new List<Particle>();

            foreach (Particle particle in particlesByLayer[layer])
            {
                if (particle.Texture == "" || particle == null || particle.drawPixelated)
                    continue;

                switch (particle.mode)
                {
                    case DrawingMode.Regular:
                        regularlyDrawnParticles.Add(particle);
                        break;

                    case DrawingMode.Additive:
                        additiveParticles.Add(particle);
                        break;
                }
            }

            spriteBatch.GetSpritebatchDetails(out SpriteSortMode spriteSortMode, out BlendState blendState, out SamplerState samplerState, out DepthStencilState depthStencilState, out RasterizerState rasterizerState, out Effect effect, out Matrix matrix);
            spriteBatch.End();
            if (regularlyDrawnParticles.Count > 0)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, matrix);
                foreach (Particle particle in regularlyDrawnParticles)
                {
                    Vector2 offset = Main.screenPosition;
                    if (particle.screenAnchor)
                        offset = Vector2.Zero;

                    if (particle.specialDraw)
                        particle.SpecialDraw(spriteBatch, particle.position - offset);
                    else
                    {
                        Texture2D texture = ModContent.Request<Texture2D>(particle.Texture).Value;
                        spriteBatch.Draw(texture, particle.position - offset, null, particle.color, particle.rotation, texture.Size() * 0.5f, particle.scale, SpriteEffects.None, 0f);
                    }
                }
                RenderTarget2D regularRenderTarget = regularPixelatedTargets[layer];
                if (regularRenderTarget is not null)
                    Main.spriteBatch.Draw(regularRenderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 2f, SpriteEffects.None, 0);

                spriteBatch.End();
            }
            if (additiveParticles.Count > 0)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, matrix);
                foreach (Particle particle in additiveParticles)
                {
                    Vector2 offset = Main.screenPosition;
                    if (particle.screenAnchor)
                        offset = Vector2.Zero;

                    if (particle.specialDraw)
                        particle.SpecialDraw(spriteBatch, particle.position - offset);
                    else
                    {
                        Texture2D texture = ModContent.Request<Texture2D>(particle.Texture).Value;
                        spriteBatch.Draw(texture, particle.position - offset, null, particle.color, particle.rotation, texture.Size() * 0.5f, particle.scale, SpriteEffects.None, 0f);
                    }
                }
                RenderTarget2D additivePixelatedTarget = additivePixelatedTargets[layer];
                if (additivePixelatedTarget is not null)
                    Main.spriteBatch.Draw(additivePixelatedTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 2f, SpriteEffects.None, 0);

                spriteBatch.End();
            }
            spriteBatch.Begin(spriteSortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, matrix);
        }
    }

    public class Particle
    {
        public int type;
        public Vector2 velocity;
        public Vector2 position;
        public int maxTime;
        public int timeLeft;

        public float scale;
        public Color color;
        public float rotation;
        public virtual string Texture => "";
        public ParticleSystem.DrawingMode mode = ParticleSystem.DrawingMode.Regular;
        public RadianceDrawLayer layer = RadianceDrawLayer.OverDust;
        public bool specialDraw = false;
        public bool drawPixelated = false;
        public bool screenAnchor = false;
        public bool active = true;
        public float Progress => maxTime > 0 ? 1f - (float)timeLeft / maxTime : 0;

        /// <param name="drawPos">The position of the particle relative to the world if the ParticleSystem it exists within is anchored to such, or the position of the particle on the screen if anchored to the screen</param>
        public virtual void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        { }

        public virtual void Update()
        { }

        public virtual void Kill()
        { }
    }
}