using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace Radiance.Common
{
    public class RadiancePlayer : ModPlayer
    {
        public bool debugMode = false;
        public Vector2 radianceContainingTileHoverOverCoords = new Vector2(0, 0);
        public bool hoveringOverRadianceContainingTile = false;
        public bool canSeeRays = false;

        public override void ResetEffects()
        {
            hoveringOverRadianceContainingTile = false;
            canSeeRays = false;
            debugMode = false;
        }
    }
}