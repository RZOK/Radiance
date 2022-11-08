using Terraria.ModLoader;
using Radiance.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Radiance.Content.Tiles;

namespace Radiance
{
	public class Radiance : Mod
	{
		public static Color RadianceColor1 = new(255, 192, 66, 255);
		public static Color RadianceColor2 = new(200, 150, 60, 255);

		public const int maxDistanceBetweenPoints = 1000;
		public const int maxRays = 1000;
        public static RadianceRay[] radianceRay = new RadianceRay[maxRays + 1];
	}
}