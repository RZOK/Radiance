using Terraria.ModLoader;
using Radiance.Core;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Radiance
{
    public class Radiance : Mod
    {
        public static Radiance Instance { get; set; }

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
        public override void Unload()
        {
            if (!Main.dedServ)
            {
                Instance = null;
            }
        } 
    }
}