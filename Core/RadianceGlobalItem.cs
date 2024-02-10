using System.Collections.Generic;

namespace Radiance.Core
{
    public class RadianceGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public int formationPickupTimer = 0;
        public override void SetStaticDefaults()
        {
            RadianceSets.RadianceProjectorLensTexture[ItemID.SpecularFish] = "Radiance/Content/Tiles/Transmutator/SpecularFish_Transmutator";
            RadianceSets.RadianceProjectorLensID[ItemID.SpecularFish] = (int)ProjectorLensID.Fish;
            RadianceSets.RadianceProjectorLensDust[ItemID.SpecularFish] = DustID.FrostDaggerfish;
            RadianceSets.RadianceProjectorLensSound[ItemID.SpecularFish] = new SoundStyle($"{nameof(Radiance)}/Sounds/FishSplat");
            RadianceSets.RadianceProjectorLensPreOrderedUpdateFunction[ItemID.SpecularFish] = (projector) => projector.transmutator.radianceModifier *= 10f;
        }
        public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
        {
            if (formationPickupTimer > 0)
                formationPickupTimer--;
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (item.ModItem is IInstrument)
            {
                TooltipLine line = new TooltipLine(item.ModItem.Mod, "InstrumentAlert", "Consumes Radiance from cells in your inventory");
                int index = tooltips.FindIndex(x => x.Name == "Tooltip0" && x.Mod == "Terraria");
                if(index > -1) 
                    tooltips.Insert(index, line);
            }
        }
    }
}
