using Terraria.Graphics.CameraModifiers;

namespace Radiance.Core.Systems
{
    public class CameraSystem : ModSystem
    {
        public static float Quake;

        public override void ModifyScreenPosition()
        {
            float mult = Main.screenWidth / 2048f * 1.2f;

            Main.instance.CameraModifiers.Add(new PunchCameraModifier(Main.LocalPlayer.position, Main.rand.NextFloat(3.14f).ToRotationVector2(), Quake * mult, 15f, 30, 2000, "Radiance Quake"));

            if (Quake > 0)
                Quake = Math.Max(Quake - 1, 0);
        }
    }
}