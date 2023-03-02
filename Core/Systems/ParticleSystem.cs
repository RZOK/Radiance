using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Radiance.Core.Systems
{
    public class ParticleSystem : ModSystem
    {
        public static List<Particle> particles;
        public static List<Particle> regularlyDrawnParticles;
        public static List<Particle> additiveParticles;

        public static int particleLimit = 1000;

        public enum DrawingMode
        {
            Regular,
            Additive,
            Special,
        }

        public override void Load()
        {
            if (!Main.dedServ)
                return;

            On.Terraria.Main.DrawInfernoRings += StartDrawParticles;
            particles = new List<Particle>();
            regularlyDrawnParticles = new List<Particle>();
            additiveParticles = new List<Particle>();
        }
        public override void Unload()
        {
            if (!Main.dedServ)
                return;

            On.Terraria.Main.DrawInfernoRings -= StartDrawParticles;
            particles = null;
            regularlyDrawnParticles = null;
            additiveParticles = null;
        }
        public static void AddParticle(Particle particle)
        {
            if (Main.gamePaused || Main.dedServ || particles == null)
                return;

            particles.Add(particle);
        }
        public static void RemoveParticle(Particle particle)
        {
            particles.Remove(particle);
        }
        public override void PostUpdateEverything()
        {
            if (!Main.dedServ)
            {
                particles.RemoveAll(x => x.timeLeft <= 0);
                foreach (Particle particle in particles)
                {
                    if(particle == null) 
                        continue;

                    particle.timeLeft--;
                    particle.position += particle.velocity;
                    if (particle.timeLeft <= 0)
                        RemoveParticle(particle);
                }
            }
        }
        private static void StartDrawParticles(On.Terraria.Main.orig_DrawInfernoRings orig, Main self)
        {
            if(particles.Count > 0)
                DrawParticles(Main.spriteBatch);
            
            orig(self);
        }
        public static void DrawParticles(SpriteBatch spriteBatch)
        {
            foreach(Particle particle in particles)
            {
                if (particle.mode == DrawingMode.Special || particle.Texture == "" || particle == null)
                    continue;

                switch(particle.mode)
                {
                    case DrawingMode.Regular:
                        regularlyDrawnParticles.Add(particle);
                        break;
                    case DrawingMode.Additive:
                        additiveParticles.Add(particle);
                        break;
                }
            }
            if(regularlyDrawnParticles.Count > 0)
            {
                foreach (Particle particle in regularlyDrawnParticles)
                {

                }
            }
            if (additiveParticles.Count > 0)
            {
                foreach (Particle particle in additiveParticles)
                {

                }
            }
        }
    }
    public class Particle
    {
        public int id;
        public int type;

        public Vector2 velocity;
        public Vector2 position;
        public int maxTime;
        public int timeLeft;

        public int alpha;
        public int scale;

        public virtual string Texture => "";
        public ParticleSystem.DrawingMode mode = ParticleSystem.DrawingMode.Regular;
        public float Progress => maxTime != 0 ? 1 - timeLeft / maxTime : 0;

        public virtual void SpecialDraw() { }
        public virtual void Update() { }
    }
}
