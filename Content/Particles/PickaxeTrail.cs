﻿using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class PickaxeTrail : Particle
    {
        private readonly Texture2D pickaxe;
        public override string Texture => "Radiance/Content/ExtraTextures/Blank";
        public PickaxeTrail(Vector2 position, Texture2D pickaxe, int maxTime, float rotation, Color color, float alpha, float scale = 1)
        {
            this.position = position;
            this.pickaxe = pickaxe;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            this.alpha = alpha;
            this.color = color;
            this.scale = scale;
            this.rotation = rotation;

            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Additive;
        }

        public override void Update()
        {
            alpha += 255f / maxTime;
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            if(timeLeft < maxTime)
                spriteBatch.Draw(pickaxe, drawPos, null, color * ((255 - alpha) / 255), rotation, pickaxe.Size() * scale / 2, scale, 0, 0);
        }
    }
}