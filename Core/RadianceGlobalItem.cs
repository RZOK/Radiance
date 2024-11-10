using Radiance.Content.Tiles.Transmutator;
using Radiance.Core.Systems;
using System.Collections.Generic;

namespace Radiance.Core
{
    public class RadianceGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public int formationPickupTimer = 0;

        public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().canSeeLensItems && RadianceSets.ProjectorLensID[item.type] != -1)
            {
                float slotScale = 0.7f;
                slotScale *= Main.inventoryScale + 0.05f * SineTiming(60);
                RadianceDrawing.DrawSoftGlow(Main.screenPosition + position, CommonColors.RadianceColor1 * (0.6f + 0.2f * SineTiming(60)), 0.3f);
                RadianceDrawing.DrawSoftGlow(Main.screenPosition + position, Color.White * (0.5f + 0.15f * SineTiming(60)), 0.2f);
            }
            return base.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        }
        public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
        {
            if (formationPickupTimer > 0)
                formationPickupTimer--;
        }
    }
}