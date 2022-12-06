using Terraria.ID;
using Radiance.Content.Items.RadianceCells;
using System;
using Terraria.ModLoader;
using Radiance.Content.Items.PedestalItems;
using Terraria;
using Radiance.Content.Items.ProjectorLenses;
using System.Linq;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Core.Systems
{
    public class TransmutationRecipeSystem : ModSystem
    {
        public static TransmutationRecipeSystem Instance;
        public TransmutationRecipeSystem()
        {
            Instance = this;
        }
        public class TransmutationRecipe
        {
            public int inputItem = 0;
            public int outputItem = 0;
            public int requiredRadiance = 0;
            public UnlockBoolean incomplete = UnlockBoolean.unlockedByDefault;
            public UnlockBoolean unlock = UnlockBoolean.unlockedByDefault;
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
        }
        public enum SpecialEffects
        {
            None,
            SummonRain,
            RemoveRain
        }
        public override void OnWorldLoad()
        {
            AddTransmutationRecipes();
        }
        public override void OnWorldUnload()
        {
            Array.Clear(transmutationRecipe);
        }
        public override void Unload()
        {
            if(!Main.dedServ)
                Instance = null;
        }
        public static void AddTransmutationRecipes()
        {
            #region Item Recipes
            AddRecipe(ModContent.ItemType<PoorRadianceCell>(), ModContent.ItemType<StandardRadianceCell>(), 100, "StandardRadianceCell");
            for (int i = 0; i < 6; i++)
            {
                int item = ItemID.Sapphire + i;
                AddRecipe(item, ModContent.ItemType<ShimmeringGlass>(), 5, item.ToString() + "Flareglass");
            }
            AddRecipe(ItemID.Amber, ModContent.ItemType<ShimmeringGlass>(), 5, "AmberFlareglass");

            AddRecipe(ItemID.SoulofLight, ModContent.ItemType<FormationCore>(), 100, "FormationCore", UnlockBoolean.hardmode, UnlockBoolean.hardmode, 3);
            AddRecipe(ItemID.SoulofNight, ModContent.ItemType<AnnihilationCore>(), 100, "AnnihilationCore", UnlockBoolean.hardmode, UnlockBoolean.hardmode, 3);
            #endregion

            #region Utility Recipes
            AddRecipe(ItemID.WaterCandle, ItemID.None, 20, "RainSummon", default, default, 1, 0, default, SpecialEffects.SummonRain);
            AddRecipe(ItemID.PeaceCandle, ItemID.None, 20, "RainStop", default, default, 1, 0, default, SpecialEffects.RemoveRain);
            #endregion
        }
        public static bool GetValueFromUnlockMethods(UnlockBoolean key)
        {
            UnlockMethods.TryGetValue(key, out var value);
            return value;
        }
        public static TransmutationRecipe FindRecipe(string id) => transmutationRecipe.FirstOrDefault(x => x.id == id) == default(TransmutationRecipe) ? null : transmutationRecipe.FirstOrDefault(x => x.id == id);
        public static void AddRecipe(int inputItem, int outputItem, int requiredRadiance, string id, UnlockBoolean incomplete = UnlockBoolean.unlockedByDefault, UnlockBoolean unlock = UnlockBoolean.unlockedByDefault, int inputStack = 1, int outputStack = 1, SpecialRequirements specialRequirement = SpecialRequirements.None, SpecialEffects specialEffect = SpecialEffects.None)
        {
            TransmutationRecipe recipe = new()
            {
                inputItem = inputItem,
                outputItem = outputItem,
                requiredRadiance = requiredRadiance,
                id = id,
                incomplete = incomplete,
                unlock = unlock,
                inputStack = inputStack,
                outputStack = outputStack,
                specialRequirements = specialRequirement,
                specialEffects = specialEffect
            };

            transmutationRecipe[numRecipes] = recipe;
            numRecipes++;
        }
    }
}
