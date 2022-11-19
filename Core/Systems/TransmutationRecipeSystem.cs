using Terraria.ID;
using Radiance.Content.Items.RadianceCells;
using System;
using Terraria.ModLoader;
using Radiance.Content.Items.PedestalItems;

namespace Radiance.Core.Systems
{
    public class TransmutationRecipeSystem : ModSystem
    {
        public static TransmutationRecipeSystem Instance;
        public class TransmutationRecipe
        {
            public int inputItem = 0;
            public int outputItem = 0;
            public int requiredRadiance = 0;
            public bool unlocked = true;
            public string id = string.Empty;
            public int inputStack = 0;
            public int outputStack = 0;
            public SpecialRequirements specialRequirements = SpecialRequirements.None;
            public SpecialEffects specialEffects = SpecialEffects.None;
        }
        public static TransmutationRecipe[] transmutationRecipe = new TransmutationRecipe[400];
        public static int numRecipes = 0;
        public enum SpecialRequirements
        {
            None
        };
        public enum SpecialEffects
        {
            None
        };
        public override void Load()
        {
            Instance = this;
            AddTransmutationRecipes();
        }
        public override void Unload()
        {
            Array.Clear(transmutationRecipe);
        }
        public void AddTransmutationRecipes()
        {
            //todo: unlock system doesn't actually work
            AddRecipe(ModContent.ItemType<PoorRadianceCell>(), ModContent.ItemType<StandardRadianceCell>(), 100, "StandardRadianceCell", true);

            AddRecipe(ItemID.SoulofLight, ModContent.ItemType<FormationCore>(), 100, "FormationCore", true, 3);
            AddRecipe(ItemID.SoulofNight, ModContent.ItemType<AnnihilationCore>(), 100, "AnnihilationCore", true, 3);
        }
        public void AddRecipe(int inputItem, int outputItem, int requiredRadiance, string id, bool unlocked, int inputStack = 1, int outputStack = 1, SpecialRequirements specialRequirement = SpecialRequirements.None, SpecialEffects specialEffect = SpecialEffects.None)
        {
            TransmutationRecipe recipe = new();

            recipe.inputItem = inputItem;
            recipe.outputItem = outputItem;
            recipe.requiredRadiance = requiredRadiance;
            recipe.id = id;
            recipe.unlocked = unlocked;
            recipe.inputStack = inputStack;
            recipe.outputStack = outputStack;
            recipe.specialRequirements = specialRequirement;
            recipe.specialEffects = specialEffect;

            transmutationRecipe[numRecipes] = recipe;
            numRecipes++;
        }
    }
}
