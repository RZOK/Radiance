using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using Radiance.Core;
using Radiance.Utilities;

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
            float fade = RadianceUtils.EaseOutCirc(Projectile.timeLeft / lifetime);
            RadianceDrawing.DrawBeam(startPos, endPos, color.ToVector4() * fade, 0.2f, outerWidth, Main.GameViewMatrix.ZoomMatrix, spike);
            RadianceDrawing.DrawBeam(startPos, endPos, new Color(255, 255, 255, 150).ToVector4() * fade, 0.2f, innerWidth, Main.GameViewMatrix.ZoomMatrix, spike);
            return false;
        }
    }
}
