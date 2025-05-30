namespace Radiance.Core.Systems
{
    public class ParticleSystem
    {
        public List<Particle> particlesToAdd;
        public List<Particle> activeParticles;

        public List<Particle> particleInstances;
        public Dictionary<Type, int> particlesDict;

        public int particleLimit = 1000;
        public ParticleAnchor anchor;
        public enum ParticleAnchor
        {
            World,
            Screen,
            UI
        }
        public ParticleSystem(ParticleAnchor anchor)
        {
            if (Main.dedServ)
                return;

            activeParticles = new List<Particle>();
            particlesToAdd = new List<Particle>();
            this.anchor = anchor;
            //particlesDict = new Dictionary<Type, int>();
            //particleInstances = new List<Particle>();
        }
        public enum DrawingMode
        {
            None,
            Regular,
            Additive,
        }

        //public static void SetupParticles()
        //{
        //    Mod mod = Radiance.Instance;
        //    foreach (Type type in AssemblyManager.GetLoadableTypes(mod.Code).Where(t => t.IsSubclassOf(typeof(Particle)) && !t.IsAbstract))
        //    {
        //        Particle particle = (Particle)FormatterServices.GetUninitializedObject(type);
        //        particleInstances.Add(particle);
        //        particlesDict[type] = particlesDict.Count;
        //    }
        //}

        public void AddParticle(Particle particle)
        {
            if (Main.gamePaused || Main.dedServ || activeParticles == null)
                return;

            activeParticles.Add(particle);
            //particle.type = particlesDict[particle.GetType()];
        }
        public void DelayedAddParticle(Particle particle)
        {
            if (Main.gamePaused || Main.dedServ || activeParticles == null)
                return;

            particlesToAdd.Add(particle);
            //particle.type = particlesDict[particle.GetType()];
        }

        public void RemoveParticle(Particle particle)
        {
            activeParticles.Remove(particle);
        }
        public void UpdateParticles()
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

                    particle.timeLeft--;
                    if (particle.timeLeft <= 0)
                        particle.Kill();
                    else
                    {
                        particle.Update();
                        particle.position += particle.velocity;
                    }
                }
            }
            activeParticles.RemoveAll(x => x.timeLeft <= 0);
        }
        public void DrawParticles(SpriteBatch spriteBatch)
        {
            List<Particle> regularlyDrawnParticles = new List<Particle>();
            List<Particle> additiveParticles = new List<Particle>();

            foreach (Particle particle in activeParticles)
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
            Vector2 offset = Main.screenPosition;
            if (anchor != ParticleAnchor.World)
                offset = Vector2.Zero;

            spriteBatch.GetSpritebatchDetails(out SpriteSortMode spriteSortMode, out BlendState blendState, out SamplerState samplerState, out DepthStencilState depthStencilState, out RasterizerState rasterizerState, out Effect effect, out Matrix matrix);
            spriteBatch.End();
            if (regularlyDrawnParticles.Count > 0)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, matrix);
                foreach (Particle particle in regularlyDrawnParticles)
                {
                    if (particle.specialDraw)
                        particle.SpecialDraw(spriteBatch, particle.position - offset);
                    else
                    {
                        Texture2D texture = ModContent.Request<Texture2D>(particle.Texture).Value;
                        spriteBatch.Draw(texture, particle.position - offset, null, particle.color * ((255 - particle.alpha) / 255), particle.rotation, texture.Size() * 0.5f, particle.scale, SpriteEffects.None, 0f);
                    }
                }
                spriteBatch.End();
            }
            if (additiveParticles.Count > 0)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, matrix);
                foreach (Particle particle in additiveParticles)
                {
                    if (particle.specialDraw)
                        particle.SpecialDraw(spriteBatch, particle.position - offset);
                    else
                    {
                        Texture2D texture = ModContent.Request<Texture2D>(particle.Texture).Value;
                        spriteBatch.Draw(texture, particle.position - offset, null, particle.color * ((255 - particle.alpha) / 255), particle.rotation, texture.Size() * 0.5f, particle.scale, SpriteEffects.None, 0f);
                    }
                }
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

        public float alpha;
        public float scale;
        public Color color;
        public float rotation;
        public virtual string Texture => "";
        public ParticleSystem.DrawingMode mode = ParticleSystem.DrawingMode.Regular;
        public bool specialDraw = false;
        public float Progress => maxTime > 0 ? 1f - (float)timeLeft / maxTime : 0;

        /// <param name="drawPos">The position of the particle relative to the world if the ParticleSystem it exists within is anchored to such, position of the particle on the screen if anchored to the screen</param>
        public virtual void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        { }

        public virtual void Update()
        { }

        public virtual void Kill()
        { }
    }
}