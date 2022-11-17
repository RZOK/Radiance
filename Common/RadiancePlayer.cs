using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace Radiance.Common
{
    public class RadiancePlayer : ModPlayer
    {
        public bool debugMode = false;
        public bool canSeeRays = false;

        public Vector2 radianceContainingTileHoverOverCoords = new Vector2(-1, -1);
        public float radianceBarAlphaTimer = 0;

        public Vector2 aoeCirclePosition = new Vector2(-1, -1);
        public Vector4 aoeCircleColor = new();
        public float aoeCircleScale = 0;
        public Matrix aoeCircleMatrix = Matrix.Identity;
        public float aoeCircleAlphaTimer = 0;

        public Vector2 hoveringOverSpecialTextTileCoords = new Vector2(-1, -1);
        public float hoveringOverSpecialTextTileAlphaTimer = 0;
        public string hoveringOverSpecialTextTileString = string.Empty;
        public string hoveringOverSpecialTextTileItemTagString = string.Empty;
        public Color hoveringOverSpecialTextTileColor = new();

        public override void ResetEffects()
        {
            debugMode = false;
            canSeeRays = false;

            radianceContainingTileHoverOverCoords = new Vector2(-1, -1);

            aoeCirclePosition = new Vector2(-1, -1);
            aoeCircleColor = new();
            aoeCircleScale = 0;
            aoeCircleMatrix = Matrix.Identity;

            hoveringOverSpecialTextTileCoords = new Vector2(-1, -1);
            hoveringOverSpecialTextTileString = string.Empty;
            hoveringOverSpecialTextTileItemTagString = string.Empty;
            hoveringOverSpecialTextTileColor = new();
        }
        public override void PostUpdate()
        {
            if (aoeCirclePosition == new Vector2(-1, -1))
                aoeCircleAlphaTimer = 0;
            else if(aoeCircleAlphaTimer < 20)
                aoeCircleAlphaTimer++;

            if (radianceContainingTileHoverOverCoords == new Vector2(-1, -1))
                radianceBarAlphaTimer = 0;
            else if (radianceBarAlphaTimer < 20)
                radianceBarAlphaTimer++;

            if (hoveringOverSpecialTextTileCoords == new Vector2(-1, -1))
                hoveringOverSpecialTextTileAlphaTimer = 0;
            else if (hoveringOverSpecialTextTileAlphaTimer < 20)
                hoveringOverSpecialTextTileAlphaTimer++;
        }
    }
}