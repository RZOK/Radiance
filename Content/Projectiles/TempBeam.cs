using Microsoft.Xna.Framework;
using Radiance.Utilities;
using Terraria.ModLoader;
using Terraria;

namespace Radiance.Content.Projectiles
{
    public class TempBeam : ModProjectile
    {
        public override string Texture => "Radiance/Content/ExtraTextures/Blank";

        public float lifetime;
        public Color color;
        public Vector2 startPos = new(0, 0);
        public Vector2 endPos = new(0, 0);
        public int innerWidth;
        public int outerWidth;
        public bool spike = false;
        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        public override bool? CanDamage()
        {
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            RadianceDrawing.DrawBeam(startPos, endPos, color.ToVector4() * (Projectile.timeLeft / lifetime), 0.2f, outerWidth, Main.GameViewMatrix.ZoomMatrix, spike);
            RadianceDrawing.DrawBeam(startPos, endPos, new Color(255, 255, 255, 150).ToVector4() * (Projectile.timeLeft / lifetime), 0.2f, innerWidth, Main.GameViewMatrix.ZoomMatrix, spike);
            return false;
        }
    }
}
