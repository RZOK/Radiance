using Terraria.ID;
using Radiance.Content.Items.RadianceCells;
using System;
using Terraria.ModLoader;
using Radiance.Content.Items.PedestalItems;
using Terraria;
using Radiance.Content.Items.ProjectorLenses;
using System.Linq;
using static Radiance.Core.Systems.UnlockSystem;
using Radiance.Core.Encycloradia;
using Radiance.Content.Tiles;
using Radiance.Utilities;

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
            public UnlockBoolean unlock = UnlockBoolean.unlockedByDefault;
            public string id = string.Empty;
            public int inputStack = 0;
            public int outputStack = 0;
            public SpecialRequirements specialRequirements = SpecialRequirements.None;
            public SpecialEffects specialEffects = SpecialEffects.None;
            public float specialEffectValue = 0;
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
            RemoveRain,
            PotionDisperse
        }
        public override void Load()
        {
            AddTransmutationRecipes();
            EncycloradiaSystem.Instance.LoadEntries(); //entries have to be loaded here so that recipes are loaded for recipe pages that pull recipe data directly
        }
        public override void Unload()
        {
            Array.Clear(transmutationRecipe);
            if (!Main.dedServ)
                Instance = null;
        }
        public static void AddTransmutationRecipes()
        {
            #region Item Recipes
            for (int i = 0; i < ItemLoader.ItemCount; i++)
            {
                if (i <= 0 || i >= ItemLoader.ItemCount)
                    continue;

                Item item = RadianceUtils.GetItem(i);
                if (item.buffType > 0 && item.buffTime > 0 && item.consumable && item.maxStack > 1 && item.Name.Contains("Potion"))
                {
                    AddRecipe(item.type, ItemID.None, 200, item.Name + "Dispersal", UnlockBoolean.downedEvilBoss, 1, 0, SpecialRequirements.None, SpecialEffects.PotionDisperse, item.buffType);
                }
            }
            AddRecipe(ModContent.ItemType<PoorRadianceCell>(), ModContent.ItemType<StandardRadianceCell>(), 100, "StandardRadianceCell");
            for (int i = 0; i < 6; i++)
            {
                int item = ItemID.Sapphire + i;
                AddRecipe(item, ModContent.ItemType<ShimmeringGlass>(), 5, "Flareglass" + item.ToString());
            }
            AddRecipe(ItemID.Amber, ModContent.ItemType<ShimmeringGlass>(), 5, "AmberFlareglass");

            AddRecipe(ItemID.SoulofLight, ModContent.ItemType<OrchestrationCore>(), 100, "OrchestrationCore", UnlockBoolean.hardmode, 3);
            AddRecipe(ItemID.SoulofNight, ModContent.ItemType<AnnihilationCore>(), 100, "AnnihilationCore", UnlockBoolean.hardmode, 3);
            AddRecipe(ItemID.CursedFlame, ModContent.ItemType<FormationCore>(), 100, "FormationCoreCursedFlame", UnlockBoolean.hardmode, 3); //todo: recipe groups in transmutation recipes 
            AddRecipe(ItemID.Ichor, ModContent.ItemType<FormationCore>(), 100, "FormationCoreIchor", UnlockBoolean.hardmode, 3);
            #endregion

            #region Utility Recipes
            AddRecipe(ItemID.WaterCandle, ItemID.None, 20, "RainSummon", default, 1, 0, default, SpecialEffects.SummonRain);
            AddRecipe(ItemID.PeaceCandle, ItemID.None, 20, "RainStop", default, 1, 0, default, SpecialEffects.RemoveRain);
            #endregion
        }
        public static bool GetValueFromUnlockMethods(UnlockBoolean key)
        {
            UnlockMethods.TryGetValue(key, out var value);
            return value;
        }
        public static TransmutationRecipe FindRecipe(string id) => transmutationRecipe.FirstOrDefault(x => x.id == id) == default(TransmutationRecipe) ? null : transmutationRecipe.FirstOrDefault(x => x.id == id);
        public static void AddRecipe(int inputItem, int outputItem, int requiredRadiance, string id, UnlockBoolean unlock = UnlockBoolean.unlockedByDefault, int inputStack = 1, int outputStack = 1, SpecialRequirements specialRequirement = SpecialRequirements.None, SpecialEffects specialEffect = SpecialEffects.None, float specialEffectValue = 0)
        {
            TransmutationRecipe recipe = new()
            {
                inputItem = inputItem,
                outputItem = outputItem,
                requiredRadiance = requiredRadiance,
                id = id,
                unlock = unlock,
                inputStack = inputStack,
                outputStack = outputStack,
                specialRequirements = specialRequirement,
                specialEffects = specialEffect,
                specialEffectValue = specialEffectValue
            };

            transmutationRecipe[numRecipes] = recipe;
            numRecipes++;
        }
    }
}
