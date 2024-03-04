using Radiance.Content.Tiles;
using System.Collections.Generic;

namespace Radiance.Core
{
    public class RadianceGlobalItem : GlobalItem
    {
        public override void SetStaticDefaults()
        {
            RadianceSets.RadianceProjectorLensTexture[ItemID.SpecularFish] = "Radiance/Content/Tiles/Transmutator/SpecularFish_Transmutator";
            RadianceSets.RadianceProjectorLensID[ItemID.SpecularFish] = (int)ProjectorLensID.Fish;
            RadianceSets.RadianceProjectorLensDust[ItemID.SpecularFish] = DustID.FrostDaggerfish;
            RadianceSets.RadianceProjectorLensSound[ItemID.SpecularFish] = new SoundStyle($"{nameof(Radiance)}/Sounds/FishSplat");
            RadianceSets.RadianceProjectorLensPreOrderedUpdateFunction[ItemID.SpecularFish] = (projector) => projector.transmutator.radianceModifier *= 10f;

            RadianceSets.RadianceCellAbsorptionStats[ItemID.FallenStar] = (20, 1);
            RadianceSets.RadianceCellAbsorptionStats[ModContent.ItemType<GlowstalkItem>()] = (12, 1.5f);
        }
    }
}
