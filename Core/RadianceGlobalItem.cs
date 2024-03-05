using Radiance.Content.Tiles.Transmutator;
using Radiance.Core.Systems;
using System.Collections.Generic;

namespace Radiance.Core
{
    public class RadianceGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public int formationPickupTimer = 0;

        public override void Load()
        {
            TransmutatorTileEntity.PostTransmutateItemEvent += GiveFishUnlock;
        }

        private void GiveFishUnlock(TransmutatorTileEntity transmutator, TransmutationRecipe recipe)
        {
            if (RadianceSets.ProjectorLensID[transmutator.projector.LensPlaced.type] == (int)ProjectorLensID.Fish)
                UnlockSystem.transmutatorFishUsed = true;
        }

        public override void SetStaticDefaults()
        {
            RadianceSets.ProjectorLensTexture[ItemID.SpecularFish] = "Radiance/Content/Tiles/Transmutator/SpecularFish_Transmutator";
            RadianceSets.ProjectorLensID[ItemID.SpecularFish] = (int)ProjectorLensID.Fish;
            RadianceSets.ProjectorLensDust[ItemID.SpecularFish] = DustID.FrostDaggerfish;
            RadianceSets.ProjectorLensSound[ItemID.SpecularFish] = new SoundStyle($"{nameof(Radiance)}/Sounds/FishSplat");
            RadianceSets.ProjectorLensPreOrderedUpdateFunction[ItemID.SpecularFish] = (projector) => projector.transmutator.radianceModifier *= 25f;
        }

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