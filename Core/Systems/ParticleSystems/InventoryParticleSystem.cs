﻿
using static Radiance.Core.Systems.ParticleSystem;

namespace Radiance.Core.Systems.ParticleSystems
{
    public class InventoryParticleSystem : ILoadable
    {
        public static readonly ParticleSystem system = new ParticleSystem(ParticleSystem.ParticleAnchor.World);

        public void Load(Mod mod)
        {
            On_Main.DrawInventory += DrawParticles;
            On_Main.UpdateParticleSystems += UpdateParticles;
        }

        private void DrawParticles(On_Main.orig_DrawInventory orig, Main self)
        {
            orig(self);
            List<Particle> regularlyDrawnParticles = new List<Particle>();
            List<Particle> additiveParticles = new List<Particle>();

            foreach (Particle particle in system.activeParticles)
            {
                if (particle.Texture == "" || particle == null)
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
                Vector2 offset = Vector2.Zero;

            Main.spriteBatch.End();
            if (regularlyDrawnParticles.Count > 0)
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);
                foreach (Particle particle in regularlyDrawnParticles)
                {
                    if (particle.specialDraw)
                        particle.SpecialDraw(Main.spriteBatch, particle.position - offset);
                    else
                    {
                        Texture2D texture = ModContent.Request<Texture2D>(particle.Texture).Value;
                        Main.spriteBatch.Draw(texture, particle.position - offset, null, particle.color * ((255 - particle.alpha) / 255), particle.rotation, texture.Size() * 0.5f, particle.scale, SpriteEffects.None, 0f);
                    }
                }
                Main.spriteBatch.End();
            }
            if (additiveParticles.Count > 0)
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.UIScaleMatrix);
                foreach (Particle particle in additiveParticles)
                {
                    if (particle.specialDraw)
                        particle.SpecialDraw(Main.spriteBatch, particle.position - offset);
                    else
                    {
                        Texture2D texture = ModContent.Request<Texture2D>(particle.Texture).Value;
                        Main.spriteBatch.Draw(texture, particle.position - offset, null, particle.color * ((255 - particle.alpha) / 255), particle.rotation, texture.Size() * 0.5f, particle.scale, SpriteEffects.None, 0f);
                    }
                }
                Main.spriteBatch.End();
            }
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);
        }
        private void UpdateParticles(On_Main.orig_UpdateParticleSystems orig, Main self)
        {
            orig(self);
            system.UpdateParticles();
        }

        public void Unload() { }
    }
}
