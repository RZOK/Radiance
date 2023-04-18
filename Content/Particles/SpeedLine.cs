//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Radiance.Core.Systems;
//using System.Collections.Generic;
//using Terraria;
//using Terraria.ModLoader;

//namespace Radiance.Content.Particles
//{
//    public class SpeedLine : Particle
//    {
//        public override string Texture => "Radiance/Content/Particles/SpeedLine";
//        private readonly int trailLength;
//        private Vector2 startPosition;;

//        public SpeedLine(Vector2 position, Vector2 velocity, int maxTime, float alpha, Color color, float rotation, int trailLength, float scale = 1)
//        {
//            this.position = position;
//            this.velocity = velocity;
//            this.maxTime = maxTime;
//            timeLeft = maxTime;
//            this.alpha = alpha;
//            this.color = color;
//            this.scale = scale;
//            specialDraw = true;
//            this.rotation = rotation;
//            startPosition = position;
//            mode = ParticleSystem.DrawingMode.Additive;
//            this.trailLength = trailLength;
//        }

//        public override void Update()
//        {
//            velocity *= 0.8f;
//        }

//        public override void SpecialDraw(SpriteBatch spriteBatch)
//        {
//                Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
//                Vector2 scale = new Vector2(1, Vector2.Distance(position, startPosition));
//                spriteBatch.Draw(tex, position - Main.screenPosition, null, color * ((255 - alpha) / 255), rotation + MathHelper.PiOver2, tex.Size() / 2, Vector2, 0, 0);
            
//        }
//    }
//}