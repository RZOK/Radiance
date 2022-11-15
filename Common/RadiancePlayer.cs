using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace Radiance.Common
{
    public class RadiancePlayer : ModPlayer
    {
        public bool debugMode = false;
        public bool canSeeRays = false;

        public Vector2 radianceContainingTileHoverOverCoords = new Vector2(-1, -1);
        public bool hoveringOverRadianceContainingTile = false;
        public float radianceBarAlphaTimer = 0;

        public Vector2 aoeCirclePosition = new Vector2(-1, -1);
        public Vector4 aoeCircleColor = new();
        public float aoeCircleScale = 0;
        public Matrix aoeCircleMatrix = Matrix.Identity;
        public float aoeCircleAlphaTimer = 0;

        public override void ResetEffects()
        {
            debugMode = false;
            canSeeRays = false;

            hoveringOverRadianceContainingTile = false;

            aoeCirclePosition = new Vector2(-1, -1);
            aoeCircleColor = new();
            aoeCircleScale = 0;
            aoeCircleMatrix = Matrix.Identity;
        }
        public override void PostUpdate()
        {
            if (aoeCirclePosition == new Vector2(-1, -1))
                aoeCircleAlphaTimer = 0;
            else if(aoeCircleAlphaTimer < 20)
                aoeCircleAlphaTimer++;

            if (!hoveringOverRadianceContainingTile)
                radianceBarAlphaTimer = 0;
            else if (radianceBarAlphaTimer < 20)
                radianceBarAlphaTimer++;
        }
    }
}