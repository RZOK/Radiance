using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace Radiance.Core
{
    public class RadianceInterfacePlayer : ModPlayer
    {
        public Vector2 radianceContainingTileHoverOverCoords = new(-1, -1);
        public float radianceBarAlphaTimer = 0;

        public Vector2 aoeCirclePosition = new(-1, -1);
        public Vector4 aoeCircleColor = new();
        public float aoeCircleScale = 0;
        public Matrix aoeCircleMatrix = Matrix.Identity;
        public float aoeCircleAlphaTimer = 0;

        public Vector2 hoveringOverSpecialTextTileCoords = new(-1, -1);
        public float hoveringOverSpecialTextTileAlphaTimer = 0;
        public string hoveringOverSpecialTextTileString = string.Empty;
        public string hoveringOverSpecialTextTileItemTagString = string.Empty;
        public Color hoveringOverSpecialTextTileColor = new();

        public Vector2 transmutatorIOCoords = new(-1, -1);
        public float transmutatorIOTimer = 0;

        public float newEntryUnlockedTimer = 0;
        public string incompleteEntryText = string.Empty;

        public override void ResetEffects()
        {
            radianceContainingTileHoverOverCoords = new Vector2(-1, -1);

            aoeCirclePosition = new Vector2(-1, -1);
            aoeCircleColor = new();
            aoeCircleScale = 0;
            aoeCircleMatrix = Matrix.Identity;

            hoveringOverSpecialTextTileCoords = new Vector2(-1, -1);
            hoveringOverSpecialTextTileString = string.Empty;
            hoveringOverSpecialTextTileItemTagString = string.Empty;
            hoveringOverSpecialTextTileColor = new();

            transmutatorIOCoords = new Vector2(-1, -1);

            incompleteEntryText = string.Empty;
        }

        public override void PostUpdate()
        {
            if (aoeCirclePosition == new Vector2(-1, -1))
                aoeCircleAlphaTimer = 0;
            else if (aoeCircleAlphaTimer < 20)
                aoeCircleAlphaTimer++;

            if (radianceContainingTileHoverOverCoords == new Vector2(-1, -1))
                radianceBarAlphaTimer = 0;
            else if (radianceBarAlphaTimer < 20)
                radianceBarAlphaTimer++;

            if (hoveringOverSpecialTextTileCoords == new Vector2(-1, -1))
                hoveringOverSpecialTextTileAlphaTimer = 0;
            else if (hoveringOverSpecialTextTileAlphaTimer < 20)
                hoveringOverSpecialTextTileAlphaTimer++;

            if (transmutatorIOCoords == new Vector2(-1, -1))
                transmutatorIOTimer = 0;
            else if (transmutatorIOTimer < 20)
                transmutatorIOTimer++;
        }
    }
}