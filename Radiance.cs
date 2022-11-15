using Terraria.ModLoader;
using Radiance.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Radiance
{
    public class Radiance : Mod
    {
        public static Radiance Instance { get; set; }
        public static Color RadianceColor1 = new(255, 192, 66);
		public static Color RadianceColor2 = new(200, 150, 60);

		public const int maxDistanceBetweenPoints = 1000;
		public const int maxRays = 1000;
        public static RadianceRay[] radianceRay = new RadianceRay[maxRays + 1];
        public static Texture2D blankTexture;
        public static Texture2D notBlankTexture;

        public Radiance()
		{
			Instance = this;
		}
        public override void Load()
        {
            blankTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/Blank").Value;
            notBlankTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/NotBlank").Value;
        }
    }
}