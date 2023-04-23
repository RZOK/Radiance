using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Core.Systems
{
    public class RecipeGroupSystem : ModSystem
    {
        public override void AddRecipeGroups()
        {
            RecipeGroup group = new RecipeGroup(() => "Any Gold Bar", new int[]
            {
                ItemID.GoldBar,
                ItemID.PlatinumBar,
            });
            RecipeGroup.RegisterGroup("GoldGroup", group);
        }
    }
}
