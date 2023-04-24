using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Core.Systems
{
    public class RecipeGroupSystem : ModSystem
    {
        public override void AddRecipeGroups()
        {
            RecipeGroup silverGroup = new RecipeGroup(() => "Any Silver Bar", new int[]
            {
                ItemID.SilverBar,
                ItemID.TungstenBar,
            });
            RecipeGroup.RegisterGroup("SilverGroup", silverGroup);

            RecipeGroup goldGroup = new RecipeGroup(() => "Any Gold Bar", new int[]
            {
                ItemID.GoldBar,
                ItemID.PlatinumBar,
            });
            RecipeGroup.RegisterGroup("GoldGroup", goldGroup);
        }
    }
}
