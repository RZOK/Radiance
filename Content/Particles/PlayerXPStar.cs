using Radiance.Core.Config;
using Radiance.Core.Systems;
using Radiance.Core.Systems.ParticleSystems;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Radiance.Content.Particles
{
    public class PlayerXPStar : Particle
    {
        public override string Texture => "Radiance/Content/Particles/Sparkle";
        private Vector2? initialPosition;
        private Vector2 initialVelocity;
        private Vector2? curveOffset;
        private readonly float initialScale;
        private readonly Player player;
        private readonly float threshold;
        private readonly Rectangle frame;
        private float Distance => Math.Max(64, initialPosition.Value.Distance(player.Center));
        private readonly List<(Vector2, float)> trailCache;
        private const int TRAIL_LENGTH = 7;
        public PlayerXPStar(Player player, Vector2 position, Vector2 initialVelocity, int maxTime, float threshold, Color color, float scale)
        {
            trailCache = new List<(Vector2, float)>();
            this.position = position;
            this.player = player;
            this.initialVelocity = initialVelocity;
            this.velocity = initialVelocity;
            this.maxTime = timeLeft = maxTime;
            this.color = color;
            initialScale = scale;
            this.scale = initialScale;
            this.threshold = threshold;
            frame = new Rectangle(0, 42, 14, 14);

            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Additive;
        }

        public override void Update()
        {
            if (Progress < threshold)
            {
                velocity = Vector2.Lerp(initialVelocity, Vector2.Zero, EaseOutCirc(Progress / threshold));
                if (velocity.Y > -0.25f)
                    velocity.Y = -0.25f;
            }
            else
            {
                velocity = Vector2.Zero;
                if (!initialPosition.HasValue)
                    initialPosition = position;
                if(!curveOffset.HasValue)
                    curveOffset = Main.rand.NextVector2CircularEdge(Distance, Distance);

                Vector2 curvePoint = ((player.Center - initialPosition.Value) / 2f) + curveOffset.Value;
                float modifiedProgress = (Progress - threshold) / (1f - threshold);
                float scaleStart = 0.92f;
                if (modifiedProgress >= scaleStart)
                    scale = Lerp(initialScale, 0f, EaseOutExponent((modifiedProgress - scaleStart) / (1f - scaleStart), 2f));

                position = Vector2.Hermite(initialPosition.Value, curvePoint, player.Center, -curvePoint, EaseInExponent(modifiedProgress, 4f));
            }
            ManageCache();
        }
        private void ManageCache()
        {
            if (trailCache.Count == 0)
            {
                for (int i = 0; i < TRAIL_LENGTH; i++)
                {
                    trailCache.Add((position, scale));
                }
            }
            trailCache.Add((position, scale));
            while (trailCache.Count > TRAIL_LENGTH)
            {
                trailCache.RemoveAt(0);
            }
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        { 
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glowTex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/ExtraTextures/SoftGlow").Value;
            float colorMod = Min(1f, Progress * 15f);
            Main.spriteBatch.Draw(glowTex, drawPos, null, color * colorMod * 0.7f, rotation, glowTex.Size() / 2, scale * Main.rand.NextFloat(0.75f, 1.1f) * 0.3f, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(tex, drawPos, frame, color * colorMod, rotation, frame.Size() / 2, scale * Main.rand.NextFloat(0.75f, 1.1f), SpriteEffects.None, 0);
            Main.spriteBatch.Draw(tex, drawPos, frame, Color.White * colorMod * 0.8f, rotation, frame.Size() / 2, scale * Main.rand.NextFloat(0.95f, 1.05f) * 0.7f, SpriteEffects.None, 0);

            for (int i = 0; i < trailCache.Count; i++)
            {
                Vector2 pos = trailCache[i].Item1;
                float scale = trailCache[i].Item2;
                float mod = (i / (float)TRAIL_LENGTH);
                Main.spriteBatch.Draw(tex, pos - Main.screenPosition, frame, color * colorMod * mod, rotation, frame.Size() / 2, scale, SpriteEffects.None, 0);
            }
        }
        public override void Kill()
        {
            WorldParticleSystem.system.DelayedAddParticle(new ShimmerSparkle(player.position + new Vector2(Main.rand.Next(player.width), Main.rand.Next(player.height) + 16), Vector2.UnitY * -Main.rand.NextFloat(6f, 8f), (int)(15f + 20f * Main.rand.NextFloat()), 0, color, 0.8f));
        }
    }
}