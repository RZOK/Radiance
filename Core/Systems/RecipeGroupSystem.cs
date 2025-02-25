using Terraria.Localization;

namespace Radiance.Core.Systems
{
    public class RecipeGroupSystem : ModSystem
    {
        private static readonly string LocalizationPrefix = "Mods.Radiance.RecipeGroups";
        private readonly LocalizedText SilverGroup;
        private readonly LocalizedText GoldGroup;
        public RecipeGroupSystem()
        {
            SilverGroup = Language.GetOrRegister($"{LocalizationPrefix}.{nameof(SilverGroup)}", () => "Any Silver Bar");
            GoldGroup = Language.GetOrRegister($"{LocalizationPrefix}.{nameof(GoldGroup)}", () => "Any Gold Bar");
        }
        public override void AddRecipeGroups()
        {

            RecipeGroup silverGroup = new RecipeGroup(() => SilverGroup.Value, CommonItemGroups.SilverBars);
            RecipeGroup.RegisterGroup(nameof(SilverGroup), silverGroup);

            RecipeGroup goldGroup = new RecipeGroup(() => GoldGroup.Value, CommonItemGroups.GoldBars);
            RecipeGroup.RegisterGroup(nameof(GoldGroup), goldGroup);
        }
    }
}
