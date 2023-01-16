using Terraria;
using Terraria.ModLoader;

namespace Radiance.Core
{
    public class ModGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public int formationPickupTimer = 0;
        public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
        {
            if(formationPickupTimer > 0)
                formationPickupTimer--;
        }
    }
}
