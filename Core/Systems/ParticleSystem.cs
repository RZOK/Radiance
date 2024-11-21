using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Policy;
using Terraria.ModLoader.Core;

namespace Radiance.Core.Systems
{
    public class ParticleSystem : ModSystem
    {
        //based on the spirit/fables particle system
        public static List<Particle> particlesToAdd;
        public static List<Particle> activeParticles;

        public static List<Particle> particleInstances;
        public static Dictionary<Type, int> particlesDict;

        public static int particleLimit = 1000;

        public enum DrawingMode
        {
            None,
            Regular,
            Additive,
        }

        public static void SetupParticles()
        {
            Mod mod = Radiance.Instance;
            foreach (Type type in AssemblyManager.GetLoadableTypes(mod.Code).Where(t => t.IsSubclassOf(typeof(Particle)) && !t.IsAbstract))
            {
                Particle particle = (Particle)FormatterServices.GetUninitializedObject(type);
                particleInstances.Add(particle);
                particlesDict[type] = particlesDict.Count;
            }
        }

        public override void Load()
        {
            if (Main.dedServ)
                return;

            On_Main.DrawInfernoRings += StartDrawParticles;
            On_Main.DoDraw_Tiles_NonSolid += StartDrawBehindTileParticles;

            activeParticles = new List<Particle>();
            particlesToAdd = new List<Particle>();
            particlesDict = new Dictionary<Type, int>();
            particleInstances = new List<Particle>();

            SetupParticles();
        }

        public override void Unload()
        {
            if (Main.dedServ)
                return;

            activeParticles = null;
        }

        public static void AddParticle(Particle particle)
        {
            if (Main.gamePaused || Main.dedServ || activeParticles == null)
                return;

                activeParticles.Add(particle);

            particle.type = particlesDict[particle.GetType()];
        }
        public static void DelayedAddParticle(Particle particle)
        {
            if (Main.gamePaused || Main.dedServ || activeParticles == null)
                return;

            particlesToAdd.Add(particle);
            particle.type = particlesDict[particle.GetType()];
        }

        public static void RemoveParticle(Particle particle)
        {
            activeParticles.Remove(particle);
        }

        public override void PostUpdateEverything()
        {
            if (!Main.dedServ)
            {
                foreach (Particle particle in particlesToAdd)
                {
                    activeParticles.Add(particle);
                }
                particlesToAdd.Clear();
                foreach (Particle particle in activeParticles)
                {
                    if (particle == null)
                        continue;

                    particle.Update();
                    particle.position += particle.velocity;
                    particle.timeLeft--;
                }
            }
            activeParticles.RemoveAll(x => x.timeLeft <= 0);
        }

        private static void StartDrawParticles(On_Main.orig_DrawInfernoRings orig, Main self)
        {
            if (activeParticles.Where(x => !x.behindTiles).Any())
                DrawParticles(Main.spriteBatch);
            orig(self);
        }
        private void StartDrawBehindTileParticles(On_Main.orig_DoDraw_Tiles_NonSolid orig, Main self)
        {
            if (activeParticles.Where(x => x.behindTiles).Any())
                DrawParticles(Main.spriteBatch, true);
            orig(self);
        }
        public static void DrawParticles(SpriteBatch spriteBatch, bool behindTiles = false)
        {
            List<Particle> regularlyDrawnParticles = new List<Particle>();
            List<Particle> additiveParticles = new List<Particle>();

            foreach (Particle particle in activeParticles)
            {
                if (particle.Texture == "" || particle == null)
                    continue;

                if (!behindTiles && particle.behindTiles)
                    continue;

                if (behindTiles && !particle.behindTiles)
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

            spriteBatch.End();
            if (regularlyDrawnParticles.Count > 0)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                foreach (Particle particle in regularlyDrawnParticles)
                {
                    if (particle.specialDraw)
                        particle.SpecialDraw(spriteBatch);
                    else
                    {
                        Texture2D texture = ModContent.Request<Texture2D>(particle.Texture).Value;
                        spriteBatch.Draw(texture, particle.position - Main.screenPosition, null, particle.color * ((255 - particle.alpha) / 255), particle.rotation, texture.Size() * 0.5f, particle.scale, SpriteEffects.None, 0f);
                    }
                }
                spriteBatch.End();
            }
            if (additiveParticles.Count > 0)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                foreach (Particle particle in additiveParticles)
                {
                    if (particle.specialDraw)
                        particle.SpecialDraw(spriteBatch);
                    else
                    {
                        Texture2D texture = ModContent.Request<Texture2D>(particle.Texture).Value;
                        spriteBatch.Draw(texture, particle.position - Main.screenPosition, null, particle.color * ((255 - particle.alpha) / 255), particle.rotation, texture.Size() * 0.5f, particle.scale, SpriteEffects.None, 0f);
                    }
                }
                spriteBatch.End();
            }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }
    }

    public class Particle
    {
        public int type;
        public Vector2 velocity;
        public Vector2 position;
        public Vector2 DrawPos => anchorScreen ? position : position - Main.screenPosition;
        public int maxTime;
        public int timeLeft;
        public bool anchorScreen = false;

        public float alpha;
        public float scale;
        public Color color;
        public float rotation;
        public bool behindTiles = false;
        public virtual string Texture => "";
        public ParticleSystem.DrawingMode mode = ParticleSystem.DrawingMode.Regular;
        public bool specialDraw = false;
        public float Progress => maxTime > 0 ? 1f - (float)timeLeft / maxTime : 0;

        public virtual void SpecialDraw(SpriteBatch spriteBatch)
        { }

        public virtual void Update()
        { }
    }
}